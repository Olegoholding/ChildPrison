using diplomApp.Services;
using System.Windows;
using System.Windows.Input;

namespace ChildPrison
{
    public partial class Registration : Window
    {
        public Registration() => InitializeComponent();
        private void Border_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void Button_Click(object sender, RoutedEventArgs e) => Close();
        private void Button_Click_2(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PassBox.Text.Trim();

            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PassBox.Focus();
                return;
            }

            if (CurrentUser.LogUser(login, password))
            {
                new MainWindow().Show();
                Close();
            }
        }
    }
}
