using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public class DesAlgorithm
    {
        public string Poruka { get; set; }
        public byte[] Kljuc { get; set; }
        public byte[] IV { get; set; }

        public DesAlgorithm(string poruka, byte[] kljuc, byte[] iV)
        {
            Poruka = poruka;
            Kljuc = kljuc;
            IV = iV;
        }

        public byte[] Encrypt()
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = Kljuc;
                des.IV = IV;

                ICryptoTransform enkriptor = des.CreateEncryptor();
                byte[] porukaBytes = Encoding.UTF8.GetBytes(Poruka);

                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, enkriptor, CryptoStreamMode.Write))
                {
                    cs.Write(porukaBytes, 0, porukaBytes.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        public string Decrypt(byte[] enkriptovaniPodaci)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = Kljuc;
                des.IV = IV;

                ICryptoTransform dekriptor = des.CreateDecryptor();

                using (MemoryStream ms = new MemoryStream(enkriptovaniPodaci))
                using (CryptoStream cs = new CryptoStream(ms, dekriptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
