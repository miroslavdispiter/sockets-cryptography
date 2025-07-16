using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public static class CryptoInfoGenerator
    {
        public static byte[] GenerateCryptoPayload(string algoritam)
        {
            List<byte> podaci = new List<byte>();

            byte[] hash = GenerateAlgorithmHashes.ComputeSHA256Hash(algoritam);

            podaci.AddRange(hash);

            if (algoritam == "DES")
            {
                byte[] keyIv = DesCryptoHelper.GenerateDesKeyIvBytes();
                podaci.AddRange(keyIv);
            }
            else if (algoritam == "RSA")
            {
                byte[] publicKey = RsaCryptoHelper.GeneratePublicKeyBytes();
                podaci.AddRange(publicKey);
            }
            return podaci.ToArray();
        }
    }
}
