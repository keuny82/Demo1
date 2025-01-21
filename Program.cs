using System;
using System.Net;

namespace Demo1
{
    class Program
    {
        static void Main(string[] args)
        {
            IOCPServer server = new IOCPServer(IPAddress.Parse("127.0.0.1"), 11000, "Server=localhost;Port=3306;Database=keuny82;Uid=root;Pwd=!2rjsdndkQk");
            server.StartServer();

            while(true)
            {
                string cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "roomcount": Console.WriteLine($"TotalRoomCount : {server.GetRoomManager().GetCurrentRoomCount()}"); break;
                    default: break;
                }
            }
        }
    }
}
