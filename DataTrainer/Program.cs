using BookRecommenderSystem.Entities;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using MongoDB.Driver;
using RecommendationMLDataService.Models;
using Tensorflow;
using BookRating = RecommendationMLDataService.Models.BookRating;

namespace RecommendationMLDataService
{
    internal class Program
    {
        private static List<Book> books = new();

        private static IMongoDatabase _db;
        private static string MongoConnectionString;

        static async Task Main(string[] args)
        {
            MongoConnectionString = "mongodb://127.0.0.1:27017";

            _db = new MongoClient(MongoConnectionString).GetDatabase("library");

            ReadAndWrite();

            MLContext mlContext = new MLContext();
            (IDataView trainingDataView, IDataView testDataView) = LoadData(mlContext);

            ITransformer model = BuildAndTrainModel(mlContext, trainingDataView);
            EvaluateModel(mlContext, testDataView, model);

            await UseModelForSinglePrediction(mlContext, model);

            SaveModel(mlContext, trainingDataView.Schema, model);

            Console.WriteLine("Done");
        }

        static (IDataView training, IDataView test) LoadData(MLContext mlContext)
        {
            var trainingDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "BX-Book-Ratings.csv");
            var testDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "BX-Book-Ratings-train.csv");

            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<BookRating>(trainingDataPath, hasHeader: true, separatorChar: ',');
            IDataView testDataView = mlContext.Data.LoadFromTextFile<BookRating>(testDataPath, hasHeader: true, separatorChar: ',');

