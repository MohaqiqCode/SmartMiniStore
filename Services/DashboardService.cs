using System.Collections.Generic;
using System.Threading.Tasks;
using SmartMiniStore.Models;

namespace SmartMiniStore.Services
{
    /// <summary>
    /// Aggregated numbers for the dashboard cards.
    /// Reads from the other services — does not touch MongoDB directly.
    /// </summary>
    public class DashboardService
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly SaleService _saleService;

        public DashboardService(
            MongoDbService mongo,
            ProductService productService,
            CategoryService categoryService,
            SaleService saleService)
        {
            // mongo not used directly, but kept in the constructor for symmetry.
            _ = mongo;
            _productService = productService;
            _categoryService = categoryService;
            _saleService = saleService;
        }

        public async Task<int> GetTotalProductsAsync()
            => (int)await _productService.CountAsync();

        public async Task<int> GetTotalCategoriesAsync()
            => (int)await _categoryService.CountAsync();

        public async Task<int> GetTodaySalesCountAsync()
            => (int)await _saleService.CountTodayAsync();

        public async Task<decimal> GetTodayRevenueAsync()
            => await _saleService.TodayRevenueAsync();

        public async Task<List<Product>> GetLowStockProductsAsync()
            => await _productService.GetLowStockAsync();
    }
}