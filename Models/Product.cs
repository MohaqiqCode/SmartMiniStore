using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartMiniStore.Models
{
    /// <summary>
    /// One product in the store, e.g. "Coca Cola".
    /// Stored in the "Products" collection in MongoDB.
    /// </summary>
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("Name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("CategoryId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; } = string.Empty;

        [BsonElement("CategoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [BsonElement("Price")]
        public decimal Price { get; set; }

        [BsonElement("Quantity")]
        public int Quantity { get; set; }

        [BsonElement("MinimumStock")]
        public int MinimumStock { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Convenience property, NOT stored in MongoDB.
        /// True when the product stock has fallen to or below the minimum.
        /// </summary>
        [BsonIgnore]
        public bool IsLowStock => Quantity <= MinimumStock;

        /// <summary>
        /// Convenience property, NOT stored in MongoDB.
        /// Readable status text for the UI.
        /// </summary>
        [BsonIgnore]
        public string StockStatus => IsLowStock ? "Low Stock" : "OK";
    }
}