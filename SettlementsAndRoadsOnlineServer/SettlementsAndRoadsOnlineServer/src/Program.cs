using Microsoft.VisualBasic;
using System;
using System.Threading;

namespace SettlementsAndRoadsOnlineServer.src
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "SettlementsAndRoadsOnlineServer";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            // 50 player maximum, server on port 25570
            Server.Start(6, 25570);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started.  Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
