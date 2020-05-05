using System;

namespace SettlementsAndRoadsOnlineServer.src
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SettlementsAndRoadsOnlineServer";

            // 50 player maximum, server on port 25570
            Server.Start(6, 25570);

            Console.ReadKey();
        }
    }
}
