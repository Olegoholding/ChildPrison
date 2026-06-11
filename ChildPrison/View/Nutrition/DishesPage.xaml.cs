using ChildPrison.Models;
using diplomApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace diplomApp.View.Nutrition
{
    public partial class DishesPage : Page
    {
        private DataContext _context;
        private Dish _currentDish;
        private bool _isNewDish = false;
        private List<Product> _allProducts;
        private ObservableCollection<DishProduct> _ingredients;

        public DishesPage()
        {
            InitializeComponent();
            Loaded += DishesPage_Loaded;
        }

        private void DishesPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new DataContext();

                // Загрузка блюд с ингредиентами и меню
                var dishes = _context.Dishes
                    .Include(d => d.Menus)
                    .Include(d => d.DishProducts)
                        .ThenInclude(dp => dp.Product)
                    .ToList();
                DishesListBox.ItemsSource = dishes;

                // Загрузка всех продуктов
                _allProducts = _context.Products.ToList();

                // Загрузка для добавления ингредиентов
                AddProductComboBox.ItemsSource = _allProducts;
                AddProductComboBox.DisplayMemberPath = "ProductName";
                AddProductComboBox.SelectedValuePath = "Id";

                ClearForm();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var searchText = SearchBox.Text.ToLower();
                var dishes = _context.Dishes
                    .Include(d => d.Menus)
                    .Include(d => d.DishProducts)
                        .ThenInclude(dp => dp.Product)
                    .Where(d => d.DishName.ToLower().Contains(searchText))
                    .ToList();
                DishesListBox.ItemsSource = dishes;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка поиска: {ex.Message}";
            }
        }

        private void DishesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DishesListBox.SelectedItem is Dish selected)
            {
                _isNewDish = false;
                _currentDish = _context.Dishes
                    .Include(d => d.Menus)
                    .Include(d => d.DishProducts)
                        .ThenInclude(dp => dp.Product)
                    .FirstOrDefault(d => d.Id == selected.Id);

                if (_currentDish != null)
                {
                    DishNameBox.Text = _currentDish.DishName;

                    // Загрузка ингредиентов
                    _ingredients = new ObservableCollection<DishProduct>(_currentDish.DishProducts);
                    IngredientsDataGrid.ItemsSource = _ingredients;

                    DeleteButton.Visibility = Visibility.Visible;
                    TitleText.Text = "ИНФОРМАЦИЯ О БЛЮДЕ";
                    StatusText.Text = "";
                }
            }
        }

        private void AddDish_Click(object sender, RoutedEventArgs e)
        {
            _isNewDish = true;
            _currentDish = new Dish();
            _ingredients = new ObservableCollection<DishProduct>();
            DishNameBox.Text = "";
            IngredientsDataGrid.ItemsSource = _ingredients;
            DeleteButton.Visibility = Visibility.Collapsed;
            TitleText.Text = "ДОБАВЛЕНИЕ БЛЮДА";
            DishesListBox.SelectedItem = null;
            StatusText.Text = "";
            AddProductComboBox.SelectedIndex = -1;
            AddQuantityBox.Text = "1";
            AddUnitComboBox.SelectedIndex = 0;
        }

        private void ClearForm()
        {
            DishNameBox.Text = "";
            _ingredients = new ObservableCollection<DishProduct>();
            IngredientsDataGrid.ItemsSource = _ingredients;
            AddProductComboBox.SelectedIndex = -1;
            AddQuantityBox.Text = "1";
            AddUnitComboBox.SelectedIndex = 0;
        }

        private void AddIngredient_Click(object sender, RoutedEventArgs e)
        {
            if (AddProductComboBox.SelectedItem == null)
            {
                StatusText.Text = "ВЫБЕРИТЕ ПРОДУКТ!";
                return;
            }

            var product = AddProductComboBox.SelectedItem as Product;
            if (product == null) return;

            // Проверяем, не добавлен ли уже этот продукт
            if (_ingredients.Any(i => i.ProductId == product.Id))
            {
                StatusText.Text = "ЭТОТ ПРОДУКТ УЖЕ ДОБАВЛЕН!";
                return;
            }

            double quantity = 1;
            if (!double.TryParse(AddQuantityBox.Text, out quantity))
            {
                quantity = 1;
            }

            string unit = (AddUnitComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "г";

            var dishProduct = new DishProduct
            {
                ProductId = product.Id,
                Product = product,
                Quantity = quantity,
                Unit = unit
            };

            _ingredients.Add(dishProduct);

            AddProductComboBox.SelectedIndex = -1;
            AddQuantityBox.Text = "1";
            AddUnitComboBox.SelectedIndex = 0;
        }

        private void RemoveIngredient_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dishProduct = button?.Tag as DishProduct;
            if (dishProduct != null)
            {
                _ingredients.Remove(dishProduct);
            }
        }

        private void SaveDish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentDish == null)
                {
                    _currentDish = new Dish();
                    _isNewDish = true;
                }

                if (string.IsNullOrWhiteSpace(DishNameBox.Text))
                {
                    StatusText.Text = "ВВЕДИТЕ НАЗВАНИЕ БЛЮДА!";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                if (_ingredients.Count == 0)
                {
                    StatusText.Text = "ДОБАВЬТЕ ХОТЯ БЫ ОДИН ИНГРЕДИЕНТ!";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                _currentDish.DishName = DishNameBox.Text.Trim();

                if (_isNewDish)
                {
                    _context.Dishes.Add(_currentDish);
                    _context.SaveChanges();
                }
                else
                {
                    _context.Entry(_currentDish).State = EntityState.Modified;
                    _context.SaveChanges();

                    // Удаляем старые связи с продуктами
                    var oldIngredients = _context.DishProducts.Where(dp => dp.DishId == _currentDish.Id).ToList();
                    _context.DishProducts.RemoveRange(oldIngredients);
                    _context.SaveChanges();
                }

                // Добавляем новые ингредиенты
                foreach (var ingredient in _ingredients)
                {
                    ingredient.DishId = _currentDish.Id;
                    ingredient.Id = 0; // Сбрасываем ID для нового добавления
                    _context.DishProducts.Add(ingredient);
                }
                _context.SaveChanges();

                StatusText.Text = "БЛЮДО СОХРАНЕНО!";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;

                LoadData();

                var saved = _context.Dishes
                    .Include(d => d.DishProducts)
                    .FirstOrDefault(d => d.Id == _currentDish.Id);
                if (saved != null) DishesListBox.SelectedItem = saved;

                _isNewDish = false;
                DeleteButton.Visibility = Visibility.Visible;
                TitleText.Text = "ИНФОРМАЦИЯ О БЛЮДЕ";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void DeleteDish_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDish == null) return;

            var menuCount = _context.Menus.Count(m => m.DishId == _currentDish.Id);

            if (menuCount > 0)
            {
                var result = MessageBox.Show(
                    $"Блюдо используется в {menuCount} позициях меню.\nУдалить блюдо?",
                    "ПОДТВЕРЖДЕНИЕ УДАЛЕНИЯ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;
            }
            else
            {
                var result = MessageBox.Show(
                    $"Удалить блюдо {_currentDish.DishName}?",
                    "ПОДТВЕРЖДЕНИЕ УДАЛЕНИЯ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;
            }

            try
            {
                // Удаляем связи с ингредиентами
                var ingredients = _context.DishProducts.Where(dp => dp.DishId == _currentDish.Id).ToList();
                _context.DishProducts.RemoveRange(ingredients);

                // Удаляем связи с меню
                var menuItems = _context.Menus.Where(m => m.DishId == _currentDish.Id).ToList();
                _context.Menus.RemoveRange(menuItems);

                // Удаляем блюдо
                _context.Dishes.Remove(_currentDish);
                _context.SaveChanges();

                StatusText.Text = "БЛЮДО УДАЛЕНО";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;

                LoadData();
                ClearForm();
                _currentDish = null;
                DeleteButton.Visibility = Visibility.Collapsed;
                TitleText.Text = "ИНФОРМАЦИЯ О БЛЮДЕ";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА УДАЛЕНИЯ: {ex.Message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
    }
}