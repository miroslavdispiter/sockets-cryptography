using ClientApp.Services;
using Common;
using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static ClientApp.UserInput;

namespace ClientApp
{
    internal class Client
    {

        static void Main(string[] args)
        {
            Socket clientSocket = null;
            IPEndPoint tcp_serverEP = new IPEndPoint(IPAddress.Loopback, 50001);
            IPEndPoint udp_serverEP = new IPEndPoint(IPAddress.Loopback, 50002);

            string protokol = IzaberiProtokol();

            if (protokol == "TCP")
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                clientSocket.Connect(tcp_serverEP);
                Console.WriteLine("Klijent je uspesno povezan sa serverom!");
            }
            else if (protokol == "UDP")
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Console.WriteLine("Mozete slati poruke preko UDP protokola!");
            }

            string algoritam = IzaberiAlgoritam();

            byte[] cryptoPayload = CryptoInfoGenerator.GenerateCryptoPayload(algoritam);

            if (clientSocket.ProtocolType == ProtocolType.Tcp)
            {
                clientSocket.Send(cryptoPayload);
                Console.WriteLine("Hes poslat TCP serveru.");
                NetworkCommunicatorClient.SendAndReceiveMessage(clientSocket, cryptoPayload, protokol, algoritam);
            }
            else if (clientSocket.ProtocolType == ProtocolType.Udp)
            {
                clientSocket.SendTo(cryptoPayload, udp_serverEP);
                Console.WriteLine("Hes poslat UDP serveru.");
                NetworkCommunicatorClient.SendAndReceiveMessage(clientSocket, cryptoPayload, protokol, algoritam);
            }
            
            Console.ReadLine();
        }
    }
}