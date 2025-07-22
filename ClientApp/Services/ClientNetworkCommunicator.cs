using Common;
using Common.Helpers;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Services
{
    public static class ClientNetworkCommunicator
    {
        public static void SendAndReceiveMessageTCP(Socket clientSocket, byte[] cryptoPayload, string algoritam)
        {
            while (true)
            {
                if (algoritam == "DES")
                {
                    try
                    {
                        Console.WriteLine("\n\n\n\n\n================================================================");

                        Console.WriteLine("\n>> Unesi poruku koju želiš da pošalješ serveru:");
                        string message = Console.ReadLine();

                        byte[] buffer = new byte[4096];
                        byte[] key = cryptoPayload.Skip(32).Take(8).ToArray();
                        byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                        DesAlgorithm desAlg = new DesAlgorithm(message, key, iv);
                        byte[] encryptedMessage = desAlg.Encrypt();
                        string base64Message = Convert.ToBase64String(encryptedMessage);

                        Console.WriteLine("\n>> Enkriptovana poruka (Base64 format):");
                        Console.WriteLine(base64Message);

                        int brBajta = clientSocket.Send(Encoding.UTF8.GetBytes(base64Message));
                        Console.WriteLine("\nINFO: Enkriptovana poruka poslata serveru.");

                        brBajta = clientSocket.Receive(buffer);
                        string echoMessage = Encoding.UTF8.GetString(buffer, 0, brBajta);

                        Console.WriteLine("\n>> Primljena eho poruka od servera (Base64 format):");
                        Console.WriteLine(echoMessage);

                        DesAlgorithm desAlgEcho = new DesAlgorithm(echoMessage, key, iv);
                        string decryptedMessage = desAlgEcho.Decrypt(Convert.FromBase64String(echoMessage));

                        Console.WriteLine("\n>> Dekriptovana eho poruka:");
                        Console.WriteLine(decryptedMessage);

                        JeKraj();

                        Console.WriteLine("\n================================================================\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greska: {ex}");
                    }
                }
                else if (algoritam == "RSA")
                {
                    try
                    {
                        Console.WriteLine("\n\n\n\n\n================================================================");

                        byte[] buffer = new byte[4096];
                        int hashLength = 32;

                        byte[] clientPublicKeyBytesRaw = cryptoPayload.Skip(hashLength).ToArray();
                        string clientPublicKeyXml = Encoding.UTF8.GetString(clientPublicKeyBytesRaw);
                        string clientPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientPublicKeyXml));
                        clientSocket.Send(Encoding.UTF8.GetBytes(clientPublicKeyBase64));
                        Console.WriteLine("\nINFO: Klijent je poslao svoj javni ključ serveru.");

                        int brBajta = clientSocket.Receive(buffer);
                        string serverPublicKeyBase64 = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        string serverPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(serverPublicKeyBase64));
                        Console.WriteLine("INFO: Primljen javni ključ servera.");

                        Console.Write("\n>> Unesi poruku za slanje serveru: ");
                        string message = Console.ReadLine();

                        var rsaEncryptor = new RsaAlgorithm(message, serverPublicKeyXml);
                        string encryptedMessage = rsaEncryptor.Encrypt();

                        byte[] encryptedMessageBytes = Encoding.UTF8.GetBytes(encryptedMessage);
                        clientSocket.Send(encryptedMessageBytes);
                        Console.WriteLine("\nINFO: Enkriptovana poruka poslata serveru.");

                        brBajta = clientSocket.Receive(buffer);
                        string echoMessage = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        Console.WriteLine("\nINFO: Primljen enkriptovani odgovor od servera.");

                        string clientPrivateKeyXml = RsaCryptoHelper.GetPrivateKeyXml();
                        var rsaDecryptor = new RsaAlgorithm(echoMessage, clientPrivateKeyXml);
                        string decryptedMessage = rsaDecryptor.Decrypt();

                        Console.WriteLine("\n>> Dekriptovana poruka servera:");
                        Console.WriteLine(decryptedMessage);

                        JeKraj();

                        Console.WriteLine("\n======================================================================");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n>> Greška u RSA komunikaciji (TCP): {ex.Message}");
                    }
                }
            }
        }

        public static void SendAndReceiveMessageUDP(Socket clientSocket, byte[] cryptoPayload, string algoritam)
        {
            EndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 50002);

            while (true)
            {
                if (algoritam == "DES")
                {
                    try
                    {
                        Console.WriteLine("\n\n\n\n\n================================================================");

                        Console.WriteLine("\n>> Unesi poruku koju želiš da pošalješ serveru:");
                        string message = Console.ReadLine();

                        byte[] buffer = new byte[4096];
                        byte[] key = cryptoPayload.Skip(32).Take(8).ToArray();
                        byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                        DesAlgorithm desAlg = new DesAlgorithm(message, key, iv);
                        byte[] encryptedMessage = desAlg.Encrypt();
                        string base64Message = Convert.ToBase64String(encryptedMessage);

                        Console.WriteLine("\n>> Enkriptovana poruka (Base64 format):");
                        Console.WriteLine(base64Message);

                        clientSocket.SendTo(Encoding.UTF8.GetBytes(base64Message), serverEP);
                        Console.WriteLine("\nINFO: Enkriptovana poruka poslata serveru.");

                        int brBajta = clientSocket.ReceiveFrom(buffer, ref serverEP);
                        string echoMessage = Encoding.UTF8.GetString(buffer, 0, brBajta);

                        Console.WriteLine("\n>> Primljena eho poruka od servera (Base64 format):");
                        Console.WriteLine(echoMessage);

                        DesAlgorithm desAlgEcho = new DesAlgorithm(echoMessage, key, iv);
                        string decryptedMessage = desAlgEcho.Decrypt(Convert.FromBase64String(echoMessage));

                        Console.WriteLine("\n>> Dekriptovana eho poruka:");
                        Console.WriteLine(decryptedMessage);

                        JeKraj();

                        Console.WriteLine("\n================================================================\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greska: {ex}");
                    }
                }
                else if (algoritam == "RSA")
                {
                    try
                    {
                        Console.WriteLine("\n\n\n\n\n================================================================");

                        byte[] buffer = new byte[4096];
                        int hashLength = 32;

                        byte[] clientPublicKeyBytesRaw = cryptoPayload.Skip(hashLength).ToArray();
                        string clientPublicKeyXml = Encoding.UTF8.GetString(clientPublicKeyBytesRaw);
                        string clientPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientPublicKeyXml));
                        clientSocket.SendTo(Encoding.UTF8.GetBytes(clientPublicKeyBase64), serverEP);
                        Console.WriteLine("\nINFO: Klijent je poslao svoj javni ključ serveru.");

                        int brBajta = clientSocket.ReceiveFrom(buffer, ref serverEP);
                        string serverPublicKeyBase64 = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        string serverPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(serverPublicKeyBase64));
                        Console.WriteLine("INFO: Primljen javni ključ servera.");

                        Console.Write("\n>> Unesi poruku za slanje serveru: ");
                        string message = Console.ReadLine();

                        var rsaEncryptor = new RsaAlgorithm(message, serverPublicKeyXml);
                        string encryptedMessage = rsaEncryptor.Encrypt();

                        clientSocket.SendTo(Encoding.UTF8.GetBytes(encryptedMessage), serverEP);
                        Console.WriteLine("\nINFO: Enkriptovana poruka poslata serveru.");

                        brBajta = clientSocket.ReceiveFrom(buffer, ref serverEP);
                        string echoMessage = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        Console.WriteLine("\nINFO: Primljen enkriptovani odgovor od servera.");

                        string clientPrivateKeyXml = RsaCryptoHelper.GetPrivateKeyXml();
                        var rsaDecryptor = new RsaAlgorithm(echoMessage, clientPrivateKeyXml);
                        string decryptedMessage = rsaDecryptor.Decrypt();

                        Console.WriteLine("\n>> Dekriptovana poruka servera:");
                        Console.WriteLine(decryptedMessage);

                        JeKraj();

                        Console.WriteLine("\n======================================================================");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n>> Greška u RSA komunikaciji (UDP): {ex.Message}");
                    }
                }
            }
        }

        private static void JeKraj()
        {
            while (true)
            {
                Console.Write("\n>> Da li želite da pošaljete još poruka? (Y/N): ");
                var odgovor = Console.ReadLine()?.Trim().ToUpper();

                if (odgovor == "N")
                {
                    Console.WriteLine("INFO: Klijent završava komunikaciju...");
                    Environment.Exit(0);
                }
                else if (odgovor == "Y")
                {
                    Console.WriteLine("INFO: Nastavljamo sa slanjem poruka...\n");
                    break;
                }
                else
                {
                    Console.WriteLine("INFO: Nepoznat odgovor. Odgovorite ");
                }
            }
        }
    }
}
