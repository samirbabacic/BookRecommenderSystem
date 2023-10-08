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

        public BookService(IRepository<Book> bookRepository)
        {
            _bookRepository = bookRepository;
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

            return new BookDTO(book);
        }
    }
}
