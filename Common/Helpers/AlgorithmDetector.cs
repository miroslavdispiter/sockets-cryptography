using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public static class AlgorithmDetector
    {
        public static string DetermineAlgorithm(byte[] receivedHash, string desHash, string rsaHash)
        {
            byte[] first32Bytes = receivedHash.Take(32).ToArray();
            string hashString = BitConverter.ToString(first32Bytes).Replace("-", "");

            if (hashString == desHash)
            {
                return "DES";
            }
            else if (hashString == rsaHash)
            {
                return "RSA";
            }
            else
            {
                Console.WriteLine("Nepoznat hash algoritam.");
                return "NEPOZNAT";
            }
        }
    }
}
