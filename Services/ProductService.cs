using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartMiniStore.Models;

namespace SmartMiniStore.Services
{
    /// <summary>
    /// All product-related database operations.
    /// </summary>
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;

        public ProductService(MongoDbService mongo)
        {
            _products = mongo.Products;
        }

        // ---------- READ ----------

        public async Task<List<Product>> GetAllAsync()
        {
            return await _products
                .Find(Builders<Product>.Filter.Empty)
                .SortBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<Product>> SearchByNameAsync(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return await GetAllAsync();

            var filter = Builders<Product>.Filter.Regex(
                p => p.Name,
                new BsonRegularExpression(search, "i")); // "i" = case-insensitive

            return await _products
                .Find(filter)
                .SortBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return null;
            return await _products
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Product>> GetLowStockAsync()
        {
            // Quantity <= MinimumStock, done server-side.
            var filter = Builders<Product>.Filter
                .Where(p => p.Quantity <= p.MinimumStock);

            return await _products
                .Find(filter)
                .SortBy(p => p.Quantity)
                .ToListAsync();
        }

        public async Task<long> CountAsync()
        {
            return await _products.CountDocumentsAsync(Builders<Product>.Filter.Empty);
        }

        // ---------- WRITE ----------

        public async Task AddAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(product.Id))
                product.Id = ObjectId.GenerateNewId().ToString();

            if (product.CreatedAt == default)
                product.CreatedAt = DateTime.UtcNow;

            await _products.InsertOneAsync(product);
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (!ObjectId.TryParse(product.Id, out _)) return false;

            var result = await _products.ReplaceOneAsync(
                p => p.Id == product.Id,
                product);

            return result.ModifiedCount > 0 || result.MatchedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return false;

            var result = await _products.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        // ---------- HELPERS USED BY OTHER SERVICES ----------

        public async Task<bool> AnyInCategoryAsync(string categoryId)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.CategoryId, categoryId);
            var count = await _products.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task UpdateCategoryNameForAllAsync(string categoryId, string newCategoryName)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.CategoryId, categoryId);
            var update = Builders<Product>.Update.Set(p => p.CategoryName, newCategoryName);
            await _products.UpdateManyAsync(filter, update);
        }

        /// <summary>
        /// Atomically decrement stock only if the product still has enough.
        /// Returns true if the decrement happened, false if the stock was insufficient.
        /// </summary>
        public async Task<bool> TryDecrementStockAsync(string productId, int quantity)
        {
            if (quantity <= 0) return false;

            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.Id, productId),
                Builders<Product>.Filter.Gte(p => p.Quantity, quantity));

            var update = Builders<Product>.Update.Inc(p => p.Quantity, -quantity);

            var result = await _products.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        /// <summary>
        /// Adds stock back to a product. Used as a rollback if a sale
        /// insert fails after the stock decrement already happened.
        /// </summary>
        public async Task AddStockAsync(string productId, int quantity)
        {
            if (quantity <= 0) return;

            var update = Builders<Product>.Update.Inc(p => p.Quantity, quantity);
            await _products.UpdateOneAsync(p => p.Id == productId, update);
        }
    }
}