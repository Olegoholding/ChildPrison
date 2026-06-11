using System.Windows;
using System.Windows.Input;
using diplomApp.View;

namespace ChildPrison
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PageFrame.Navigate(new ChildrenPage());
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void Button_Click(object sender, RoutedEventArgs e) => Close();
        private void Button_Click_2(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Button_Click_1(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

        private void child_Checked(object sender, RoutedEventArgs e)
        {
            PageFrame.Navigate(new ChildrenPage());
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            PageFrame.Navigate(new EmployeesPage());
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            PageFrame.Navigate(new GroupsPage());
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            PageFrame.Navigate(new NutritionPage());
        }
    }
}