using Common;
using Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.Services
{
    public static class CommunicationHandler
    {
        public static void HandleTcp(Socket tcpSocket, string desHash, string rsaHash)
        {
            Socket acceptedSocket = tcpSocket.Accept();
            IPEndPoint clientEP = acceptedSocket.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"TCP klijent povezan: {clientEP}");

            byte[] buffer = new byte[1024];
            int received = acceptedSocket.Receive(buffer);
            byte[] validData = buffer.Take(received).ToArray();

            string algoritam = AlgorithmDetector.DetermineAlgorithm(validData, desHash, rsaHash);
            Console.WriteLine($"\nKoristimo {algoritam} algoritam.");

            NacinKomunikacije komunikacija = KomunikacijaHelper.NapraviNacinKomunikacije(acceptedSocket.RemoteEndPoint, validData, algoritam);
            Console.WriteLine("Informacije o komunikaciji: ");
            Console.WriteLine(komunikacija);
        }

        public static void HandleUdp(Socket udpSocket, string desHash, string rsaHash)
        {
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[4096];
            int received = udpSocket.ReceiveFrom(buffer, ref clientEP);
            byte[] validData = buffer.Take(received).ToArray();

            string algoritam = AlgorithmDetector.DetermineAlgorithm(validData, desHash, rsaHash);
            Console.WriteLine($"\nKoristimo {algoritam} algoritam.");

            NacinKomunikacije komunikacija = KomunikacijaHelper.NapraviNacinKomunikacije(clientEP, validData, algoritam);
            Console.WriteLine("Informacije o komunikaciji: ");
            Console.WriteLine(komunikacija);
        }
    }
}
