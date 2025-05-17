using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    internal class Server
    {
        static readonly IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);
        static string desHash;
        static string rsaHash;

        static void Main(string[] args)
        {
            Socket serverSocket = null;

            GenerateAlgotithmHashes(out desHash, out rsaHash);

            string algoritam = string.Empty;

            while (true)
            {
                Console.WriteLine("Unesite protokol koji zelite da koristite ( TCP ili UDP ): ");
                string protokol = Console.ReadLine();

                if (protokol.ToLower() == "tcp")
                {
                    // Povezivanje
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverSocket.Bind(serverEP);

                    serverSocket.Listen(5);
                    Console.WriteLine($"Server je stavljen u stanje osluskivanja i ocekuje komunikaciju na {serverEP}");

                    Socket acceptedSocket = serverSocket.Accept();
                    IPEndPoint clientEP = acceptedSocket.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine($"Povezao se novi klijent! Njegova adresa je {clientEP}");

                    // Primanje hasha

                    byte[] baferHash = new byte[32];
                    int received = acceptedSocket.Receive(baferHash);
                    Console.WriteLine("Primljen hesiran podatak od klijenta.");

                    algoritam = DetermineAlgorithm(baferHash);
                    Console.WriteLine($"Koristimo {algoritam.ToUpper()} algoritam.");

                    break;
                }
                else if (protokol.ToLower() == "udp")
                {
                    // Povezivanje
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    serverSocket.Bind(serverEP);
                    Console.WriteLine($"Server je pokrenut i ceka poruku na: {serverEP}");

                    byte[] baferHash = new byte[32];
                    EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int received = serverSocket.ReceiveFrom(baferHash, ref clientEndPoint);

                    Console.WriteLine($"Primljen heširani podatak od klijenta {clientEndPoint}:");

                    // Primanje hasha
                    algoritam = DetermineAlgorithm(baferHash);
                    Console.WriteLine($"Koristimo {algoritam.ToUpper()} algoritam.");

                    break;
                }
                else
                {
                    Console.WriteLine("Uneli ste pogresan naziv protokola, pokusajte ponovo");
                }
            }
            Console.ReadLine();
        }

        static void GenerateAlgotithmHashes(out string desHashOut, out string rsaHashOut)
        {
            byte[] hashDes = new byte[1024];
            byte[] hashRsa = new byte[1024];

            using (SHA256 sha = SHA256.Create())
            {
                hashDes = sha.ComputeHash(Encoding.UTF8.GetBytes("des"));
                hashRsa = sha.ComputeHash(Encoding.UTF8.GetBytes("rsa"));
            }

            desHashOut = BitConverter.ToString(hashDes).Replace("-", "");
            rsaHashOut = BitConverter.ToString(hashRsa).Replace("-", "");
        }

        static string DetermineAlgorithm(byte[] receivedHash)
        {
            string hashString = BitConverter.ToString(receivedHash).Replace("-", "");
            Console.WriteLine($"Primljen hash: {hashString}");

            if (hashString == desHash)
            {
                return "des";
            }
            else if (hashString == rsaHash)
            {
                return "rsa";
            }
            else
            {
                Console.WriteLine("Nepoznat hash algoritam.");
                return "nepoznat";
            }
        }
    }
}