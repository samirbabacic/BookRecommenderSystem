﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookRecommenderSystem.Entities
{
    public class BaseDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}
