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
            if (algoritam == "DES")
            {
                try
                {
                    Console.WriteLine("\n==================== CLIENT TCP [DES] KOMUNIKACIJA ====================");

                    Console.WriteLine("\n>> Unesi poruku koju želiš da pošalješ serveru:");
                    string poruka = Console.ReadLine();

                    byte[] buffer = new byte[4096];
                    byte[] kljuc = cryptoPayload.Skip(32).Take(8).ToArray();
                    byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                    DesAlgorithm desAlg = new DesAlgorithm(poruka, kljuc, iv);
                    byte[] enkriptovanaPoruka = desAlg.Encrypt();
                    string base64Poruka = Convert.ToBase64String(enkriptovanaPoruka);

                    Console.WriteLine("\n>> Enkriptovana poruka (Base64 format):");
                    Console.WriteLine(base64Poruka);

                    int brBajta = clientSocket.Send(Encoding.UTF8.GetBytes(base64Poruka));

                    brBajta = clientSocket.Receive(buffer);
                    string ehoPoruka = Encoding.UTF8.GetString(buffer, 0, brBajta);

                    Console.WriteLine("\n>> Primljena eho poruka od servera (Base64 format):");
                    Console.WriteLine(ehoPoruka);

                    DesAlgorithm desAlgEho = new DesAlgorithm(ehoPoruka, kljuc, iv);
                    string dekriptovanaPoruka = desAlgEho.Decrypt(Convert.FromBase64String(ehoPoruka));

                    Console.WriteLine("\n>> Dekriptovana eho poruka:");
                    Console.WriteLine(dekriptovanaPoruka);

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
                    Console.WriteLine("\n==================== CLIENT TCP [RSA] KOMUNIKACIJA ====================");

                    byte[] buffer = new byte[4096];
                    int hashLength = 32;

                    byte[] clientPublicKeyBytesRaw = cryptoPayload.Skip(hashLength).ToArray();
                    string clientPublicKeyXml = Encoding.UTF8.GetString(clientPublicKeyBytesRaw);
                    string clientPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientPublicKeyXml));
                    clientSocket.Send(Encoding.UTF8.GetBytes(clientPublicKeyBase64));
                    Console.WriteLine("\n>> Klijent je poslao svoj javni ključ serveru.");

                    int brBajta = clientSocket.Receive(buffer);
                    string serverPublicKeyBase64 = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    string serverPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(serverPublicKeyBase64));
                    Console.WriteLine(">> Primljen javni ključ servera.");

                    Console.Write("\n>> Unesi poruku za slanje serveru: ");
                    string poruka = Console.ReadLine();

                    var rsaEncryptor = new RsaAlgorithm(poruka, serverPublicKeyXml);
                    string enkriptovanaPoruka = rsaEncryptor.Encrypt();

                    byte[] enkriptovanaPorukaBytes = Encoding.UTF8.GetBytes(enkriptovanaPoruka);
                    clientSocket.Send(enkriptovanaPorukaBytes);
                    Console.WriteLine(">> Enkriptovana poruka poslata serveru.");

                    brBajta = clientSocket.Receive(buffer);
                    string odgovorEncrypted = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    Console.WriteLine("\n>> Primljen enkriptovani odgovor od servera.");

                    string clientPrivateKeyXml = RsaCryptoHelper.GetPrivateKeyXml();
                    var rsaDecryptor = new RsaAlgorithm(odgovorEncrypted, clientPrivateKeyXml);
                    string dekriptovanOdgovor = rsaDecryptor.Decrypt();

                    Console.WriteLine("\n>> Dekriptovana poruka servera:");
                    Console.WriteLine(dekriptovanOdgovor);

                    Console.WriteLine("\n======================================================================");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n>> Greška u RSA komunikaciji (TCP): {ex.Message}");
                }
            }
        }

        public static void SendAndReceiveMessageUDP(Socket clientSocket, byte[] cryptoPayload, string algoritam)
        {
            EndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 50002);

            if (algoritam == "DES")
            {
                try
                {
                    Console.WriteLine("\n==================== CLIENT UDP [DES] KOMUNIKACIJA ====================");

                    Console.WriteLine("\n>> Unesi poruku koju želiš da pošalješ serveru:");
                    string poruka = Console.ReadLine();

                    byte[] buffer = new byte[4096];
                    byte[] kljuc = cryptoPayload.Skip(32).Take(8).ToArray();
                    byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                    DesAlgorithm desAlg = new DesAlgorithm(poruka, kljuc, iv);
                    byte[] enkriptovanaPoruka = desAlg.Encrypt();
                    string base64Poruka = Convert.ToBase64String(enkriptovanaPoruka);

                    Console.WriteLine("\n>> Enkriptovana poruka (Base64 format):");
                    Console.WriteLine(base64Poruka);

                    clientSocket.SendTo(Encoding.UTF8.GetBytes(base64Poruka), serverEP);

                    int brBajta = clientSocket.ReceiveFrom(buffer, ref serverEP);
                    string ehoPoruka = Encoding.UTF8.GetString(buffer, 0, brBajta);

                    Console.WriteLine("\n>> Primljena eho poruka od servera (Base64 format):");
                    Console.WriteLine(ehoPoruka);

                    DesAlgorithm desAlgEho = new DesAlgorithm(ehoPoruka, kljuc, iv);
                    string dekriptovanaPoruka = desAlgEho.Decrypt(Convert.FromBase64String(ehoPoruka));

                    Console.WriteLine("\n>> Dekriptovana eho poruka:");
                    Console.WriteLine(dekriptovanaPoruka);

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
                    Console.WriteLine("\n==================== CLIENT UDP [RSA] KOMUNIKACIJA ====================");

                    byte[] buffer = new byte[4096];
                    int hashLength = 32;

                    byte[] clientPublicKeyBytesRaw = cryptoPayload.Skip(hashLength).ToArray();
                    string clientPublicKeyXml = Encoding.UTF8.GetString(clientPublicKeyBytesRaw);
                    string clientPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientPublicKeyXml));
                    clientSocket.SendTo(Encoding.UTF8.GetBytes(clientPublicKeyBase64), serverEP);
                    Console.WriteLine("\n>> Klijent je poslao svoj javni ključ serveru.");

                    int brBajta = clientSocket.ReceiveFrom(buffer, ref serverEP);
                    string serverPublicKeyBase64 = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    string serverPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(serverPublicKeyBase64));
                    Console.WriteLine(">> Primljen javni ključ servera.");

                    Console.Write("\n>> Unesi poruku za slanje serveru: ");
                    string poruka = Console.ReadLine();

                    var rsaEncryptor = new RsaAlgorithm(poruka, serverPublicKeyXml);
                    string enkriptovanaPoruka = rsaEncryptor.Encrypt();

                    clientSocket.SendTo(Encoding.UTF8.GetBytes(enkriptovanaPoruka), serverEP);
                    Console.WriteLine(">> Enkriptovana poruka poslata serveru.");

                    brBajta = clientSocket.ReceiveFrom(buffer, ref serverEP);
                    string odgovorEncrypted = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    Console.WriteLine("\n>> Primljen enkriptovani odgovor od servera.");

                    string clientPrivateKeyXml = RsaCryptoHelper.GetPrivateKeyXml();
                    var rsaDecryptor = new RsaAlgorithm(odgovorEncrypted, clientPrivateKeyXml);
                    string dekriptovanOdgovor = rsaDecryptor.Decrypt();

                    Console.WriteLine("\n>> Dekriptovana poruka servera:");
                    Console.WriteLine(dekriptovanOdgovor);

                    Console.WriteLine("\n======================================================================");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n>> Greška u RSA komunikaciji (UDP): {ex.Message}");
                }
            }
        }
    }
}
