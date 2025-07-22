using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public static class KomunikacijaHelper
    {
        public static NacinKomunikacije NapraviNacinKomunikacije(EndPoint klijentEP, byte[] podaci, string algoritam)
        {
            string kljuc = "";
            string dodatneInfo = "";

            if (algoritam == "DES")
            {
                byte[] key = podaci.Skip(32).Take(8).ToArray();
                byte[] iv = podaci.Skip(40).Take(8).ToArray();

                kljuc = Convert.ToBase64String(key);
                dodatneInfo = Convert.ToBase64String(iv);
            }
            else if (algoritam == "RSA")
            {
                byte[] publicKeyBytes = podaci.Skip(32).ToArray();

                kljuc = Convert.ToBase64String(publicKeyBytes);
                dodatneInfo = "";
            }

            return new NacinKomunikacije
            {
                KlijentAdresa = klijentEP,
                Algoritam = algoritam,
                Kljuc = kljuc,
                DodatneInfo = dodatneInfo

            };
        }
    }
}