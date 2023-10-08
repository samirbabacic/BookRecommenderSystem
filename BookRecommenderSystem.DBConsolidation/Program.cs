using MongoDB.Driver;

namespace BookRecommenderSystem.DBConsolidation
{
    internal class Program
    {
        private static IMongoDatabase _db;
        private static string MongoConnectionString;

        static void Main(string[] args)
        {
            MongoConnectionString = "mongodb://127.0.0.1:27017";

            _db = new MongoClient(MongoConnectionString).GetDatabase("library");
        }
    }
}