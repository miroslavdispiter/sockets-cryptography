using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    internal class Server
    {
        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);
            serverSocket.Bind(serverEP);

            Console.WriteLine($"Server je pokrenut i ceka poruku na adresi: {serverEP}");

            EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] prijemniBafer = new byte[1024];

            while (true)
            {
                try
                {
                    int brBajta = serverSocket.ReceiveFrom(prijemniBafer, ref senderEndPoint);
                    string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
                    Console.WriteLine($"Server primio poruku: {poruka}");
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
