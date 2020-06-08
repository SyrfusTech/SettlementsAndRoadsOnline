using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class GameLobbyInfo
    {
        public int maxPlayers;
        public int victoryPointsForWin;
        public bool manualSetup;

        public GameLobbyInfo(int _maxPlayers = 4, int _victoryPointsForWin = 10, bool _manualSetup = false)
        {
            maxPlayers = _maxPlayers;
            victoryPointsForWin = _victoryPointsForWin;
            manualSetup = _manualSetup;
        }
    }

    public struct LobbyInfoContainer
    {
        public GameLobbyInfo lobbyInfo;

        public LobbyInfoContainer(GameLobbyInfo _lobbyInfo)
        {
            lobbyInfo = _lobbyInfo;
        }
    }
}
