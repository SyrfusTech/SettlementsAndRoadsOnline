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

            // Starting Thread which will handle the logic of how IO from network will be handled
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            // 50 player maximum, server on port 25570
            Server.Start(6, 25570);
        }

        // Thread that holds the server side GameLoop and also ensures that the tick rate of the server runs at the speed defined in the Constants class
        private static void MainThread()
        {
            Console.WriteLine($"Main thread started.  Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    // This is the call to the function that determines what to do with the post-processed(handled) IO from the NetworkStream
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
