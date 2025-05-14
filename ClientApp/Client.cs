using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    internal class Client
    {
        static void Main(string[] args)
        {
            Socket clientSocket = null;
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 50001);
            string algoritam = string.Empty;

            while (true)
            {
                Console.WriteLine("Koji protokol koristi server? (TCP ili UDP): ");
                string protokol = Console.ReadLine();

                if (protokol.ToLower() == "tcp")
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    clientSocket.Connect(serverEP);
                    Console.WriteLine("Klijent je uspesno povezan sa serverom!");

                    break;
                }
                else if (protokol.ToLower() == "udp")
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    Console.WriteLine("Mozete slati poruke preko UDP protokola!");

                    break;
                }
                else
                {
                    Console.WriteLine("Uneli ste pogresan naziv protokola, pokusajte ponovo");
                    continue;
                }
            }

            while (true)
            {
                Console.WriteLine("Koji algoritam za sifrovanje ce se koristiti? (DES ili RSA): ");
                algoritam = Console.ReadLine();

                if (algoritam.ToLower() == "des" || algoritam.ToLower() == "rsa")
                {
                    Console.WriteLine($"Izabrani algoritam: {algoritam}");
                    break;
                }
                else
                {
                    Console.WriteLine("Uneli ste pogresan naziv algoritma za sifrovanje, pokusajte ponovo");
                }
            }

            Console.ReadLine();
        }
    }
}
