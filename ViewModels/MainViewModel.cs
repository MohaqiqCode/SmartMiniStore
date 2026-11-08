using System;
using SmartMiniStore.Helpers;
using SmartMiniStore.Services;
using SmartMiniStore.Views;

namespace SmartMiniStore.ViewModels
{
    /// <summary>
    /// Owns the navigation between pages and the yellow warning banner
    /// when MongoDB is not reachable.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly MongoDbService _mongo;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly SaleService _saleService;
        private readonly DashboardService _dashboardService;
        private readonly SeedDataService _seedService;

        private object? _currentView;
        private string _headerText = "Dashboard";

        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string HeaderText
        {
            get => _headerText;
            set => SetProperty(ref _headerText, value);
        }

        public RelayCommand NavigateCommand { get; }

        public System.Windows.Visibility ConnectionErrorVisibility =>
            _mongo.IsConnected
                ? System.Windows.Visibility.Collapsed
                : System.Windows.Visibility.Visible;

        public string ConnectionErrorMessage =>
            _mongo.IsConnected
                ? string.Empty
                : "Unable to connect to MongoDB. Please make sure MongoDB is running.";

        public MainViewModel(
            MongoDbService mongo,
            ProductService productService,
            CategoryService categoryService,
            SaleService saleService,
            DashboardService dashboardService,
            SeedDataService seedService)
        {
            _mongo = mongo;
            _productService = productService;
            _categoryService = categoryService;
            _saleService = saleService;
            _dashboardService = dashboardService;
            _seedService = seedService;

            NavigateCommand = new RelayCommand(p => Navigate(p as string));

            // Start on the dashboard.
            Navigate("Dashboard");
        }

        private void Navigate(string? page)
        {
            // If Mongo is down, don't even try to build views that query data.
            if (!_mongo.IsConnected)
            {
                HeaderText = "Offline";
                CurrentView = new OfflineView();
                return;
            }

            switch (page)
            {
                case "Dashboard":
                    {
                        HeaderText = "Dashboard";
                        var vm = new DashboardViewModel(_dashboardService, _seedService);
                        var view = new DashboardView { DataContext = vm };
                        vm.RefreshCommand.Execute(null);
                        CurrentView = view;
                        break;
                    }
                case "Products":
                    {
                        HeaderText = "Products";
                        var vm = new ProductViewModel(_productService, _categoryService);
                        var view = new ProductsView { DataContext = vm };
                        vm.LoadCommand.Execute(null);
                        CurrentView = view;
                        break;
                    }
                case "Categories":
                    {
                        HeaderText = "Categories";
                        var vm = new CategoryViewModel(_categoryService);
                        var view = new CategoriesView { DataContext = vm };
                        vm.LoadCommand.Execute(null);
                        CurrentView = view;
                        break;
                    }
                case "Sales":
                    {
                        HeaderText = "Sales";
                        var vm = new SalesViewModel(_saleService, _productService);
                        var view = new SalesView { DataContext = vm };
                        vm.LoadCommand.Execute(null);
                        CurrentView = view;
                        break;
                    }
                case "Reports":
                    {
                        HeaderText = "Reports";
                        var vm = new ReportsViewModel(_dashboardService, _saleService, _productService);
                        var view = new ReportsView { DataContext = vm };
                        vm.LoadCommand.Execute(null);
                        CurrentView = view;
                        break;
                    }
            }
        }
    }
}