using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SmartMiniStore.Helpers;
using SmartMiniStore.Models;
using SmartMiniStore.Services;

namespace SmartMiniStore.ViewModels
{
    /// <summary>
    /// Backs the ReportsView: total sales, total revenue,
    /// best-selling product, and low-stock products.
    /// </summary>
    public class ReportsViewModel : ViewModelBase
    {
        private readonly DashboardService _dashboardService;
        private readonly SaleService _saleService;
        private readonly ProductService _productService;

        private int _totalSales;
        private decimal _totalRevenue;
        private string _bestSellingProduct = "—";
        private int _lowStockCount;

        public int TotalSales
        {
            get => _totalSales;
            set => SetProperty(ref _totalSales, value);
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                if (SetProperty(ref _totalRevenue, value))
                    OnPropertyChanged(nameof(TotalRevenueDisplay));
            }
        }

        public string TotalRevenueDisplay => MoneyFormatter.Format(_totalRevenue);

        public string BestSellingProduct
        {
            get => _bestSellingProduct;
            set => SetProperty(ref _bestSellingProduct, value);
        }

        public int LowStockCount
        {
            get => _lowStockCount;
            set => SetProperty(ref _lowStockCount, value);
        }

        public ObservableCollection<Product> LowStockProducts { get; } = new();

        public RelayCommand LoadCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public ReportsViewModel(
            DashboardService dashboardService,
            SaleService saleService,
            ProductService productService)
        {
            _dashboardService = dashboardService;
            _saleService = saleService;
            _productService = productService;

            LoadCommand = new RelayCommand(async _ => await RefreshAsync());
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        }

        private async Task RefreshAsync()
        {
            try
            {
                TotalSales = (int)await _saleService.CountAllAsync();
                TotalRevenue = await _saleService.TotalRevenueAsync();

                var best = await _saleService.GetBestSellingProductNameAsync();
                BestSellingProduct = best ?? "—";

                var lowStock = await _dashboardService.GetLowStockProductsAsync();
                LowStockCount = lowStock.Count;
                LowStockProducts.Clear();
                foreach (var p in lowStock) LowStockProducts.Add(p);
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to load report: " + ex.Message);
            }
        }
    }
}