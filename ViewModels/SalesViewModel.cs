using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SmartMiniStore.Helpers;
using SmartMiniStore.Models;
using SmartMiniStore.Services;

namespace SmartMiniStore.ViewModels
{
    /// <summary>
    /// Backs the SalesView: create a sale + sales history.
    /// </summary>
    public class SalesViewModel : ViewModelBase
    {
        private readonly SaleService _saleService;
        private readonly ProductService _productService;

        // form state
        private Product? _formProduct;
        private string _formQuantity = string.Empty;

        // history state
        private string _searchText = string.Empty;
        private DateTime? _filterDate;
        private Sale? _selectedSale;

        public ObservableCollection<Product> AvailableProducts { get; } = new();
        public ObservableCollection<Sale> Sales { get; } = new();

        public Product? FormProduct
        {
            get => _formProduct;
            set
            {
                if (SetProperty(ref _formProduct, value))
                    OnPropertyChanged(nameof(FormProductInfo));
            }
        }

        public string FormQuantity
        {
            get => _formQuantity;
            set => SetProperty(ref _formQuantity, value);
        }

        public string FormProductInfo =>
            FormProduct == null
                ? "Select a product."
                : $"Price: {MoneyFormatter.Format(FormProduct.Price)}   |   Available: {FormProduct.Quantity}";

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    _ = ReloadHistoryAsync();
            }
        }

        public DateTime? FilterDate
        {
            get => _filterDate;
            set
            {
                if (SetProperty(ref _filterDate, value))
                    _ = ReloadHistoryAsync();
            }
        }

        public Sale? SelectedSale
        {
            get => _selectedSale;
            set => SetProperty(ref _selectedSale, value);
        }

        public RelayCommand LoadCommand { get; }
        public RelayCommand CreateSaleCommand { get; }
        public RelayCommand RefreshHistoryCommand { get; }
        public RelayCommand ClearFilterCommand { get; }

        public SalesViewModel(SaleService saleService, ProductService productService)
        {
            _saleService = saleService;
            _productService = productService;

            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            CreateSaleCommand = new RelayCommand(async _ => await CreateSaleAsync());
            RefreshHistoryCommand = new RelayCommand(async _ => await ReloadHistoryAsync());
            ClearFilterCommand = new RelayCommand(async _ =>
            {
                FilterDate = null;
                SearchText = string.Empty;
                await ReloadHistoryAsync();
            });
        }

        private async Task LoadAsync()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                AvailableProducts.Clear();
                foreach (var p in products) AvailableProducts.Add(p);

                await ReloadHistoryAsync();
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to load sales page: " + ex.Message);
            }
        }

        private async Task ReloadHistoryAsync()
        {
            try
            {
                System.Collections.Generic.List<Sale> list;

                if (FilterDate.HasValue)
                    list = await _saleService.GetByDateAsync(FilterDate.Value);
                else
                    list = await _saleService.SearchByProductNameAsync(SearchText);

                Sales.Clear();
                foreach (var s in list) Sales.Add(s);
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to load sales history: " + ex.Message);
            }
        }

        private async Task CreateSaleAsync()
        {
            if (FormProduct == null)
            {
                DialogHelper.Warn("Please select a product.");
                return;
            }

            if (!int.TryParse(FormQuantity, out var qty) || qty <= 0)
            {
                DialogHelper.Warn("Quantity must be a whole number greater than 0.");
                return;
            }

            try
            {
                var (ok, message) = await _saleService.CreateSaleAsync(FormProduct.Id, qty);

                if (ok)
                {
                    DialogHelper.Info(message);
                    FormQuantity = string.Empty;

                    // Reload products so we show the new stock and refresh the picker.
                    var products = await _productService.GetAllAsync();
                    AvailableProducts.Clear();
                    foreach (var p in products) AvailableProducts.Add(p);

                    // Re-select the same product (its Quantity changed).
                    FormProduct = null;
                    foreach (var p in AvailableProducts)
                    {
                        if (p.Id == _formProduct?.Id) { /* nothing — we already cleared */ }
                    }

                    await ReloadHistoryAsync();
                }
                else
                {
                    DialogHelper.Warn(message);
                }
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to create sale: " + ex.Message);
            }
        }
    }
}