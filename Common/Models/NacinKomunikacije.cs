using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class NacinKomunikacije
    {
        public EndPoint KlijentAdresa { get; set; }
        public string Algoritam { get; set; }
        public string Kljuc { get; set; }
        public string DodatneInfo { get; set; }

        public override string ToString()
        {
            return $"Klijent: {KlijentAdresa}\nAlgoritam: {Algoritam}\nKljuc: {Kljuc}\nDodatno: {DodatneInfo}\n";
        }
    }
}
