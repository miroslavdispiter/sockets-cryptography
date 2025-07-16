using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Helpers;

namespace ClientApp.Services
{
    public static class ClientCommunicationHandler
    {
        public static void HandleTcp(string algoritam)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 50001);
            clientSocket.Connect(serverEP);

            Console.WriteLine("TCP klijent povezan sa serverom.");

            byte[] payload = CryptoInfoGenerator.GenerateCryptoPayload(algoritam);

            clientSocket.Send(payload);
            Console.WriteLine("TCP klijent poslao hes serveru.");

            ClientNetworkCommunicator.SendAndReceiveMessageTCP(clientSocket, payload, algoritam);
        }

        public static void HandleUdp(string algoritam)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 50002);

            Console.WriteLine("UDP klijent spreman za slanje.");

            byte[] payload = CryptoInfoGenerator.GenerateCryptoPayload(algoritam);

            clientSocket.SendTo(payload, serverEP);
            Console.WriteLine("UDP klijent poslao hes serveru.");

            ClientNetworkCommunicator.SendAndReceiveMessageUDP(clientSocket, payload, algoritam);
        }
    }
}
