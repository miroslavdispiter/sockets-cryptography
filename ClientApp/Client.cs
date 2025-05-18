using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    internal class Client
    {
        static string algoritam = string.Empty;

        static void Main(string[] args)
        {
            Socket clientSocket = null;
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 50001);

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
                Console.WriteLine("\nKoji algoritam za sifrovanje ce se koristiti? (DES ili RSA): ");
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

            if (clientSocket.ProtocolType == ProtocolType.Tcp)
            {
                clientSocket.Send(SendCryptoInfo());
                Console.WriteLine("Hes poslat TCP serveru.");
            }
            else if (clientSocket.ProtocolType == ProtocolType.Udp)
            {
                clientSocket.SendTo(SendCryptoInfo(), serverEP);
                Console.WriteLine("Hes poslat UDP serveru.");
            }

            try
            {
                Console.WriteLine("\nUnesite poruku: ");
                string poruka = Console.ReadLine();
                byte[] kljuc = SendCryptoInfo().Skip(32).Take(8).ToArray();
                byte[] iv = SendCryptoInfo().Skip(40).Take(8).ToArray();

                DesAlgorithm desAlg = new DesAlgorithm(poruka, kljuc, iv);

                byte[] enkriptovanaPoruka = desAlg.Encrypt();
                Console.WriteLine("Enkriptovana poruka (Base64): " + Convert.ToBase64String(enkriptovanaPoruka));

                string dekriptovana = desAlg.Decrypt(enkriptovanaPoruka);
                Console.WriteLine("Dekriptovana poruka: " + dekriptovana);

                Console.ReadLine();
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
        }

        static byte[] SendCryptoInfo()
        {
            List<byte> podaci = new List<byte>();

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(algoritam));
                podaci.AddRange(hash);
                //Console.WriteLine("Hesirana vrednost algoritma: " + BitConverter.ToString(hash).Replace("-", ""));
            }

            if (algoritam.ToLower() == "des")
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    des.GenerateKey();
                    des.GenerateIV();

                    podaci.AddRange(des.Key);
                    podaci.AddRange(des.IV);

                    //Console.WriteLine("DES kljuc: " + BitConverter.ToString(des.Key));
                    //Console.WriteLine("DES IV: " + BitConverter.ToString(des.IV));
                }
            }
            else if (algoritam.ToLower() == "rsa")
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    string publicKeyXml = rsa.ToXmlString(false);

                    byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKeyXml);

                    podaci.AddRange(publicKeyBytes);

                    //Console.WriteLine("RSA javni kljuc: " + publicKeyXml);
                }
            }
            return podaci.ToArray();
        }
    }
}