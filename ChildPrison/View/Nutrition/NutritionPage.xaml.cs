using diplomApp.Services;
using diplomApp.View.Nutrition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace diplomApp.View
{
    public partial class NutritionPage : Page
    {
        private DataContext _context;

        public NutritionPage()
        {
            InitializeComponent();
            Loaded += NutritionPage_Loaded;
        }

        private void NutritionPage_Loaded(object sender, RoutedEventArgs e)
        {
            _context = new DataContext();
            UpdateStats();
            ProductsRadio.IsChecked = true;
            ContentFrame.Navigate(new ProductsPage());
        }

        private void UpdateStats()
        {
            var productsCount = _context.Products.Count();
            var suppliersCount = _context.Suppliers.Count();
            var dishesCount = _context.Dishes.Count();
            var menuCount = _context.Menus.Count();

            StatsText.Text = $"ПРОДУКТОВ: {productsCount}\nПОСТАВЩИКОВ: {suppliersCount}\nБЛЮД: {dishesCount}\nПОЗИЦИЙ МЕНЮ: {menuCount}";
        }

        private void ProductsRadio_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ProductsPage());
            UpdateStats();
        }

        private void SuppliersRadio_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new SuppliersPage());
            UpdateStats();
        }

        private void DishesRadio_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new DishesPage());
            UpdateStats();
        }

        private void MenuRadio_Checked(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new MenuManagementPage());
            UpdateStats();
        }
    }
}