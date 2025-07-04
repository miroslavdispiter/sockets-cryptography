using Common;
using Common.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerApp.Services
{
    public static class NetworkCommunicatorServer
    {
        public static void SendAndReceiveMessageTCP(Socket serverSocket, byte[] cryptoPayload, string algoritam)
        {
            if (algoritam == "DES")
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    byte[] kljuc = cryptoPayload.Skip(32).Take(8).ToArray();
                    byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                    int brBajta = serverSocket.Receive(buffer);
                    string poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    Console.WriteLine($"\nPrimljena poruka od klijenta: {poruka}");

                    DesAlgorithm desAlg = new DesAlgorithm(poruka, kljuc, iv);
                    string dekriptovanaPoruka = desAlg.Decrypt(Convert.FromBase64String(poruka));
                    Console.WriteLine("Dekriptovana poruka: " + dekriptovanaPoruka);

                    Console.WriteLine("Unesite eho poruku: ");
                    string odgovor = Console.ReadLine();
                    DesAlgorithm desAlgEho = new DesAlgorithm(odgovor, kljuc, iv);
                    byte[] enkriptovanaPoruka = desAlgEho.Encrypt();
                    Console.WriteLine("\nEnkriptovana poruka (Base64): " + Convert.ToBase64String(enkriptovanaPoruka));

                    brBajta = serverSocket.Send(Encoding.UTF8.GetBytes(Convert.ToBase64String(enkriptovanaPoruka)));
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske:\n{ex}");
                }
            }
            else if (algoritam == "RSA")
            {
                try
                {
                    byte[] buffer = new byte[4096];

                    int brBajta = serverSocket.Receive(buffer);
                    string clientPublicKeyBase64 = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    string clientPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(clientPublicKeyBase64));
                    Console.WriteLine("Primljen javni ključ klijenta.");

                    string serverPublicKeyXml = RsaCryptoHelper.GetPublicKeyXml();
                    string serverPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(serverPublicKeyXml));
                    byte[] serverPublicKeyBytes = Encoding.UTF8.GetBytes(serverPublicKeyBase64);
                    serverSocket.Send(serverPublicKeyBytes);

                    brBajta = serverSocket.Receive(buffer);
                    string encryptedMessage = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    Console.WriteLine("Primljena enkriptovana poruka od klijenta.");

                    string serverPrivateKey = RsaCryptoHelper.GetPrivateKeyXml();
                    string decryptedMessage = RSAEncryptionService.Decrypt(encryptedMessage, serverPrivateKey);
                    Console.WriteLine("Dekriptovana poruka: " + decryptedMessage);

                    Console.Write("Unesi eho poruku: ");
                    string odgovor = Console.ReadLine();

                    string encryptedResponse = RSAEncryptionService.Encrypt(odgovor, clientPublicKeyXml);

                    byte[] encryptedResponseBytes = Encoding.UTF8.GetBytes(encryptedResponse);
                    serverSocket.Send(encryptedResponseBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška u RSA komunikaciji (TCP): {ex.Message}");
                }
            }
        }

        public static void SendAndReceiveMessageUDP(Socket serverSocket, byte[] cryptoPayload, string algoritam)
        {
            if (algoritam == "DES")
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

                    int brBajta = serverSocket.ReceiveFrom(buffer, ref clientEP);
                    string base64Poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    Console.WriteLine($"\nPrimljena enkriptovana poruka od klijenta (Base64): {base64Poruka}");

                    byte[] kljuc = cryptoPayload.Skip(32).Take(8).ToArray();
                    byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                    DesAlgorithm desAlg = new DesAlgorithm(string.Empty, kljuc, iv);
                    string dekriptovana = desAlg.Decrypt(Convert.FromBase64String(base64Poruka));
                    Console.WriteLine("Dekriptovana poruka: " + dekriptovana);

                    Console.Write("\nUnesi poruku za klijenta: ");
                    string odgovor = Console.ReadLine();

                    DesAlgorithm desAlgOdgovor = new DesAlgorithm(odgovor, kljuc, iv);
                    byte[] enkriptovanOdgovor = desAlgOdgovor.Encrypt();
                    string ehoBase64 = Convert.ToBase64String(enkriptovanOdgovor);

                    serverSocket.SendTo(Encoding.UTF8.GetBytes(ehoBase64), clientEP);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greska na serveru (UDP): {ex}");
                }
            }
            else if (algoritam == "RSA")
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

                    int brBajta = serverSocket.ReceiveFrom(buffer, ref clientEP);
                    string clientPublicKeyBase64 = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    string clientPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(clientPublicKeyBase64));
                    Console.WriteLine("Primljen javni ključ klijenta.");

                    string serverPublicKeyXml = RsaCryptoHelper.GetPublicKeyXml();
                    string serverPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(serverPublicKeyXml));
                    byte[] serverPublicKeyBytes = Encoding.UTF8.GetBytes(serverPublicKeyBase64);
                    serverSocket.SendTo(serverPublicKeyBytes, clientEP);

                    brBajta = serverSocket.ReceiveFrom(buffer, ref clientEP);
                    string encryptedMessage = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    Console.WriteLine("Primljena enkriptovana poruka od klijenta.");

                    string serverPrivateKey = RsaCryptoHelper.GetPrivateKeyXml();
                    string decryptedMessage = RSAEncryptionService.Decrypt(encryptedMessage, serverPrivateKey);
                    Console.WriteLine("Dekriptovana poruka: " + decryptedMessage);

                    Console.Write("Unesi poruku za klijenta: ");
                    string odgovor = Console.ReadLine();

                    string encryptedResponse = RSAEncryptionService.Encrypt(odgovor, clientPublicKeyXml);

                    byte[] encryptedResponseBytes = Encoding.UTF8.GetBytes(encryptedResponse);
                    serverSocket.SendTo(encryptedResponseBytes, clientEP);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška u RSA komunikaciji (UDP): {ex.Message}");
                }
            }
        }
    }
}
