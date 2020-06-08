using System;
using System.Collections.Generic;
using System.Text;

namespace SettlementsAndRoadsOnlineServer.src
{
    class GameLogic
    {
        // Function that is called ocne every server tick
        public static void Update()
        {
            ThreadManager.UpdateMain();
        }
    }
}
