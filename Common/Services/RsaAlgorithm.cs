using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public class RsaAlgorithm
    {
        public string Poruka { get; set; }
        public string Ključ { get; set; }

        public RsaAlgorithm(string poruka, string kljuc)
        {
            Poruka = poruka;
            Ključ = kljuc;
        }

        public string Encrypt()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(Ključ);
                byte[] podaci = Encoding.UTF8.GetBytes(Poruka);
                byte[] enkriptovani = rsa.Encrypt(podaci, false);
                return Convert.ToBase64String(enkriptovani);
            }
        }

        public string Decrypt()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(Ključ);
                byte[] podaci = Convert.FromBase64String(Poruka);
                byte[] dekriptovani = rsa.Decrypt(podaci, false);
                return Encoding.UTF8.GetString(dekriptovani);
            }
        }
    }
}
