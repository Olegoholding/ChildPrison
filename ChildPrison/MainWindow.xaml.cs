using System.Windows;
using System.Windows.Input;
using diplomApp.View.ListPage;

namespace ChildPrison
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PageFrame.Content = new ListPage();
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void Button_Click(object sender, RoutedEventArgs e) => Close();
        private void Button_Click_2(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Button_Click_1(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
    }
}