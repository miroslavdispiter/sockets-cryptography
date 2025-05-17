using Common;
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
                    byte[] baferHash = new byte[48];
                    try
                    {
                        int received = acceptedSocket.Receive(baferHash);
                        algoritam = DetermineAlgorithm(baferHash);
                        Console.WriteLine($"\nKoristimo {algoritam.ToUpper()} algoritam.");

                        // Pravljenje objekta NacinKomunikacije
                        NacinKomunikacije komunikacija = NapraviNacinKomunikacije(acceptedSocket.RemoteEndPoint, baferHash, algoritam);
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
                    serverSocket.Bind(serverEP);
                    Console.WriteLine($"Server je pokrenut i ceka poruku na: {serverEP}");

                    // Primanje hasha
                    EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] baferHash = new byte[1024];
                    try
                    {
                        int received = serverSocket.ReceiveFrom(baferHash, ref clientEndPoint);
                        //Console.WriteLine($"Primljen heširani podatak od klijenta {clientEndPoint}.");
                        algoritam = DetermineAlgorithm(baferHash);
                        Console.WriteLine($"\nKoristimo {algoritam.ToUpper()} algoritam.");
                    }
                    catch(SocketException ex)
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
            byte[] first32Bytes = receivedHash.Take(32).ToArray();
            string hashString = BitConverter.ToString(first32Bytes).Replace("-", "");
            //Console.WriteLine($"\nPrimljena hesirana vrednost: {hashString}");

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

        static NacinKomunikacije NapraviNacinKomunikacije(EndPoint klijentEP, byte[] primljeniPodaci, string algoritam)
        {
            string kljuc = "";
            string dodatneInfo = "";

            if (algoritam == "des")
            {
                byte[] key = primljeniPodaci.Skip(32).Take(8).ToArray();
                byte[] iv = primljeniPodaci.Skip(40).Take(8).ToArray();

                kljuc = Convert.ToBase64String(key);
                dodatneInfo = Convert.ToBase64String(iv);
            }
            else if (algoritam == "rsa")
            {
                byte[] publicKeyBytes = primljeniPodaci.Skip(32).ToArray();

                kljuc = Convert.ToBase64String(publicKeyBytes);
                dodatneInfo = "";
            }

            return new NacinKomunikacije()
            {
                KlijentAdresa = klijentEP,
                Algoritam = algoritam,
                Kljuc = kljuc,
                DodatneInfo = dodatneInfo
            };
        }
    }
}