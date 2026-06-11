using ChildPrison.Models;
using diplomApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace diplomApp.View.Nutrition
{
    public partial class SuppliersPage : Page
    {
        private DataContext _context;
        private Supplier _currentSupplier;
        private bool _isNewSupplier = false;

        public SuppliersPage()
        {
            InitializeComponent();
            Loaded += SuppliersPage_Loaded;
        }

        private void SuppliersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new DataContext();
                SuppliersListBox.ItemsSource = _context.Suppliers.ToList();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SuppliersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuppliersListBox.SelectedItem is Supplier selected)
            {
                _isNewSupplier = false;
                _currentSupplier = selected;
                NameBox.Text = _currentSupplier.Name;
                AddressBox.Text = _currentSupplier.Address;
                ContactBox.Text = _currentSupplier.ContactInfo;
                DeleteButton.Visibility = Visibility.Visible;
                TitleText.Text = "ИНФОРМАЦИЯ О ПОСТАВЩИКЕ";
            }
        }

        private void AddSupplier_Click(object sender, RoutedEventArgs e)
        {
            _isNewSupplier = true;
            _currentSupplier = new Supplier();
            NameBox.Text = "";
            AddressBox.Text = "";
            ContactBox.Text = "";
            DeleteButton.Visibility = Visibility.Collapsed;
            TitleText.Text = "ДОБАВЛЕНИЕ ПОСТАВЩИКА";
            SuppliersListBox.SelectedItem = null;
        }

        private void ClearForm()
        {
            NameBox.Text = "";
            AddressBox.Text = "";
            ContactBox.Text = "";
        }

        private void SaveSupplier_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentSupplier == null) _currentSupplier = new Supplier();

                if (string.IsNullOrWhiteSpace(NameBox.Text))
                {
                    MessageBox.Show("ВВЕДИТЕ НАЗВАНИЕ ПОСТАВЩИКА");
                    return;
                }

                _currentSupplier.Name = NameBox.Text.Trim();
                _currentSupplier.Address = AddressBox.Text.Trim();
                _currentSupplier.ContactInfo = ContactBox.Text.Trim();

                if (_isNewSupplier) _context.Suppliers.Add(_currentSupplier);
                else _context.Entry(_currentSupplier).State = EntityState.Modified;

                _context.SaveChanges();
                MessageBox.Show("Поставщик сохранён");
                LoadData();

                var saved = _context.Suppliers.FirstOrDefault(s => s.Id == _currentSupplier.Id);
                if (saved != null) SuppliersListBox.SelectedItem = saved;

                _isNewSupplier = false;
                DeleteButton.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSupplier == null) return;

            var result = MessageBox.Show($"Удалить {_currentSupplier.Name}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _context.Suppliers.Remove(_currentSupplier);
                _context.SaveChanges();
                LoadData();
                ClearForm();
                _currentSupplier = null;
                DeleteButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}