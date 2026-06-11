using System.Windows;

namespace diplomApp.Services
{
    public static class CurrentUser
    {
        public static int ID { get; set; }
        public static string Login { get; set; }
        public static string FirstName { get; set; }
        public static string MidName { get; set; }
        public static string LastName { get; set; }

        public static bool LogUser(string login, string password)
        {
            DataContext _context = new DataContext();

            var user = _context.Users.FirstOrDefault(u => u.Login == login);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ID = user.Id;
                Login = login;
                FirstName = user.FirstName;
                MidName = user.MidName;
                LastName = user.LastName;

                MessageBox.Show($"Добро пожаловать, {Login}!");
                return true;
            }
            else
            {
                MessageBox.Show($"Неверный логин или пароль");
                return false;
            }
        }

        public static void LogoutUser()
        {
            ID = 0;
            Login = null;
            FirstName = null;
            MidName = null;
            LastName = null;
        }
    }
}
