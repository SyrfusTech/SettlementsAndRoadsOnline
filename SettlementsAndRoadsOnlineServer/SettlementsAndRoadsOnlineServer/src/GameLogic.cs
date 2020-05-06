using System;
using System.Collections.Generic;
using System.Text;

namespace SettlementsAndRoadsOnlineServer.src
{
    class GameLogic
    {
        // At the moment a pointless function however we can put whatever we want in here so this is where the actual Server Logic is going to reside
        public static void Update()
        {
            ThreadManager.UpdateMain();
        }
    }
}
