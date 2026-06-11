using ChildPrison.Models;
using diplomApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace diplomApp.View.Nutrition
{
    public partial class ProductsPage : Page
    {
        private DataContext _context;
        private Product _currentProduct;
        private bool _isNewProduct = false;

        public ProductsPage()
        {
            InitializeComponent();
            Loaded += ProductsPage_Loaded;
        }

        private void ProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new DataContext();

                var products = _context.Products.Include(p => p.Supplier).ToList();
                ProductsListBox.ItemsSource = products;

                var suppliers = _context.Suppliers.ToList();
                SupplierComboBox.ItemsSource = suppliers;
                SupplierComboBox.SelectedValuePath = "Id";

                ClearForm();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void ProductsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsListBox.SelectedItem is Product selected)
            {
                _isNewProduct = false;
                _currentProduct = _context.Products.Include(p => p.Supplier).FirstOrDefault(p => p.Id == selected.Id);
                if (_currentProduct != null)
                {
                    ProductNameBox.Text = _currentProduct.ProductName;
                    QuantityBox.Text = _currentProduct.Quantity?.ToString() ?? "";
                    SupplierComboBox.SelectedValue = _currentProduct.SupplierId;
                    DeleteButton.Visibility = Visibility.Visible;
                    TitleText.Text = "ИНФОРМАЦИЯ О ПРОДУКТЕ";
                    StatusText.Text = "";
                }
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            _isNewProduct = true;
            _currentProduct = new Product();
            ProductNameBox.Text = "";
            QuantityBox.Text = "";
            SupplierComboBox.SelectedIndex = -1;
            DeleteButton.Visibility = Visibility.Collapsed;
            TitleText.Text = "ДОБАВЛЕНИЕ ПРОДУКТА";
            ProductsListBox.SelectedItem = null;
            StatusText.Text = "";
        }

        private void ClearForm()
        {
            ProductNameBox.Text = "";
            QuantityBox.Text = "";
            SupplierComboBox.SelectedIndex = -1;
        }

        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentProduct == null)
                {
                    _currentProduct = new Product();
                    _isNewProduct = true;
                }

                if (string.IsNullOrWhiteSpace(ProductNameBox.Text))
                {
                    StatusText.Text = "ВВЕДИТЕ НАЗВАНИЕ ПРОДУКТА!";
                    return;
                }

                _currentProduct.ProductName = ProductNameBox.Text.Trim();
                _currentProduct.Quantity = int.TryParse(QuantityBox.Text, out int qty) ? qty : (int?)null;
                _currentProduct.SupplierId = SupplierComboBox.SelectedValue as int?;

                if (_isNewProduct)
                {
                    _context.Products.Add(_currentProduct);
                }
                else
                {
                    _context.Entry(_currentProduct).State = EntityState.Modified;
                }
                _context.SaveChanges();

                StatusText.Text = "ПРОДУКТ СОХРАНЕН!";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;
                LoadData();

                var saved = _context.Products.FirstOrDefault(p => p.Id == _currentProduct.Id);
                if (saved != null) ProductsListBox.SelectedItem = saved;

                _isNewProduct = false;
                DeleteButton.Visibility = Visibility.Visible;
                TitleText.Text = "ИНФОРМАЦИЯ О ПРОДУКТЕ";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА: {ex.Message}";
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProduct == null) return;

            var result = MessageBox.Show($"Удалить {_currentProduct.ProductName}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _context.Products.Remove(_currentProduct);
                _context.SaveChanges();
                StatusText.Text = "ПРОДУКТ УДАЛЕН";
                LoadData();
                ClearForm();
                _currentProduct = null;
                DeleteButton.Visibility = Visibility.Collapsed;
                TitleText.Text = "ИНФОРМАЦИЯ О ПРОДУКТЕ";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"ОШИБКА: {ex.Message}";
            }
        }
    }
}