using BookRecommenderSystem.DTOs;
using BookRecommenderSystem.Entities;
using BookRecommenderSystem.Repository;
using DnsClient;

namespace BookRecommenderSystem.Services
{
    public interface IUserService
    {
        Task<UserDTO> GetAsync(string id);
        Task<UserDTO> RateBook(string userId, string bookId, int rating);
        Task<UserDTO> OrderBook(string userId, string bookId);
    }

    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Book> _bookRepository;
        private readonly IRepository<BookRating> _userBookRatingRepo;

        public UserService(IRepository<User> userRepository, IRepository<Book> bookRepository, IRepository<BookRating> userBookRatingRepo)
        {
            _userRepository = userRepository;
            _bookRepository = bookRepository;
            _userBookRatingRepo = userBookRatingRepo;
        }

        public async Task<UserDTO> GetAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            var booksOrdered = _bookRepository.Get(x => user.OrderedBooksIds.Contains(x.Id));
            var recommendedBooks = _bookRepository.Get(x => user.RecommendedBooksIds.Contains(x.Id));

            List<BookDTO> booksOrderedDtos = new List<BookDTO>();

            foreach (var bookOrdered in booksOrdered)
            {
                booksOrderedDtos.Add(new BookDTO(bookOrdered));
            }

            List<BookDTO> recommendedBooksDtos = new List<BookDTO>();

            foreach (var recommendedBook in recommendedBooks)
            {
                recommendedBooksDtos.Add(new BookDTO(recommendedBook));
            }

            List<UserBookRatingDTO> ratedBooksDtos = new();

            var userBooksRated = _userBookRatingRepo.Get(x => x.UserProvisionnedId == user.ProvisionId);

            var userBooksRatedProvisionIds = userBooksRated.Select(x => x.BookProvisionnedId).ToList();
            var booksRated = _bookRepository.Get(x => userBooksRatedProvisionIds.Contains(x.ProvisionId)).ToDictionary(x => x.ProvisionId, x => x);

            foreach (var userBookRating in userBooksRated)
            {
                if (!booksRated.ContainsKey(userBookRating.BookProvisionnedId))
                    continue;

                var bookRated = booksRated[userBookRating.BookProvisionnedId];

                ratedBooksDtos.Add(new UserBookRatingDTO { Book = new BookDTO(bookRated), Rating = userBookRating.Rating });
            }

            return new UserDTO
            {
                Id = user.Id,
                ProvisionId = user.ProvisionId,
                BooksRated = ratedBooksDtos,
                OrderedBooks = booksOrderedDtos,
                RecommendedBooks = recommendedBooksDtos,
            };
        }

        public async Task<UserDTO> OrderBook(string userId, string bookId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new Exception("User does not exist");

            user.OrderedBooksIds.Add(bookId);
            await _userRepository.ReplaceAsync(user.Id, user);

            return await GetAsync(userId);
        }

        public async Task<UserDTO> RateBook(string userId, string bookId, int rating)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new Exception("User does not exist");

            var bookToBeRated = _bookRepository.FirstOrDefault(x => x.Id == bookId);

            var userBookRating = _userBookRatingRepo.FirstOrDefault(x => x.UserProvisionnedId == user.ProvisionId && x.BookProvisionnedId == bookToBeRated.ProvisionId);

            if (userBookRating == null)
            {
                await _userBookRatingRepo.CreateAsync(new BookRating
                {
                    BookProvisionnedId = bookToBeRated.ProvisionId,
                    UserProvisionnedId = user.ProvisionId,
                    Rating = rating
                });

                return await GetAsync(userId);
            }

            await _userBookRatingRepo.ReplaceAsync(userBookRating.Id, new BookRating
            {
                BookProvisionnedId = bookToBeRated.ProvisionId,
                UserProvisionnedId = user.ProvisionId,
                Rating = rating
            });

            return await GetAsync(userId);
        }
    }
}
