using Common.Helpers;
using ServerApp.Services;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading.Tasks;

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
            tcpSocket.Listen(10);
            Console.WriteLine($"INFO: TCP server sluša na {tcp_serverEP}");

            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(udp_serverEP);
            Console.WriteLine($"INFO: UDP server sluša na {udp_serverEP}");

            Console.WriteLine("-----------------------------------------");

            List<Socket> socketsToCheck = new List<Socket>();

            while (true)
            {
                socketsToCheck.Clear();
                socketsToCheck.Add(tcpSocket);
                socketsToCheck.Add(udpSocket);

                Socket.Select(socketsToCheck, null, null, -1);

                foreach (Socket activeSocket in socketsToCheck)
                {
                    if (activeSocket == tcpSocket)
                    {
                        Socket acceptedClient = tcpSocket.Accept();
                        Console.WriteLine($"\nINFO: TCP klijent povezan: {acceptedClient.RemoteEndPoint}");

                        Task.Run(() =>
                        {
                            ServerCommunicationHandler.HandleTcpClient(acceptedClient, desHash, rsaHash);
                        });
                    }
                    else if (activeSocket == udpSocket)
                    {
                        ServerCommunicationHandler.HandleUdp(udpSocket, desHash, rsaHash);
                    }
                }
            }
        }
    }
}


