using Common;
using Common.Helpers;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.Services
{
    public static class ServerNetworkCommunicator
    {
        public static void SendAndReceiveMessageTCP(Socket serverSocket, byte[] cryptoPayload, string algoritam)
        {
            while (true)
            {
                if (algoritam == "DES")
                {
                    try
                    {
                        Console.WriteLine("\n==================== SERVER TCP [DES] KOMUNIKACIJA ====================");

                        byte[] buffer = new byte[4096];
                        byte[] kljuc = cryptoPayload.Skip(32).Take(8).ToArray();
                        byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                        int brBajta = serverSocket.Receive(buffer);
                        string base64Message = Encoding.UTF8.GetString(buffer, 0, brBajta);

                        Console.WriteLine("\n>> Primljena enkriptovana poruka od klijenta (Base64):");
                        Console.WriteLine(base64Message);

                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        DesAlgorithm desAlg = new DesAlgorithm(base64Message, kljuc, iv);
                        string decryptedMessage = desAlg.Decrypt(Convert.FromBase64String(base64Message));

                        stopwatch.Stop();

                        Console.WriteLine($"\n>> Dekriptovana poruka:");
                        Console.WriteLine(decryptedMessage);
                        Console.WriteLine($">> Vreme dekripcije: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

                        StatisticsManager.DodajStatistiku("DES", decryptedMessage.Length, stopwatch.Elapsed.TotalMilliseconds);

                        Console.WriteLine("\n>> Unesite eho poruku koju želite da pošaljete nazad klijentu:");
                        string echoMessage = Console.ReadLine();

                        DesAlgorithm desAlgEcho = new DesAlgorithm(echoMessage, kljuc, iv);
                        byte[] encryptedMessage = desAlgEcho.Encrypt();
                        string base64EchoMessage = Convert.ToBase64String(encryptedMessage);

                        Console.WriteLine("\n>> Enkriptovana eho poruka (Base64):");
                        Console.WriteLine(base64EchoMessage);

                        serverSocket.Send(Encoding.UTF8.GetBytes(base64EchoMessage));

                        Console.WriteLine("\n================================================================\n");
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("\n>> Došlo je do greške prilikom TCP komunikacije sa klijentom:");
                        Console.WriteLine(ex);
                    }
                }
                else if (algoritam == "RSA")
                {
                    try
                    {
                        Console.WriteLine("\n==================== SERVER TCP [RSA] KOMUNIKACIJA ====================");

                        byte[] buffer = new byte[4096];

                        int brBajta = serverSocket.Receive(buffer);
                        string clientPublicKeyBase64 = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        string clientPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(clientPublicKeyBase64));
                        Console.WriteLine("\n>> Primljen javni ključ od klijenta.");

                        string serverPublicKeyXml = RsaCryptoHelper.GetPublicKeyXml();
                        string serverPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(serverPublicKeyXml));
                        serverSocket.Send(Encoding.UTF8.GetBytes(serverPublicKeyBase64));
                        Console.WriteLine(">> Server je poslao svoj javni ključ klijentu.");

                        brBajta = serverSocket.Receive(buffer);
                        string encryptedMessage = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        Console.WriteLine("\n>> Primljena enkriptovana poruka od klijenta.");

                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        string serverPrivateKeyXml = RsaCryptoHelper.GetPrivateKeyXml();
                        var rsaDecryptor = new RsaAlgorithm(encryptedMessage, serverPrivateKeyXml);
                        string decryptedMessage = rsaDecryptor.Decrypt();

                        stopwatch.Stop();

                        Console.WriteLine(">> Dekriptovana poruka klijenta:");
                        Console.WriteLine(decryptedMessage);
                        Console.WriteLine($">> Vreme dekripcije: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

                        StatisticsManager.DodajStatistiku("RSA", decryptedMessage.Length, stopwatch.Elapsed.TotalMilliseconds);

                        Console.Write("\n>> Unesite eho poruku za slanje klijentu: ");
                        string echoMessage = Console.ReadLine();

                        var rsaEncryptor = new RsaAlgorithm(echoMessage, clientPublicKeyXml);
                        string encryptedResponse = rsaEncryptor.Encrypt();

                        serverSocket.Send(Encoding.UTF8.GetBytes(encryptedResponse));
                        Console.WriteLine(">> Enkriptovani odgovor je poslat klijentu.");

                        Console.WriteLine("\n=====================================================================");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n>> Greška u RSA komunikaciji (TCP): {ex.Message}");
                    }
                }
            }
        }

        public static void SendAndReceiveMessageUDP(Socket serverSocket, byte[] cryptoPayload, string algoritam)
        {
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[4096];

            while (true)
            {
                if (algoritam == "DES")
                {
                    try
                    {
                        Console.WriteLine("\n==================== SERVER UDP [DES] KOMUNIKACIJA ====================");

                        byte[] kljuc = cryptoPayload.Skip(32).Take(8).ToArray();
                        byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                        int brBajta = serverSocket.ReceiveFrom(buffer, ref clientEP);
                        string base64Message = Encoding.UTF8.GetString(buffer, 0, brBajta);

                        Console.WriteLine("\n>> Primljena enkriptovana poruka od klijenta (Base64):");
                        Console.WriteLine(base64Message);

                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        DesAlgorithm desAlg = new DesAlgorithm(base64Message, kljuc, iv);
                        string decryptedMessage = desAlg.Decrypt(Convert.FromBase64String(base64Message));

                        stopwatch.Stop();

                        Console.WriteLine("\n>> Dekriptovana poruka:");
                        Console.WriteLine(decryptedMessage);
                        Console.WriteLine($">> Vreme dekripcije: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

                        StatisticsManager.DodajStatistiku("DES", decryptedMessage.Length, stopwatch.Elapsed.TotalMilliseconds);

                        Console.WriteLine("\n>> Unesite eho poruku koju želite da pošaljete nazad klijentu:");
                        string echoMessage = Console.ReadLine();

                        DesAlgorithm desAlgEcho = new DesAlgorithm(echoMessage, kljuc, iv);
                        byte[] encryptedMessage = desAlgEcho.Encrypt();
                        string base64EchoMessage = Convert.ToBase64String(encryptedMessage);

                        Console.WriteLine("\n>> Enkriptovana eho poruka (Base64):");
                        Console.WriteLine(base64EchoMessage);

                        serverSocket.SendTo(Encoding.UTF8.GetBytes(base64EchoMessage), clientEP);

                        Console.WriteLine("\n================================================================\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n>> Greška u UDP DES komunikaciji: {ex.Message}");
                    }
                }
                else if (algoritam == "RSA")
                {
                    try
                    {
                        Console.WriteLine("\n==================== SERVER UDP [RSA] KOMUNIKACIJA ====================");

                        //int hashLength = 32;

                        int brBajta = serverSocket.ReceiveFrom(buffer, ref clientEP);
                        string clientPublicKeyBase64 = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        string clientPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(clientPublicKeyBase64));
                        Console.WriteLine("\n>> Primljen javni ključ od klijenta.");

                        string serverPublicKeyXml = RsaCryptoHelper.GetPublicKeyXml();
                        string serverPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(serverPublicKeyXml));
                        serverSocket.SendTo(Encoding.UTF8.GetBytes(serverPublicKeyBase64), clientEP);
                        Console.WriteLine(">> Server je poslao svoj javni ključ klijentu.");

                        brBajta = serverSocket.ReceiveFrom(buffer, ref clientEP);
                        string encryptedMessage = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        Console.WriteLine("\n>> Primljena enkriptovana poruka od klijenta.");

                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        string serverPrivateKeyXml = RsaCryptoHelper.GetPrivateKeyXml();
                        var rsaDecryptor = new RsaAlgorithm(encryptedMessage, serverPrivateKeyXml);
                        string decryptedMessage = rsaDecryptor.Decrypt();

                        stopwatch.Stop();

                        Console.WriteLine(">> Dekriptovana poruka klijenta:");
                        Console.WriteLine(decryptedMessage);
                        Console.WriteLine($">> Vreme dekripcije: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

                        StatisticsManager.DodajStatistiku("RSA", decryptedMessage.Length, stopwatch.Elapsed.TotalMilliseconds);

                        Console.Write("\n>> Unesite eho poruku za slanje klijentu: ");
                        string echoMessage = Console.ReadLine();

                        var rsaEncryptor = new RsaAlgorithm(echoMessage, clientPublicKeyXml);
                        string encryptedResponse = rsaEncryptor.Encrypt();

                        serverSocket.SendTo(Encoding.UTF8.GetBytes(encryptedResponse), clientEP);
                        Console.WriteLine(">> Enkriptovani odgovor je poslat klijentu.");

                        Console.WriteLine("\n=====================================================================");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n>> Greška u UDP RSA komunikaciji: {ex.Message}");
                    }
                }
            }
        }
    }
}
