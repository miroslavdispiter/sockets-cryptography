using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    internal class Server
    {
        static void Main(string[] args)
        {
            Socket serverSocket = null;
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);

            while (true)
            {
                Console.WriteLine("Unesite protokol koji zelite da koristite ( TCP ili UDP ): ");
                string protokol = Console.ReadLine();

                if (protokol.ToLower() == "tcp")
                {
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverSocket.Bind(serverEP);

                    serverSocket.Listen(5);
                    Console.WriteLine($"Server je stavljen u stanje osluskivanja i ocekuje komunikaciju na {serverEP}");

                    Socket acceptedSocket = serverSocket.Accept();
                    IPEndPoint clientEP = acceptedSocket.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine($"Povezao se novi klijent! Njegova adresa je {clientEP}");

                    byte[] baferHash = new byte[32];
                    int received = acceptedSocket.Receive(baferHash);
                    Console.WriteLine("Primljen hesiran podatak od klijenta: ");

                    string hashString = BitConverter.ToString(baferHash, 0, received).Replace("-", "");
                    Console.WriteLine(hashString);

                    break;
                }
                else if (protokol.ToLower() == "udp")
                {
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    serverSocket.Bind(serverEP);
                    Console.WriteLine($"Server je pokrenut i ceka poruku na: {serverEP}");

                    byte[] buffer = new byte[32];
                    EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int received = serverSocket.ReceiveFrom(buffer, ref clientEndPoint);

                    Console.WriteLine($"Primljen heširani podatak od klijenta {clientEndPoint}:");

                    string hashString = BitConverter.ToString(buffer, 0, received).Replace("-", "");
                    Console.WriteLine(hashString);

                    break;
                }
                else
                {
                    Console.WriteLine("Uneli ste pogresan naziv protokola, pokusajte ponovo");
                }
            }
            Console.ReadLine();
        }
    }
}
