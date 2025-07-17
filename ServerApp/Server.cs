using Common.Helpers;
using ServerApp.Services;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace ServerApp
{
    internal class Server
    {
        static readonly IPEndPoint tcp_serverEP = new IPEndPoint(IPAddress.Loopback, 50001);
        static readonly IPEndPoint udp_serverEP = new IPEndPoint(IPAddress.Loopback, 50002);

        static void Main(string[] args)
        {
            byte[] desHashBytes = GenerateAlgorithmHashes.ComputeSHA256Hash("DES");
            byte[] rsaHashBytes = GenerateAlgorithmHashes.ComputeSHA256Hash("RSA");

            string desHash = GenerateAlgorithmHashes.ToHexString(desHashBytes);
            string rsaHash = GenerateAlgorithmHashes.ToHexString(rsaHashBytes);

            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Bind(tcp_serverEP);
            tcpSocket.Listen(5);
            Console.WriteLine($"TCP server slusa na {tcp_serverEP}");

            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(udp_serverEP);
            Console.WriteLine($"UDP server slusa na {udp_serverEP}");

            List<Socket> socketsToCheck = new List<Socket> { tcpSocket, udpSocket };
            Socket.Select(socketsToCheck, null, null, -1);
            Socket activeSocket = socketsToCheck.First();

            if (activeSocket == tcpSocket)
            {
                udpSocket.Close();
                ServerCommunicationHandler.HandleTcp(tcpSocket, desHash, rsaHash);
            }
            else if (activeSocket == udpSocket)
            {
                tcpSocket.Close();
                ServerCommunicationHandler.HandleUdp(udpSocket, desHash, rsaHash);
            }

            StatisticsManager.PrikaziStatistiku();

            Console.ReadLine();
        }
    }
}
