using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShoppingApi.Models
{
    public class ShoppingItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // Use string to store ObjectId

        public string Name { get; set; }
        public bool IsComplete { get; set; }
        public int Position { get; set; }
    }
}
