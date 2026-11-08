using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartMiniStore.Models
{
    /// <summary>
    /// One sale transaction. Each sale = one product line.
    /// Stored in the "Sales" collection in MongoDB.
    /// </summary>
    public class Sale
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("ProductId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; } = string.Empty;

        [BsonElement("ProductName")]
        public string ProductName { get; set; } = string.Empty;

        [BsonElement("Quantity")]
        public int Quantity { get; set; }

        [BsonElement("UnitPrice")]
        public decimal UnitPrice { get; set; }

        [BsonElement("TotalPrice")]
        public decimal TotalPrice { get; set; }

        [BsonElement("SaleDate")]
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Convenience property for the UI. Not stored in MongoDB.
        /// </summary>
        [BsonIgnore]
        public string TotalDisplay => $"{TotalPrice:N0} AFN";
    }
}