using MongoDB.Bson.Serialization.Attributes;

namespace BookRecommenderSystem.Entities
{
    [BsonIgnoreExtraElements]
    public class Book : BaseDocument
    {
        public string ProvisionId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int YearOfPublication { get; set; }
        public string Publisher { get; set; }
        public string ImageUrl { get; set; }
    }
}
