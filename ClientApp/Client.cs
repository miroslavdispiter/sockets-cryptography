using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    internal class Client
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint destinationEP = new IPEndPoint(IPAddress.Loopback, 50001);

            while (true)
            {
                try
                {
                    Console.WriteLine("Unesite poruku: ");
                    string poruka = Console.ReadLine();
                    byte[] binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                    clientSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, destinationEP);
                    Console.WriteLine($"Klijent poslao poruku na: {destinationEP}");
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
