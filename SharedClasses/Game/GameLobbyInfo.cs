using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class GameLobbyInfo
    {
        public int maxPlayers { get; set; }
        public int victoryPointsForWin { get; set; }
        public bool manualSetup { get; set; }

        public GameLobbyInfo(int _maxPlayers = 4, int _victoryPointsForWin = 10, bool _manualSetup = false)
        {
            maxPlayers = _maxPlayers;
            victoryPointsForWin = _victoryPointsForWin;
            manualSetup = _manualSetup;
        }
    }
}
