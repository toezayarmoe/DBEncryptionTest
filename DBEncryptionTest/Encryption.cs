using System.Security.Cryptography;
using System.Text;

namespace DBEncryptionTest
{
    public class Encryption
    {
        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string DecryptConnectionString(string connectionString)
        {
            string userIdKey = "User Id=";
            string passwordKey = "Password=";

            // Find and decrypt User Id
            int userIdStart = connectionString.IndexOf(userIdKey) + userIdKey.Length;
            int userIdEnd = connectionString.IndexOf(';', userIdStart);
            if (userIdStart >= userIdKey.Length && userIdEnd > userIdStart)
            {
                string encryptedUserId = connectionString.Substring(userIdStart, userIdEnd - userIdStart);
                string decryptedUserId = DecryptString("b14ca5898a4e4133bbce2ea2315a1916", encryptedUserId);
                connectionString = connectionString.Replace(encryptedUserId, decryptedUserId);
            }

            // Find and decrypt Password
            int passwordStart = connectionString.IndexOf(passwordKey) + passwordKey.Length;
            int passwordEnd = connectionString.IndexOf(';', passwordStart);
            if (passwordStart >= passwordKey.Length && passwordEnd > passwordStart)
            {
                string encryptedPassword = connectionString.Substring(passwordStart, passwordEnd - passwordStart);
                string decryptedPassword = DecryptString("b14ca5898a4e4133bbce2ea2315a1916", encryptedPassword);
                connectionString = connectionString.Replace(encryptedPassword, decryptedPassword);
            }

            return connectionString;
        }
    }
}
