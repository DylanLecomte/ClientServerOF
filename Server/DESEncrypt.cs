using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    class DESEncrypt
    {

        static TripleDES CreateDES(string key)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            TripleDES des = new TripleDESCryptoServiceProvider();
            des.Key = md5.ComputeHash(Encoding.Unicode.GetBytes(key));
            des.IV = new byte[des.BlockSize / 8];
            return des;
        }

        public string EncryptString(string plainText, string password)
        {
            // Conversion de la chaine en "byte array"
            byte[] plainTextBytes = Encoding.Unicode.GetBytes(plainText);

            // Utilisation d'un memory string
            MemoryStream myStream = new MemoryStream();

            // Creation de la cle et initialisation d'un vector utilisant le mot de passe
            TripleDES des = CreateDES(password);

            // Creation de l'encoder qui écrira dans le memory stream
            CryptoStream cryptStream = new CryptoStream(myStream, des.CreateEncryptor(), CryptoStreamMode.Write);

            // On utilise maintenant le cryptstream pour écrire l'"array byte" dans le memory stream
            cryptStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptStream.FlushFinalBlock();

            // On converti le cryptstream en une version de affichable de notre chaine chiffrée
            return Convert.ToBase64String(myStream.ToArray());
        }

        public string DecryptString(string encryptedText, string password)
        {
            // Conversion de la chaine en "Byte array"
            byte[] encryptedTextBytes = Convert.FromBase64String(encryptedText);

            // Utilisation d'un memory stream
            MemoryStream myStream = new MemoryStream();

            // Creation de la cle et initialisation d'un vector utilisant le mot de passe
            TripleDES des = CreateDES(password);

            // Creation de l'encoder qui écrira dans le memory stream
            CryptoStream decryptStream = new CryptoStream(myStream, des.CreateDecryptor(), CryptoStreamMode.Write);

            // On utilise le decrypt stream pour écrire l'array byte dans le memory stream
            decryptStream.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);
            decryptStream.FlushFinalBlock();

            // Conversion de notre crypt stream en chaine affichable
            return Encoding.Unicode.GetString(myStream.ToArray());
        }
    }
}
