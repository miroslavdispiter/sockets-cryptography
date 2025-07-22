using ClientApp.Services;
using System;
using static ClientApp.Helpers.UserInput;

namespace ClientApp
{
    internal class Client
    {
        static void Main(string[] args)
        {
            string protokol = IzaberiProtokol();
            string algoritam = IzaberiAlgoritam();

            if (protokol == "TCP")
                ClientCommunicationHandler.HandleTcp(algoritam);
            else if (protokol == "UDP")
                ClientCommunicationHandler.HandleUdp(algoritam);
            else
                Console.WriteLine("Nepoznat protokol.");

            Console.ReadLine();
        }
    }
}