using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChildPrison
{
    public partial class Registration : Window
    {
        private string adminKey = "admin";
        public Registration() => InitializeComponent();
        private void Border_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void Button_Click(object sender, RoutedEventArgs e) => Close();
        private void Button_Click_2(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void AuthBtn_Checked(object sender, RoutedEventArgs e)
        {
            string login = (LoginBox.Text).Replace(" ", "").ToLower();
            string pass = (PassBox.Text).Replace(" ", "").ToLower();
            BackLogic.EncryptionSystem encrypt = new();
            if (KeyLbl.Visibility == Visibility.Visible)
            {
                if ((KeyBox.Text).Replace(" ", "").ToLower() != adminKey)
                {
                    MessageBox.Show("Ключ неверен");
                    return;
                }
                byte[] salt = Array.Empty<byte>();
                encrypt.CreateHash(pass, login, salt);
                MessageBox.Show("Регистрация завершена");
            }
            else
            {
                BackLogic.DatabaseProvider database = new();
                byte[] saltFromDatabase = database.getFromUserTable(login, "SELECT Соль FROM Пользователи WHERE Логин = @Login");
                byte[] hashFromDatabase = database.getFromUserTable(login, "SELECT Хеш FROM Пользователи WHERE Логин = @Login");
                byte[] newHash = encrypt.CreateHash(pass, login, saltFromDatabase);
                if(hashFromDatabase.SequenceEqual(newHash))
                {
                    MessageBox.Show("All true");
                }
                else
                {
                    MessageBox.Show("Nothing true");
                }
            }
            AuthBtn.IsChecked = false;
        }
        #region Switches
        private void ChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            if(!(TextRegBtn.Text == "Вход"))
            {
                KeyGrid.Visibility = Visibility.Collapsed;
                KeyLbl.Visibility = Visibility.Collapsed;
                TextRegBtn.Text = "Вход";
                AuthBtnText.Text = "Войти";
                ChangeBtn.IsChecked = false;
            }
            else
            {
                KeyGrid.Visibility = Visibility.Visible;
                KeyLbl.Visibility = Visibility.Visible;
                TextRegBtn.Text = "Регистрация";
                AuthBtnText.Text = "Зарегистрироваться";
                ChangeBtn.IsChecked = false;
            }
        }
        #endregion
    }
}
