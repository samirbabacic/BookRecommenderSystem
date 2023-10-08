using BookRecommenderSystem.DTOs;
using BookRecommenderSystem.Entities;
using BookRecommenderSystem.Repository;

namespace BookRecommenderSystem.Services
{
    public interface IBookService
    {
        List<BookDTO> GetBooks(int pageNumber, int pageSize);
        BookDTO GetById(string id);
    }

    public class BookService : IBookService
    {
        private readonly IRepository<Book> _bookRepository;
        private readonly IRepository<BookRating> _bookRatingRepository;

        public BookService(IRepository<Book> bookRepository, IRepository<BookRating> bookRatingRepository)
        {
            _bookRepository = bookRepository;
            _bookRatingRepository = bookRatingRepository;
        }

        public List<BookDTO> GetBooks(int pageNumber, int pageSize)
        {
            var books = _bookRepository.GetPaged(pageNumber, pageSize);

            List<BookDTO> booksDtos = new();

            foreach (var book in books)
            {
                booksDtos.Add(new BookDTO(book));
            }

            return booksDtos;
        }

        public BookDTO GetById(string bookId)
        {
            var book = _bookRepository.FirstOrDefault(x => x.Id == bookId);

            var bookRatings = _bookRatingRepository.Get(x => x.BookProvisionnedId == book.ProvisionId);

            var bookRatingsDtos = bookRatings.Select(x => new BookDTO.BookRatedValueDTO
            {
                Description = x.Description,
                UserProvisionId = x.UserProvisionnedId,
                Rating = x.Rating,
            }).ToList();

            return new BookDTO(book)
            {
                Ratings = bookRatingsDtos
            };
        }
    }
}
