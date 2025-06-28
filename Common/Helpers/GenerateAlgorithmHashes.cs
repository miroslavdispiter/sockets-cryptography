using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public static class GenerateAlgorithmHashes
    {
        public static void GenerateHash(out string desHash, out string rsaHash)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] desBytes = sha.ComputeHash(Encoding.UTF8.GetBytes("DES"));
                byte[] rsaBytes = sha.ComputeHash(Encoding.UTF8.GetBytes("RSA"));

                desHash = BitConverter.ToString(desBytes).Replace("-", "");
                rsaHash = BitConverter.ToString(rsaBytes).Replace("-", "");
            }
        }

        public static byte[] ComputeSHA256Hash(string algoritam)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(algoritam));
            }
        }
    }
}
