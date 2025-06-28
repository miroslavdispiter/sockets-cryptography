using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public static class DesCryptoHelper
    {
        public static byte[] GenerateDesKeyIvBytes()
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.GenerateKey();
                des.GenerateIV();

                return des.Key.Concat(des.IV).ToArray();
            }
        }
    }
}
