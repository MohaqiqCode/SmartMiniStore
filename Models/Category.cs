using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartMiniStore.Models
{
    /// <summary>
    /// One product category, e.g. "Beverages".
    /// Stored in the "Categories" collection in MongoDB.
    /// </summary>
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("Name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("Description")]
        public string Description { get; set; } = string.Empty;
    }
}