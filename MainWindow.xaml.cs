using System.Windows;
using SmartMiniStore.Services;
using SmartMiniStore.ViewModels;

namespace SmartMiniStore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 1. The single MongoDB connection.
            var mongo = new MongoDbService();

            // 2. Domain services. Order matters: services that other
            //    services depend on are created first.
            var productService = new ProductService(mongo);
            var categoryService = new CategoryService(mongo, productService);
            var saleService = new SaleService(mongo, productService);
            var dashboardService = new DashboardService(mongo, productService, categoryService, saleService);
            var seedService = new SeedDataService(mongo, categoryService, productService);

            // 3. The MainViewModel coordinates navigation.
            var mainViewModel = new MainViewModel(
                mongo,
                productService,
                categoryService,
                saleService,
                dashboardService,
                seedService);

            DataContext = mainViewModel;
        }
    }
}