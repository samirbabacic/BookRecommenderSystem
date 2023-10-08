using BookRecommenderSystem.Entities;

namespace BookRecommenderSystem.DTOs
{
    public class BookDTO
    {
        public BookDTO(Book book)
        {
            Id = book.Id;
            Title = book.Title;
            Author = book.Author;
            YearOfPublication = book.YearOfPublication;
            Publisher = book.Publisher;
            ImageUrl = book.ImageUrl;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int YearOfPublication { get; set; }
        public string Publisher { get; set; }
        public string ImageUrl { get; set; }
    }
}
