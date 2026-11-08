using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SmartMiniStore.Models;

namespace SmartMiniStore.Services
{
    /// <summary>
    /// Inserts demonstration sample data exactly once.
    /// Does not duplicate if data already exists.
    /// </summary>
    public class SeedDataService
    {
        private readonly MongoDbService _mongo;
        private readonly CategoryService _categoryService;
        private readonly ProductService _productService;

        public SeedDataService(
            MongoDbService mongo,
            CategoryService categoryService,
            ProductService productService)
        {
            _mongo = mongo;
            _categoryService = categoryService;
            _productService = productService;
        }

        /// <summary>
        /// Seeds categories and products if they are missing.
        /// Returns a short human-readable summary.
        /// </summary>
        public async Task<string> SeedIfEmptyAsync()
        {
            int categoriesAdded = 0;
            int productsAdded = 0;

            // 1) Categories
            var existingCategories = await _categoryService.GetAllAsync();
            var categoryByName = existingCategories
                .ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase);

            var seedCategories = new List<Category>
            {
                new() { Name = "Beverages",     Description = "Soft drinks, juices, water" },
                new() { Name = "Food",          Description = "Rice, bread, cooking items" },
                new() { Name = "Snacks",        Description = "Biscuits, chips, nuts" },
                new() { Name = "Dairy",         Description = "Milk and dairy products" },
                new() { Name = "Cleaning",      Description = "Soap, detergent, cleaning" },
                new() { Name = "Personal Care", Description = "Toothpaste, shampoo, etc." }
            };

            foreach (var c in seedCategories)
            {
                if (!categoryByName.ContainsKey(c.Name))
                {
                    await _categoryService.AddAsync(c);
                    categoryByName[c.Name] = c;
                    categoriesAdded++;
                }
            }

            // 2) Products
            var existingProducts = await _productService.GetAllAsync();
            var existingProductNames = existingProducts
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // helper to look up a category id safely
            string Cat(string name) =>
                categoryByName.TryGetValue(name, out var c) ? c.Id : string.Empty;

            var seedProducts = new List<Product>
            {
                new() { Name = "Coca Cola",    CategoryId = Cat("Beverages"),     CategoryName = "Beverages",     Price = 30m,   Quantity = 40, MinimumStock = 5 },
                new() { Name = "Pepsi",        CategoryId = Cat("Beverages"),     CategoryName = "Beverages",     Price = 30m,   Quantity = 25, MinimumStock = 5 },
                new() { Name = "Water 1.5L",   CategoryId = Cat("Beverages"),     CategoryName = "Beverages",     Price = 15m,   Quantity = 60, MinimumStock = 10 },
                new() { Name = "Bread",        CategoryId = Cat("Food"),          CategoryName = "Food",          Price = 20m,   Quantity = 30, MinimumStock = 5 },
                new() { Name = "Rice 5kg",     CategoryId = Cat("Food"),          CategoryName = "Food",          Price = 450m,  Quantity = 12, MinimumStock = 3 },
                new() { Name = "Biscuits",     CategoryId = Cat("Snacks"),        CategoryName = "Snacks",        Price = 25m,   Quantity = 50, MinimumStock = 10 },
                new() { Name = "Milk 1L",      CategoryId = Cat("Dairy"),         CategoryName = "Dairy",         Price = 70m,   Quantity = 18, MinimumStock = 6 },
                new() { Name = "Soap",         CategoryId = Cat("Cleaning"),      CategoryName = "Cleaning",      Price = 40m,   Quantity = 22, MinimumStock = 5 },
                new() { Name = "Shampoo",      CategoryId = Cat("Personal Care"), CategoryName = "Personal Care", Price = 180m,  Quantity = 8,  MinimumStock = 3 },
                new() { Name = "Toothpaste",   CategoryId = Cat("Personal Care"), CategoryName = "Personal Care", Price = 90m,   Quantity = 10, MinimumStock = 4 }
            };

            foreach (var p in seedProducts)
            {
                if (string.IsNullOrWhiteSpace(p.CategoryId))
                    continue; // category missing, skip

                if (!existingProductNames.Contains(p.Name))
                {
                    p.Id = ObjectId.GenerateNewId().ToString();
                    p.CreatedAt = DateTime.UtcNow;
                    await _productService.AddAsync(p);
                    productsAdded++;
                }
            }

            if (categoriesAdded == 0 && productsAdded == 0)
                return "Sample data already exists. Nothing was inserted.";

            return $"Added {categoriesAdded} categor{(categoriesAdded == 1 ? "y" : "ies")} and {productsAdded} product{(productsAdded == 1 ? "" : "s")}.";
        }
    }
}