using MongoDB.Bson.Serialization.Attributes;

namespace BookRecommenderSystem.Entities
{
    [BsonIgnoreExtraElements]
    public class BookRating : BaseDocument
    {
        public int UserProvisionnedId { get; set; }
        public string BookProvisionnedId { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }
    }
}
