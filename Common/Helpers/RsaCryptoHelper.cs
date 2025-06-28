using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public static class RsaCryptoHelper
    {
        public static byte[] GeneratePublicKeyBytes()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                string publicKeyXml = rsa.ToXmlString(false);
                return Encoding.UTF8.GetBytes(publicKeyXml);
            }
        }
    }
}
