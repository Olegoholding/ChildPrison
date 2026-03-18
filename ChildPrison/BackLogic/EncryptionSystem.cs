using System.Security.Cryptography;

namespace ChildPrison.BackLogic
{
    internal class EncryptionSystem : DatabaseProvider
    {
        #region -- Константы --

        const int Size = 24; 
        const int Itr = 100000; 
        #endregion

        public byte[] CreateHash(string password, string login, byte[] salt)
        {
            byte[] hash = new byte[Size];
            Rfc2898DeriveBytes pbkdf2;
            if (salt == null)
            {
                salt = new byte[Size];
                RNGCryptoServiceProvider provider = new();
                provider.GetBytes(salt);

                pbkdf2 = new(password, salt, Itr);
                hash = pbkdf2.GetBytes(Size);
                //string readableHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                byte[][] data = new byte[2][];
                data[0] = salt; data[1] = hash;
                addUser(login, data);
                return hash;
            }
            pbkdf2 = new(password, salt, Itr);
            hash = pbkdf2.GetBytes(Size);
            return hash;
        }
    }
}