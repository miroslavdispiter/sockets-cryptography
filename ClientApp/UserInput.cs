using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public static class UserInput
    {
        public static string IzaberiProtokol()
        {
            Console.WriteLine("Izaberi protokol: ");
            Console.WriteLine("1 - TCP");
            Console.WriteLine("2 - UDP");

            while (true)
            {
                Console.WriteLine("Unos: ");
                string izbor = Console.ReadLine();

                switch (izbor)
                {
                    case "1":
                        return "TCP";
                    case "2":
                        return "UDP";
                    default:
                        Console.WriteLine("Nepoznat unos. Pokusaj ponovo.");
                        break;
                }
            }
        }

        public static string IzaberiAlgoritam()
        {
            Console.WriteLine("Izaberi algoritam: ");
            Console.WriteLine("1 - DES");
            Console.WriteLine("2 - RSA");

            while (true)
            {
                Console.WriteLine("Unos: ");
                string izbor = Console.ReadLine();

                switch (izbor)
                {
                    case "1":
                        return "DES";
                    case "2":
                        return "RSA";
                    default:
                        Console.WriteLine("Nepoznat unos. Pokusaj ponovo.");
                        break;
                }
            }
        }
    }
}
