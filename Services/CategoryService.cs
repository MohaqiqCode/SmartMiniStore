using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartMiniStore.Models;

namespace SmartMiniStore.Services
{
    /// <summary>
    /// All category-related database operations, including
    /// duplicate-name prevention and safe delete.
    /// </summary>
    public class CategoryService
    {
        private readonly IMongoCollection<Category> _categories;
        private readonly ProductService _productService;

        public CategoryService(MongoDbService mongo, ProductService productService)
        {
            _categories = mongo.Categories;
            _productService = productService;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _categories
                .Find(Builders<Category>.Filter.Empty)
                .SortBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Category>> SearchByNameAsync(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return await GetAllAsync();

            var filter = Builders<Category>.Filter.Regex(
                c => c.Name,
                new BsonRegularExpression(search, "i"));

            return await _categories
                .Find(filter)
                .SortBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return null;
            return await _categories
                .Find(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<long> CountAsync()
        {
            return await _categories.CountDocumentsAsync(
                Builders<Category>.Filter.Empty);
        }

        /// <summary>
        /// Returns true if a category with the given name already exists,
        /// optionally ignoring a specific id (used when editing).
        /// </summary>
        public async Task<bool> NameExistsAsync(string name, string? ignoreId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            var filter = Builders<Category>.Filter.Regex(
                c => c.Name,
                new BsonRegularExpression("^" + System.Text.RegularExpressions.Regex.Escape(name.Trim()) + "$", "i"));

            var matches = await _categories.Find(filter).ToListAsync();

            foreach (var m in matches)
            {
                if (ignoreId != null && m.Id == ignoreId) continue;
                return true;
            }
            return false;
        }

        public async Task AddAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            if (string.IsNullOrWhiteSpace(category.Id))
                category.Id = ObjectId.GenerateNewId().ToString();

            await _categories.InsertOneAsync(category);
        }

        /// <summary>
        /// Updates the category and, if the name changed,
        /// also updates the denormalized CategoryName on all related products.
        /// </summary>
        public async Task<bool> UpdateAsync(Category updated)
        {
            if (updated == null) throw new ArgumentNullException(nameof(updated));
            if (!ObjectId.TryParse(updated.Id, out _)) return false;

            var existing = await GetByIdAsync(updated.Id);
            if (existing == null) return false;

            var result = await _categories.ReplaceOneAsync(
                c => c.Id == updated.Id,
                updated);

            bool nameChanged = !string.Equals(existing.Name, updated.Name, StringComparison.Ordinal);
            if (nameChanged)
            {
                await _productService.UpdateCategoryNameForAllAsync(updated.Id, updated.Name);
            }

            return result.ModifiedCount > 0 || result.MatchedCount > 0;
        }

        /// <summary>
        /// Deletes a category only if no product uses it.
        /// Returns (success, message).
        /// </summary>
        public async Task<(bool success, string message)> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return (false, "Invalid category id.");

            bool inUse = await _productService.AnyInCategoryAsync(id);
            if (inUse)
                return (false, "This category is used by one or more products and cannot be deleted.");

            var result = await _categories.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0
                ? (true, "Category deleted.")
                : (false, "Category was not found.");
        }
    }
}