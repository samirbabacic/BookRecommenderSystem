using MongoDB.Bson.Serialization.Attributes;

namespace BookRecommenderSystem.Entities
{
    [BsonIgnoreExtraElements]
    public class User : BaseDocument
    {
        public string Password { get; set; }

        private List<string> _orderedBookIds = new List<string>();
        public List<string> OrderedBooksIds
        {
            get
            {
                if (_orderedBookIds == null)
                {
                    _orderedBookIds = new();
                }

                return _orderedBookIds;
            }
            set { _orderedBookIds = value; }
        }

        private List<string> _recommendedBooksIds = new List<string>();
        public List<string> RecommendedBooksIds
        {
            get
            {
                if (_recommendedBooksIds == null)
                {
                    _recommendedBooksIds = new();
                }

                return _recommendedBooksIds;
            }
            set { _recommendedBooksIds = value; }
        }

        public int ProvisionId { get; set; }
    }
}
