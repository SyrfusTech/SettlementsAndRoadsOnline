using System;
using System.Collections.Generic;
using System.Text;
using SharedClasses;

namespace SettlementsAndRoadsOnlineServer.src.GameState
{
    public class HostedGame
    {
        public List<Player> players;

        public string jsonBoard;
        public GameLobbyInfo lobbyInfo;

        public bool inProgress;
        public int currentPlayerTurnNumber;

        public HostedGame(Player host, string _jsonBoard)
        {
            players = new List<Player>();
            jsonBoard = _jsonBoard;
            lobbyInfo = new GameLobbyInfo();
            inProgress = false;
            currentPlayerTurnNumber = 0;

            host.readyStatus = true;
            AddPlayer(host);
        }

        public void AddPlayer(Player _player)
        {
            players.Add(_player);
            players[players.Count - 1].currentGameHostId = players[0].clientId;
            if (players.Count >= 2)
                players[players.Count - 1].playerNumber = players[players.Count - 2].playerNumber;
            else
                players[players.Count - 1].playerNumber = 0;
        }
        
        public void StartGame()
        {
            inProgress = true;
        }

        public void IncrementTurn()
        {
            currentPlayerTurnNumber = SyrfusMath.Mod(currentPlayerTurnNumber + 1, players.Count);
        }
    }
}