            return (trainingDataView, testDataView);
        }
        static ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainingDataView)
        {
            IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: "UserId")
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "bookIdEncoded", inputColumnName: "BookId"));

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "userIdEncoded",
                MatrixRowIndexColumnName = "bookIdEncoded",
                LabelColumnName = "Rating",
                NumberOfIterations = 100,
                ApproximationRank = 100
            };

            var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));
            Console.WriteLine("=============== Training the model ===============");
            ITransformer model = trainerEstimator.Fit(trainingDataView);

            return model;
        }
        static void EvaluateModel(MLContext mlContext, IDataView testDataView, ITransformer model)
        {
            Console.WriteLine("=============== Evaluating the model ===============");
            var prediction = model.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "Rating", scoreColumnName: "Score");
        }
        static async Task UseModelForSinglePrediction(MLContext mlContext, ITransformer model)
        {
            Console.WriteLine("=============== Making a prediction ===============");
            var predictionEngine = mlContext.Model.CreatePredictionEngine<BookRating, BookRatingPrediction>(model);

            var userCollection = _db.GetCollection<User>(nameof(User));

            var userCollectionCursor = await userCollection.FindAsync(Builders<User>.Filter.Empty, new FindOptions<User>
            {
                BatchSize = 500
            });

            while (await userCollectionCursor.MoveNextAsync())
            {
                foreach (var user in userCollectionCursor.Current)
                {
                    int userId = user.ProvisionId;

                    UserRecommendedBook[] topRecommendedBooks = new UserRecommendedBook[10];

                    (float Score, int Index) lowestRecommended = (9999, 0);

                    for (int i = 0; i < 10; i++)
                    {
                        var predict = predictionEngine.Predict(new BookRating { BookId = books[i].ProvisionId, UserId = userId });

                        if (float.IsNaN(predict.Score))
                        {
                            predict.Score = -1;
                        }

                        if (lowestRecommended.Score > predict.Score)
                        {
                            lowestRecommended = (predict.Score, i);
                        }

                        topRecommendedBooks[i] = new() { BookId = books[i].Id, Score = lowestRecommended.Score };
                    }

                    for (int i = 10; i < books.Count; i++)
                    {
                        var predict = predictionEngine.Predict(new BookRating { BookId = books[i].ProvisionId, UserId = userId });

                        if (float.IsNaN(predict.Score))
                            continue;

                        if (lowestRecommended.Score >= predict.Score)
                            continue;

                        topRecommendedBooks[lowestRecommended.Index] = new UserRecommendedBook { BookId = books[i].Id, Score = predict.Score };

                        lowestRecommended.Score = topRecommendedBooks[0].Score;
                        lowestRecommended.Index = 0;

                        for (int j = 1; j < topRecommendedBooks.Length; j++)
                        {
                            if (topRecommendedBooks[j].Score < lowestRecommended.Score)
                            {
                                lowestRecommended.Index = j;
                                lowestRecommended.Score = topRecommendedBooks[j].Score;
                            }
                        }
                    }

                    var recommendedBooksIds = topRecommendedBooks.Where(x => x.Score > -1)
                        .OrderByDescending(x => x.Score).Select(x => x.BookId).ToList();

                    if (recommendedBooksIds.Count == 0)
                    {
                        continue;
                    }

                    var userInDb = userCollection.AsQueryable().FirstOrDefault(x => x.ProvisionId == userId);

                    if (userInDb == null)
                        continue;

                    userInDb.RecommendedBooksIds = recommendedBooksIds;

                    userCollection.FindOneAndReplace(Builders<User>.Filter.Eq(x => x.ProvisionId, userId), userInDb);
                }
            }
        }

        static void SaveModel(MLContext mlContext, DataViewSchema trainingDataViewSchema, ITransformer model)
        {
            var modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "MovieRecommenderModel.zip");

            Console.WriteLine("=============== Saving the model to a file ===============");
            mlContext.Model.Save(model, trainingDataViewSchema, modelPath);
        }


        private static void ReadAndWrite()
        {
            string inputFilePath = Path.Combine("Data", "BX-Books.csv");

            var bookCollection = _db.GetCollection<Book>(nameof(Book));
            // Read input CSV file line by line and write to output CSV file
            using (var reader = new StreamReader(inputFilePath))
            {
                //header skip
                var line = reader.ReadLine();

                while (true)
                {
                    line = reader.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        break;

                    string[] values = line.Replace("\"", "").Split(";");

                    Book book = new Book
                    {
                        ProvisionId = values[0],
                        Title = values[1],
                        Author = values[2],
                        YearOfPublication = Int32.TryParse(values[3], out int yearParsed) ? yearParsed : 0,
                        Publisher = values[4],
                        ImageUrl = values[7]
                    };

                    bookCollection.InsertOne(book);

                    books.Add(book);
                }
            }

            inputFilePath = Path.Combine("Data", "BX-Book-Ratings.csv");

            var userCollection = _db.GetCollection<User>(nameof(User));
            var bookRatingCollection = _db.GetCollection<BookRecommenderSystem.Entities.BookRating>(nameof(BookRecommenderSystem.Entities.BookRating));
            // Read input CSV file line by line and write to output CSV file
            using (var reader = new StreamReader(inputFilePath))
            {
                //header skip
                var line = reader.ReadLine();

                while (true)
                {
                    line = reader.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        break;

                    string[] values = line.Split(",");

                    if (values.Length != 3)
                        continue;

                    if (!Int32.TryParse(values[0], out int userId) || !Int32.TryParse(values[2], out int rating))
                        continue;

                    BookRecommenderSystem.Entities.BookRating dbBookRating = new()
                    {
                        UserProvisionnedId = userId,
                        BookProvisionnedId = values[1],
                        Rating = rating
                    };

                    bookRatingCollection.InsertOne(dbBookRating);

                    var user = userCollection.AsQueryable().FirstOrDefault(x => x.ProvisionId == dbBookRating.UserProvisionnedId);

                    if (user != null)
                        continue;

                    user = new User()
                    {
                        ProvisionId = userId,
                        Password = "password"
                    };

                    userCollection.InsertOne(user);
                }
            }

            inputFilePath = Path.Combine("Data", "BX-Users.csv");

        }
    }
}