using Common;
using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Services
{
    public class NetworkCommunicatorClient
    {
        public static void SendAndReceiveMessage(Socket clientSocket, byte[] cryptoPayload, string protokol, string algoritam)
        {
            if (protokol == "TCP")
            {
                if (algoritam == "DES")
                {
                    try
                    {
                        Console.WriteLine("\nUnesi poruku: ");
                        string poruka = Console.ReadLine();
                        byte[] buffer = new byte[4096];
                        byte[] kljuc = cryptoPayload.Skip(32).Take(8).ToArray();
                        byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();

                        DesAlgorithm desAlg = new DesAlgorithm(poruka, kljuc, iv);

                        byte[] enkriptovanaPoruka = desAlg.Encrypt();

                        int brBajta = clientSocket.Send(Encoding.UTF8.GetBytes(Convert.ToBase64String(enkriptovanaPoruka)));

                        brBajta = clientSocket.Receive(buffer);
                        string ehoPoruka = Encoding.UTF8.GetString(buffer, 0, brBajta);

                        Console.WriteLine($"\nPrimljena eho poruka od servera: {ehoPoruka}");
                        DesAlgorithm desAlgEho = new DesAlgorithm(ehoPoruka, kljuc, iv);

                        string dekriptovanaPoruka = desAlgEho.Decrypt(Convert.FromBase64String(ehoPoruka));
                        Console.WriteLine("Dekriptovana eho poruka: " + dekriptovanaPoruka);
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
                        byte[] buffer = new byte[4096];
                        int hashLength = 32;

           
                        byte[] clientPublicKeyBytesRaw = cryptoPayload.Skip(hashLength).ToArray();
                        string clientPublicKeyXml = Encoding.UTF8.GetString(clientPublicKeyBytesRaw);
                        string clientPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientPublicKeyXml));
                        clientSocket.Send(Encoding.UTF8.GetBytes(clientPublicKeyBase64));

                        int brBajta = clientSocket.Receive(buffer);
                        string serverPublicKeyBase64 = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        string serverPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(serverPublicKeyBase64));
                        Console.WriteLine("Primljen javni ključ servera.");

                        Console.Write("Unesi poruku: ");
                        string poruka = Console.ReadLine();
                        string enkriptovanaPoruka = RSAEncryptionService.Encrypt(poruka, serverPublicKeyXml);

                        byte[] enkriptovanaPorukaBytes = Encoding.UTF8.GetBytes(enkriptovanaPoruka);
                        clientSocket.Send(enkriptovanaPorukaBytes);

                        brBajta = clientSocket.Receive(buffer);
                        string odgovorEncrypted = Encoding.UTF8.GetString(buffer, 0, brBajta);

                        string clientPrivateKey = RsaCryptoHelper.GetPrivateKeyXml();
                        string dekriptovanOdgovor = RSAEncryptionService.Decrypt(odgovorEncrypted, clientPrivateKey);

                        Console.WriteLine("Poruka od servera (dekriptovana): " + dekriptovanOdgovor);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greška u RSA komunikaciji (TCP): {ex.Message}");
                    }
                }
            }
            else // UDP
            {
                if (algoritam == "DES")
                {
                    try
                    {
                        Console.WriteLine("\nUnesi poruku: ");
                        string poruka = Console.ReadLine();
                        byte[] buffer = new byte[4096];
                        byte[] kljuc = cryptoPayload.Skip(32).Take(8).ToArray();
                        byte[] iv = cryptoPayload.Skip(40).Take(8).ToArray();
                        IPEndPoint udp_serverEP = new IPEndPoint(IPAddress.Loopback, 50002);

                        DesAlgorithm desAlg = new DesAlgorithm(poruka, kljuc, iv);
                        byte[] enkriptovanaPoruka = desAlg.Encrypt();
                        string base64Poruka = Convert.ToBase64String(enkriptovanaPoruka);

                        clientSocket.SendTo(Encoding.UTF8.GetBytes(base64Poruka), udp_serverEP);

                        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        int brBajta = clientSocket.ReceiveFrom(buffer, ref remoteEP);
                        string ehoPoruka = Encoding.UTF8.GetString(buffer, 0, brBajta);

                        Console.WriteLine($"\nPrimljena eho poruka od servera: {ehoPoruka}");

                        DesAlgorithm desAlgEho = new DesAlgorithm(string.Empty, kljuc, iv);
                        string dekriptovanaPoruka = desAlgEho.Decrypt(Convert.FromBase64String(ehoPoruka));
                        Console.WriteLine("Dekriptovana eho poruka: " + dekriptovanaPoruka);
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
                        byte[] buffer = new byte[4096];
                        IPEndPoint udp_serverEP = new IPEndPoint(IPAddress.Loopback, 50002);
                        int hashLength = 32;

    
                        byte[] clientPublicKeyBytesRaw = cryptoPayload.Skip(hashLength).ToArray();
                        string clientPublicKeyXml = Encoding.UTF8.GetString(clientPublicKeyBytesRaw);
                        string clientPublicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientPublicKeyXml));
                        clientSocket.SendTo(Encoding.UTF8.GetBytes(clientPublicKeyBase64), udp_serverEP);

                        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        int brBajta = clientSocket.ReceiveFrom(buffer, ref remoteEP);
                        string serverPublicKeyBase64 = Encoding.UTF8.GetString(buffer, 0, brBajta);
                        string serverPublicKeyXml = Encoding.UTF8.GetString(Convert.FromBase64String(serverPublicKeyBase64));
                        Console.WriteLine("Primljen javni ključ servera.");

                        Console.Write("Unesi poruku: ");
                        string poruka = Console.ReadLine();
                        string enkriptovanaPoruka = RSAEncryptionService.Encrypt(poruka, serverPublicKeyXml);

                        byte[] enkriptovanaPorukaBytes = Encoding.UTF8.GetBytes(enkriptovanaPoruka);
                        clientSocket.SendTo(enkriptovanaPorukaBytes, udp_serverEP);

                        brBajta = clientSocket.ReceiveFrom(buffer, ref remoteEP);
                        string odgovorEncrypted = Encoding.UTF8.GetString(buffer, 0, brBajta);

                        string clientPrivateKey = RsaCryptoHelper.GetPrivateKeyXml();
                        string dekriptovanOdgovor = RSAEncryptionService.Decrypt(odgovorEncrypted, clientPrivateKey);

                        Console.WriteLine("Poruka od servera (dekriptovana): " + dekriptovanOdgovor);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greška u RSA komunikaciji (UDP): {ex.Message}");
                    }
                }
            }
        }
    }
}
