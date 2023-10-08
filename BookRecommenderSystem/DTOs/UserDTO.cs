namespace BookRecommenderSystem.DTOs
{
    public record UserDTO
    {
        public string Id { get; init; }
        public int ProvisionId { get; init; }
        public List<BookDTO> RecommendedBooks { get; init; } = new List<BookDTO>();
        public List<BookDTO> OrderedBooks { get; init; } = new List<BookDTO>();
        public List<UserBookRatingDTO> BooksRated { get; init; } = new List<UserBookRatingDTO>();
    }

    public record UserBookRatingDTO
    {
        public int Rating { get; set; }
        public BookDTO Book { get; set; }
    }
}
