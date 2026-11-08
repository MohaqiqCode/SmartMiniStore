using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SmartMiniStore.Helpers;
using SmartMiniStore.Models;
using SmartMiniStore.Services;

namespace SmartMiniStore.ViewModels
{
    /// <summary>
    /// Backs the DashboardView: five statistic cards + low-stock table.
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        private readonly DashboardService _dashboardService;
        private readonly SeedDataService _seedService;

        private int _totalProducts;
        private int _totalCategories;
        private int _todaySales;
        private decimal _todayRevenue;
        private int _lowStockCount;

        public int TotalProducts
        {
            get => _totalProducts;
            set => SetProperty(ref _totalProducts, value);
        }

        public int TotalCategories
        {
            get => _totalCategories;
            set => SetProperty(ref _totalCategories, value);
        }

        public int TodaySales
        {
            get => _todaySales;
            set => SetProperty(ref _todaySales, value);
        }

        public decimal TodayRevenue
        {
            get => _todayRevenue;
            set
            {
                if (SetProperty(ref _todayRevenue, value))
                    OnPropertyChanged(nameof(TodayRevenueDisplay));
            }
        }

        public string TodayRevenueDisplay => MoneyFormatter.Format(_todayRevenue);

        public int LowStockCount
        {
            get => _lowStockCount;
            set => SetProperty(ref _lowStockCount, value);
        }

        public ObservableCollection<Product> LowStockProducts { get; } = new();

        public RelayCommand RefreshCommand { get; }
        public RelayCommand LoadSampleDataCommand { get; }

        public DashboardViewModel(DashboardService dashboardService, SeedDataService seedService)
        {
            _dashboardService = dashboardService;
            _seedService = seedService;

            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
            LoadSampleDataCommand = new RelayCommand(async _ => await LoadSampleDataAsync());
        }

        private async Task RefreshAsync()
        {
            try
            {
                TotalProducts = await _dashboardService.GetTotalProductsAsync();
                TotalCategories = await _dashboardService.GetTotalCategoriesAsync();
                TodaySales = await _dashboardService.GetTodaySalesCountAsync();
                TodayRevenue = await _dashboardService.GetTodayRevenueAsync();

                var lowStock = await _dashboardService.GetLowStockProductsAsync();
                LowStockCount = lowStock.Count;

                LowStockProducts.Clear();
                foreach (var p in lowStock) LowStockProducts.Add(p);
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to load dashboard: " + ex.Message);
            }
        }

        private async Task LoadSampleDataAsync()
        {
            try
            {
                var summary = await _seedService.SeedIfEmptyAsync();
                DialogHelper.Info(summary);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to load sample data: " + ex.Message);
            }
        }
    }
}