namespace BookRecommenderSystem.Requests.User
{
    public class RateBookRequest
    {
        public string UserId { get; set; }
        public string BookId { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
    }
}
