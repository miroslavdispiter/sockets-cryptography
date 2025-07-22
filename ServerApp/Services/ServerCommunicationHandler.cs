using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Helpers;
using Common.Models;
using Common;

namespace ServerApp.Services
{
    public static class ServerCommunicationHandler
    {
        public static void HandleTcpClient(Socket acceptedSocket, string desHash, string rsaHash)
        {
            try
            {
                IPEndPoint clientEP = acceptedSocket.RemoteEndPoint as IPEndPoint;

                byte[] buffer = new byte[1024];
                int received = acceptedSocket.Receive(buffer);
                byte[] validData = buffer.Take(received).ToArray();


                string algoritam = AlgorithmDetector.DetermineAlgorithm(validData, desHash, rsaHash);
                Console.WriteLine($"\nINFO: [TCP {clientEP}] koristi {algoritam} algoritam.");

                Console.WriteLine("\n------------------------------------------------------\n");
                NacinKomunikacije komunikacija = KomunikacijaHelper.NapraviNacinKomunikacije(clientEP, validData, algoritam);
                Console.WriteLine(">> Informacije o komunikaciji:");
                Console.WriteLine(komunikacija);
                Console.WriteLine("------------------------------------------------------");

                ServerNetworkCommunicator.SendAndReceiveMessageTCP(acceptedSocket, validData, algoritam);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n>> Greska u TCP handleru: {ex.Message}");
            }
        }

        public static void HandleUdp(Socket udpSocket, string desHash, string rsaHash)
        {
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[4096];
            int received = udpSocket.ReceiveFrom(buffer, ref clientEP);
            byte[] validData = buffer.Take(received).ToArray();

            string algoritam = AlgorithmDetector.DetermineAlgorithm(validData, desHash, rsaHash);
            Console.WriteLine($"\nINFO: UDP klijent koristi {algoritam} algoritam.");

            Console.WriteLine("\n------------------------------------------------------\n");
            NacinKomunikacije komunikacija = KomunikacijaHelper.NapraviNacinKomunikacije(clientEP, validData, algoritam);
            Console.WriteLine("Informacije o komunikaciji: ");
            Console.WriteLine(komunikacija);
            Console.WriteLine("------------------------------------------------------");

            ServerNetworkCommunicator.SendAndReceiveMessageUDP(udpSocket, validData, algoritam);
        }
    }
}
