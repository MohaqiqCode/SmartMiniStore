using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SmartMiniStore.Helpers;
using SmartMiniStore.Models;
using SmartMiniStore.Services;

namespace SmartMiniStore.ViewModels
{
    /// <summary>
    /// Backs the CategoriesView: list, add, edit, delete, search.
    /// </summary>
    public class CategoryViewModel : ViewModelBase
    {
        private readonly CategoryService _categoryService;

        private string _searchText = string.Empty;
        private Category? _selectedCategory;

        private string _formName = string.Empty;
        private string _formDescription = string.Empty;
        private bool _isEditing;
        private string? _editingId;

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

        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }
         
        public string FormName
        {
            get => _formName;
            set => SetProperty(ref _formName, value);
        }

        public string FormDescription
        {
            get => _formDescription;
            set => SetProperty(ref _formDescription, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string FormTitle => IsEditing ? "Edit Category" : "Add Category";

        public RelayCommand LoadCommand { get; }
        public RelayCommand NewCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand CancelCommand { get; }

        public CategoryViewModel(CategoryService categoryService)
        {
            _categoryService = categoryService;

            LoadCommand = new RelayCommand(async _ => await ReloadAsync());
            NewCommand = new RelayCommand(_ => BeginNew());
            EditCommand = new RelayCommand(_ => BeginEdit(), _ => SelectedCategory != null);
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedCategory != null);
            CancelCommand = new RelayCommand(_ => BeginNew());
        }

        private async Task ReloadAsync()
        {
            try
            {
                var list = await _categoryService.SearchByNameAsync(SearchText);
                Categories.Clear();
                foreach (var c in list) Categories.Add(c);
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to load categories: " + ex.Message);
            }
        }

        private void BeginNew()
        {
            IsEditing = false;
            _editingId = null;
            FormName = string.Empty;
            FormDescription = string.Empty;
            OnPropertyChanged(nameof(FormTitle));
        }

        private void BeginEdit()
        {
            if (SelectedCategory == null) return;

            IsEditing = true;
            _editingId = SelectedCategory.Id;
            FormName = SelectedCategory.Name;
            FormDescription = SelectedCategory.Description;
            OnPropertyChanged(nameof(FormTitle));
        }

        private async Task SaveAsync()
        {
            var name = (FormName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            {
                DialogHelper.Warn("Category name is required and must be at least 2 characters.");
                return;
            }

            try
            {
                if (IsEditing && !string.IsNullOrEmpty(_editingId))
                {
                    if (await _categoryService.NameExistsAsync(name, _editingId))
                    {
                        DialogHelper.Warn("Another category with this name already exists.");
                        return;
                    }

                    var updated = new Category
                    {
                        Id = _editingId,
                        Name = name,
                        Description = (FormDescription ?? string.Empty).Trim()
                    };
                    await _categoryService.UpdateAsync(updated);
                    DialogHelper.Info("Category updated.");
                }
                else
                {
                    if (await _categoryService.NameExistsAsync(name))
                    {
                        DialogHelper.Warn("A category with this name already exists.");
                        return;
                    }

                    var category = new Category
                    {
                        Name = name,
                        Description = (FormDescription ?? string.Empty).Trim()
                    };
                    await _categoryService.AddAsync(category);
                    DialogHelper.Info("Category added.");
                }

                BeginNew();
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to save category: " + ex.Message);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedCategory == null) return;

            if (!DialogHelper.Confirm($"Delete category '{SelectedCategory.Name}'?"))
                return;

            try
            {
                var (ok, message) = await _categoryService.DeleteAsync(SelectedCategory.Id);
                if (ok) DialogHelper.Info(message);
                else DialogHelper.Warn(message);

                await ReloadAsync();
            }
            catch (Exception ex)
            {
                DialogHelper.Error("Failed to delete category: " + ex.Message);
            }
        }
    }
}