using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartMiniStore.Models;

namespace SmartMiniStore.Services
{
    /// <summary>
    /// Sales operations. Creating a sale must:
    ///   1. validate the product and quantity,
    ///   2. atomically decrement the product stock,
    ///   3. insert the sale document.
    /// </summary>
    public class SaleService
    {
        private readonly IMongoCollection<Sale> _sales;
        private readonly ProductService _productService;

        public SaleService(MongoDbService mongo, ProductService productService)
        {
            _sales = mongo.Sales;
            _productService = productService;
        }

        public async Task<List<Sale>> GetAllAsync()
        {
            return await _sales
                .Find(Builders<Sale>.Filter.Empty)
                .SortByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<List<Sale>> SearchByProductNameAsync(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return await GetAllAsync();

            var filter = Builders<Sale>.Filter.Regex(
                s => s.ProductName,
                new BsonRegularExpression(search, "i"));

            return await _sales
                .Find(filter)
                .SortByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<List<Sale>> GetByDateAsync(DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            var filter = Builders<Sale>.Filter.And(
                Builders<Sale>.Filter.Gte(s => s.SaleDate, start),
                Builders<Sale>.Filter.Lt(s => s.SaleDate, end));

            return await _sales
                .Find(filter)
                .SortByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<long> CountAllAsync()
        {
            return await _sales.CountDocumentsAsync(Builders<Sale>.Filter.Empty);
        }

        public async Task<long> CountTodayAsync()
        {
            var start = DateTime.UtcNow.Date;
            var end = start.AddDays(1);

            var filter = Builders<Sale>.Filter.And(
                Builders<Sale>.Filter.Gte(s => s.SaleDate, start),
                Builders<Sale>.Filter.Lt(s => s.SaleDate, end));

            return await _sales.CountDocumentsAsync(filter);
        }

        public async Task<decimal> TotalRevenueAsync()
        {
            var all = await _sales.Find(Builders<Sale>.Filter.Empty).ToListAsync();
            decimal total = 0m;
            foreach (var s in all) total += s.TotalPrice;
            return total;
        }

        public async Task<decimal> TodayRevenueAsync()
        {
            var today = await GetByDateAsync(DateTime.UtcNow.Date);
            decimal total = 0m;
            foreach (var s in today) total += s.TotalPrice;
            return total;
        }

        /// <summary>
        /// Creates a sale and decreases product stock.
        /// Returns (success, message).
        /// </summary>
        public async Task<(bool success, string message)> CreateSaleAsync(string productId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(productId))
                return (false, "Please select a product.");

            if (quantity <= 0)
                return (false, "Quantity must be greater than zero.");

            if (!ObjectId.TryParse(productId, out _))
                return (false, "Invalid product id.");

            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
                return (false, "Product not found.");

            if (product.Quantity < quantity)
                return (false,
                    $"Not enough stock. Available: {product.Quantity}, requested: {quantity}.");

            // 1) Atomically decrement stock. If someone else already took the last
            //    units between our read and this call, this returns false.
            bool decremented = await _productService
                .TryDecrementStockAsync(productId, quantity);

            if (!decremented)
                return (false, "Stock changed. Please refresh and try again.");

            // 2) Insert the sale document.
            var sale = new Sale
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * quantity,
                SaleDate = DateTime.UtcNow
            };

            try
            {
                await _sales.InsertOneAsync(sale);
            }
            catch
            {
                // If insert fails, roll the stock back.
                var rollback = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
                // We can't call a private method, so use ProductService to add back via a small helper.
                await _productService.AddStockAsync(product.Id, quantity);
                return (false, "Sale could not be saved. Stock was restored.");
            }

            return (true, $"Sale recorded: {sale.TotalPrice:N0} AFN");
        }

        /// <summary>
        /// Returns the best-selling product name by total quantity sold.
        /// Null if no sales exist.
        /// </summary>
        public async Task<string?> GetBestSellingProductNameAsync()
        {
            var all = await _sales.Find(Builders<Sale>.Filter.Empty).ToListAsync();
            if (all.Count == 0) return null;

            var totals = new Dictionary<string, int>();
            foreach (var s in all)
            {
                totals.TryGetValue(s.ProductName, out int prev);
                totals[s.ProductName] = prev + s.Quantity;
            }

            string? best = null;
            int bestQty = -1;
            foreach (var kv in totals)
            {
                if (kv.Value > bestQty)
                {
                    bestQty = kv.Value;
                    best = kv.Key;
                }
            }
            return best;
        }
    }
}