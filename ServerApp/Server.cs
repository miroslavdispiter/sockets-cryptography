using Common;
using Common.Helpers;
using ServerApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ServerApp
{
    internal class Server
    {
        static readonly IPEndPoint tcp_serverEP = new IPEndPoint(IPAddress.Loopback, 50001);
        static readonly IPEndPoint udp_serverEP = new IPEndPoint(IPAddress.Loopback, 50002);
        static void Main(string[] args)
        {
            Socket serverSocket = null;
            string desHash;
            string rsaHash;

            GenerateAlgorithmHashes.GenerateHash(out desHash, out rsaHash);

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
                CommunicationHandler.HandleTcp(tcpSocket, desHash, rsaHash);
                udpSocket.Close();
            }
            else if (activeSocket == udpSocket)
            {
                CommunicationHandler.HandleUdp(udpSocket, desHash, rsaHash);
                tcpSocket.Close();
            }

            Console.ReadLine();
        }
    }
}