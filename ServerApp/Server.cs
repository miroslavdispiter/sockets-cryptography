using Common;
using Common.Helpers;
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
        static readonly IPEndPoint tcp_serverEP = new IPEndPoint(IPAddress.Any, 50001);
        static readonly IPEndPoint udp_serverEP = new IPEndPoint(IPAddress.Any, 50002);
        static string desHash;
        static string rsaHash;

        static void Main(string[] args)
        {
            Socket serverSocket = null;

            GenerateAlgorithmHashes.GenerateHash(out desHash, out rsaHash);

            string algoritam = string.Empty;

            while (true)
            {
                Console.WriteLine("Unesite protokol koji zelite da koristite ( TCP ili UDP ): ");
                string protokol = Console.ReadLine();

                if (protokol.ToLower() == "tcp")
                {
                    // Povezivanje
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverSocket.Bind(tcp_serverEP);

                    serverSocket.Listen(5);
                    Console.WriteLine($"Server je stavljen u stanje osluskivanja i ocekuje komunikaciju na {tcp_serverEP}");

                    Socket acceptedSocket = serverSocket.Accept();
                    IPEndPoint clientEP = acceptedSocket.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine($"Povezao se novi klijent! Njegova adresa je {clientEP}");

                    // Primanje hasha
                    byte[] baferHash = new byte[1024];
                    try
                    {
                        int received = acceptedSocket.Receive(baferHash);
                        byte[] validData = baferHash.Take(received).ToArray();
                        algoritam = AlgorithmDetector.DetermineAlgorithm(validData, desHash, rsaHash);
                        Console.WriteLine($"\nKoristimo {algoritam.ToUpper()} algoritam.");

                        // Pravljenje objekta NacinKomunikacije
                        NacinKomunikacije komunikacija = KomunikacijaHelper.NapraviNacinKomunikacije(acceptedSocket.RemoteEndPoint, validData, algoritam);
                        Console.WriteLine("Informacije o komunikaciji: ");
                        Console.WriteLine(komunikacija);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine(ex);
                    }
                    break;
                }
                else if (protokol.ToLower() == "udp")
                {
                    // Povezivanje
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    serverSocket.Bind(udp_serverEP);
                    Console.WriteLine($"Server je pokrenut i ceka poruku na: {udp_serverEP}");

                    // Primanje hasha
                    EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] baferHash = new byte[4096];
                    try
                    {
                        int received = serverSocket.ReceiveFrom(baferHash, ref clientEndPoint);
                        //Console.WriteLine($"Primljen heširani podatak od klijenta {clientEndPoint}.");
                        byte[] validData = baferHash.Take(received).ToArray();
                        algoritam = AlgorithmDetector.DetermineAlgorithm(validData, desHash, rsaHash);
                        Console.WriteLine($"\nKoristimo {algoritam.ToUpper()} algoritam.");

                        // Pravljenje objekta NacinKomunikacije
                        NacinKomunikacije komunikacija = KomunikacijaHelper.NapraviNacinKomunikacije(clientEndPoint, validData, algoritam);
                        Console.WriteLine("Informacije o komunikaciji: ");
                        Console.WriteLine(komunikacija); 
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine(ex);
                    }
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