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

namespace ServerApp.Services
{
    public static class ServerNetworkCommunicator
    {
        public static void SendAndReceiveMessageTCP(Socket serverSocket, byte[] cryptoPayload, string algoritam)
        {
            if (algoritam == "DES")
            {
                try
                {
                    Console.WriteLine("\n==================== SERVER TCP KOMUNIKACIJA ====================");

                    byte[] buffer = new byte[4096];
                    byte[] kljuc = cryptoPayload.Skip(32).Take(8).ToArray();
                    byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                    int brBajta = serverSocket.Receive(buffer);
                    string base64Poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);

                    Console.WriteLine("\n>> Primljena enkriptovana poruka od klijenta (Base64):");
                    Console.WriteLine(base64Poruka);

                    DesAlgorithm desAlg = new DesAlgorithm(base64Poruka, kljuc, iv);
                    string dekriptovanaPoruka = desAlg.Decrypt(Convert.FromBase64String(base64Poruka));

                    Console.WriteLine("\n>> Dekriptovana poruka:");
                    Console.WriteLine(dekriptovanaPoruka);

                    Console.WriteLine("\n>> Unesite eho poruku koju želite da pošaljete nazad klijentu:");
                    string odgovor = Console.ReadLine();

                    DesAlgorithm desAlgEho = new DesAlgorithm(odgovor, kljuc, iv);
                    byte[] enkriptovanaPoruka = desAlgEho.Encrypt();
                    string base64EhoPoruka = Convert.ToBase64String(enkriptovanaPoruka);

                    Console.WriteLine("\n>> Enkriptovana eho poruka (Base64):");
                    Console.WriteLine(base64EhoPoruka);

                    serverSocket.Send(Encoding.UTF8.GetBytes(base64EhoPoruka));

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
                    Console.WriteLine("\n==================== SERVER TCP RSA KOMUNIKACIJA ====================");

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

                    string serverPrivateKeyXml = RsaCryptoHelper.GetPrivateKeyXml();
                    var rsaDecryptor = new RsaAlgorithm(encryptedMessage, serverPrivateKeyXml);
                    string decryptedMessage = rsaDecryptor.Decrypt();
                    Console.WriteLine(">> Dekriptovana poruka klijenta:");
                    Console.WriteLine(decryptedMessage);

                    Console.Write("\n>> Unesite eho poruku za slanje klijentu: ");
                    string odgovor = Console.ReadLine();

                    var rsaEncryptor = new RsaAlgorithm(odgovor, clientPublicKeyXml);
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

        public static void SendAndReceiveMessageUDP(Socket serverSocket, byte[] cryptoPayload, string algoritam)
        {
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[4096];

            if (algoritam == "DES")
            {
                try
                {
                    Console.WriteLine("\n==================== SERVER UDP [DES] KOMUNIKACIJA ====================");

                    byte[] kljuc = cryptoPayload.Skip(32).Take(8).ToArray();
                    byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                    int brBajta = serverSocket.ReceiveFrom(buffer, ref clientEP);
                    string base64Poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);

                    Console.WriteLine("\n>> Primljena enkriptovana poruka od klijenta (Base64):");
                    Console.WriteLine(base64Poruka);

                    DesAlgorithm desAlg = new DesAlgorithm(base64Poruka, kljuc, iv);
                    string dekriptovanaPoruka = desAlg.Decrypt(Convert.FromBase64String(base64Poruka));

                    Console.WriteLine("\n>> Dekriptovana poruka:");
                    Console.WriteLine(dekriptovanaPoruka);

                    Console.WriteLine("\n>> Unesite eho poruku koju želite da pošaljete nazad klijentu:");
                    string odgovor = Console.ReadLine();

                    DesAlgorithm desAlgEho = new DesAlgorithm(odgovor, kljuc, iv);
                    byte[] enkriptovanaPoruka = desAlgEho.Encrypt();
                    string base64EhoPoruka = Convert.ToBase64String(enkriptovanaPoruka);

                    Console.WriteLine("\n>> Enkriptovana eho poruka (Base64):");
                    Console.WriteLine(base64EhoPoruka);

                    serverSocket.SendTo(Encoding.UTF8.GetBytes(base64EhoPoruka), clientEP);

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

                    int hashLength = 32;

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

                    string serverPrivateKeyXml = RsaCryptoHelper.GetPrivateKeyXml();
                    var rsaDecryptor = new RsaAlgorithm(encryptedMessage, serverPrivateKeyXml);
                    string decryptedMessage = rsaDecryptor.Decrypt();
                    Console.WriteLine(">> Dekriptovana poruka klijenta:");
                    Console.WriteLine(decryptedMessage);

                    Console.Write("\n>> Unesite eho poruku za slanje klijentu: ");
                    string odgovor = Console.ReadLine();

                    var rsaEncryptor = new RsaAlgorithm(odgovor, clientPublicKeyXml);
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
