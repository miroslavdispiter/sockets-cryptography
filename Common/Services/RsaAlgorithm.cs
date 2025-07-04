using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.Helpers
{
    public static class RSAEncryptionService
    {
        private static RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);

        public static string PublicKey => rsa.ToXmlString(false);
        public static string PrivateKey => rsa.ToXmlString(true);

        public static string Encrypt(string plainText, string publicKey)
        {
            using (var rsaEncryptor = new RSACryptoServiceProvider(2048))
            {
                rsaEncryptor.FromXmlString(publicKey);
                var data = Encoding.UTF8.GetBytes(plainText);
                var encrypted = rsaEncryptor.Encrypt(data, false);
                return Convert.ToBase64String(encrypted);
            }
        }

        public static string Decrypt(string cipherTextBase64, string privateKey)
        {
            using (var rsaDecryptor = new RSACryptoServiceProvider(2048))
            {
                rsaDecryptor.FromXmlString(privateKey);
                var data = Convert.FromBase64String(cipherTextBase64);
                var decrypted = rsaDecryptor.Decrypt(data, false);
                return Encoding.UTF8.GetString(decrypted);
            }
        }
    }
}