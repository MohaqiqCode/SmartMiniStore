using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SmartMiniStore.Helpers;
using SmartMiniStore.Models;
using SmartMiniStore.Services;

namespace SmartMiniStore.ViewModels
{
    /// <summary>
    /// Backs the ProductsView: list, add, edit, delete, search.
    /// </summary>
    public class ProductViewModel : ViewModelBase
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;

        // ---- list state ----
        private string _searchText = string.Empty;
        private Product? _selectedProduct;

        // ---- form state ----
        private string _formName = string.Empty;
        private string _formPrice = string.Empty;
        private string _formQuantity = string.Empty;
        private string _formMinimumStock = string.Empty;
        private Category? _formCategory;
        private bool _isEditing;
        private string? _editingId;

        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    _ = ReloadAsync();
            }
        }

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public string FormName
        {
            get => _formName;
            set => SetProperty(ref _formName, value);
        }

        public string FormPrice
        {
            get => _formPrice;
            set => SetProperty(ref _formPrice, value);
        }

        public string FormQuantity
        {
            get => _formQuantity;
            set => SetProperty(ref _formQuantity, value);
        }

        public string FormMinimumStock
        {
            get => _formMinimumStock;
            set => SetProperty(ref _formMinimumStock, value);
        }

        public Category? FormCategory
        {
            get => _formCategory;
            set => SetProperty(ref _formCategory, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string FormTitle => IsEditing ? "Edit Product" : "Add Product";

        // ---- commands ----
        public RelayCommand LoadCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand NewCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ProductViewModel(ProductService productService, CategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;

            LoadCommand = new RelayCommand(async _ => await ReloadAsync());
            SearchCommand = new RelayCommand(async _ => await ReloadAsync());
            NewCommand = new RelayCommand(_ => BeginNew());
            EditCommand = new RelayCommand(_ => BeginEdit(), _ => SelectedProduct != null);
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedProduct != null);
            CancelCommand = new RelayCommand(_ => CancelForm());
        }

        private async Task ReloadAsync()
        {
            try
            {
                // Load categories once so the ComboBox has data.
                if (Categories.Count == 0)
                {
                    var cats = await _categoryService.GetAllAsync();
                    Categories.Clear();
                    foreach (var c in cats) Categories.Add(c);
                }

                var list = await _productService.SearchByNameAsync(SearchText);
                Products.Clear();
                foreach (var p in list) Products.Add(p);
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to load products: " + ex.Message);
            }
        }

        private void BeginNew()
        {
            IsEditing = false;
            _editingId = null;
            FormName = string.Empty;
            FormPrice = string.Empty;
            FormQuantity = string.Empty;
            FormMinimumStock = string.Empty;
            FormCategory = null;
            OnPropertyChanged(nameof(FormTitle));
        }

        private void BeginEdit()
        {
            if (SelectedProduct == null) return;

            IsEditing = true;
            _editingId = SelectedProduct.Id;
            FormName = SelectedProduct.Name;
            FormPrice = SelectedProduct.Price.ToString();
            FormQuantity = SelectedProduct.Quantity.ToString();
            FormMinimumStock = SelectedProduct.MinimumStock.ToString();

            // Find the matching category object
            FormCategory = null;
            foreach (var c in Categories)
            {
                if (c.Id == SelectedProduct.CategoryId)
                {
                    FormCategory = c;
                    break;
                }
            }
            OnPropertyChanged(nameof(FormTitle));
        }

        private void CancelForm()
        {
            BeginNew();
        }

        private async Task SaveAsync()
        {
            // ---- validation ----
            if (string.IsNullOrWhiteSpace(FormName) || FormName.Trim().Length < 2)
            {
                DialogHelper.Warn("Product name is required and must be at least 2 characters.");
                return;
            }

            if (!decimal.TryParse(FormPrice, out var price) || price <= 0)
            {
                DialogHelper.Warn("Price must be a number greater than 0.");
                return;
            }

            if (!int.TryParse(FormQuantity, out var quantity) || quantity < 0)
            {
                DialogHelper.Warn("Quantity must be a whole number greater than or equal to 0.");
                return;
            }

            if (!int.TryParse(FormMinimumStock, out var minStock) || minStock < 0)
            {
                DialogHelper.Warn("Minimum stock must be a whole number greater than or equal to 0.");
                return;
            }

            if (FormCategory == null)
            {
                DialogHelper.Warn("Please select a category.");
                return;
            }

            try
            {
                if (IsEditing && !string.IsNullOrEmpty(_editingId))
                {
                    var product = new Product
                    {
                        Id = _editingId,
                        Name = FormName.Trim(),
                        CategoryId = FormCategory.Id,
                        CategoryName = FormCategory.Name,
                        Price = price,
                        Quantity = quantity,
                        MinimumStock = minStock,
                        CreatedAt = SelectedProduct?.CreatedAt ?? DateTime.UtcNow
                    };
                    await _productService.UpdateAsync(product);
                    DialogHelper.Info("Product updated.");
                }
                else
                {
                    var product = new Product
                    {
                        Name = FormName.Trim(),
                        CategoryId = FormCategory.Id,
                        CategoryName = FormCategory.Name,
                        Price = price,
                        Quantity = quantity,
                        MinimumStock = minStock,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _productService.AddAsync(product);
                    DialogHelper.Info("Product added.");
                }

                BeginNew();
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to save product: " + ex.Message);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedProduct == null) return;

            if (!DialogHelper.Confirm($"Delete product '{SelectedProduct.Name}'?"))
                return;

            try
            {
                var ok = await _productService.DeleteAsync(SelectedProduct.Id);
                if (ok) DialogHelper.Info("Product deleted.");
                else DialogHelper.Warn("Product was not found.");
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to delete product: " + ex.Message);
            }
        }
    }
}