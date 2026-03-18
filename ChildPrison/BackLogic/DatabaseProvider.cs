using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;

namespace ChildPrison.BackLogic
{
    internal class DatabaseProvider : MainWindow
    {
        SQLiteConnection conn;
        SQLiteCommand cmd;
        SQLiteDataAdapter adapter;
        SQLiteDataReader reader;
        string path = $"{Environment.CurrentDirectory}\\Database.db";
        public DatabaseProvider()
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("Err");
                Close();
            }
        }
        protected string getConnStr() => $"Data Source = {path};Version=3; FailIfMissing=true";
        private bool isDuplicate(string login)
        {
            try
            {
                using (conn = new($"{getConnStr()}"))
                {
                    conn.Open();
                    using (cmd = new("SELECT Логин FROM Пользователи WHERE Логин = @Login", conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        return cmd.ExecuteScalar() != null;
                    }
                }
            }
            catch (Exception ex) { return false; }
        }
        protected void addUser(string login, byte[][] data)
        {
            if(isDuplicate(login))
            {
                MessageBox.Show("Err1");
                return;
            }
            using (conn = new($"{getConnStr()}"))
            {
                conn.Open();
                using (cmd = new("INSERT INTO Пользователи (Логин, Хеш, Соль) VALUES(@Login, @Hash, @Salt)", conn))
                {
                    cmd.Parameters.AddWithValue("@Login", login);
                    cmd.Parameters.AddWithValue("@Salt", data[0]);
                    cmd.Parameters.AddWithValue("@Hash", data[1]);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public byte[] getFromUserTable(string login, string command)
        {
            byte[] byteMassive = new byte[16];
            using (conn = new($"{getConnStr()}"))
            {
                conn.Open();
                using (cmd = new(command, conn))
                {
                    cmd.Parameters.AddWithValue("@Login", login);
                    object result = cmd.ExecuteScalar();
                    byteMassive = ((byte[])result);
                    return byteMassive;
                }
            }
        }
    }
}
