using System;
using System.Collections.Generic;
using System.Text;
using SharedClasses;
using SettlementsAndRoadsOnlineServer.src;

namespace SettlementsAndRoadsOnlineServer.src.GameState
{
    public static class ServerState
    {
        public static List<Player> connectedPlayers;
        public static List<HostedGame> hostedGames;

        public static void Initialize()
        {
            connectedPlayers = new List<Player>();
            hostedGames = new List<HostedGame>();
        }

        public static void PlayerConnected(int _clientId, string _username)
        {
            connectedPlayers.Add(new Player(_clientId, _username));
        }

        public static Player GetPlayer(int _clientId)
        {
            foreach (Player player in connectedPlayers)
            {
                if (player.clientId == _clientId)
                    return player;
            }
            return null;
        }

        public static GameLobbyInfo AddNewHostedGame(int _hostId, string _jsonBoard)
        {
            foreach (Player player in connectedPlayers)
            {
                if (player.clientId == _hostId)
                {
                    HostedGame newGame = new HostedGame(player, _jsonBoard);
                    hostedGames.Add(newGame);
                    return newGame.lobbyInfo;
                }
            }
            return null;
        }

        public static void AddPlayerToHostedGame(int _hostId, int _clientId)
        {
            foreach (HostedGame hostedGame in hostedGames)
            {
                if (hostedGame.players[0].clientId == _hostId)
                {
                    foreach (Player player in connectedPlayers)
                    {
                        if (player.clientId == _clientId)
                        {
                            hostedGame.AddPlayer(player);
                            break;
                        }
                    }
                }
            }
        }

        public static void ChangeReadyStatusOfPlayer(int _clientId)
        {
            HostedGame hostedGame;
            Player player;
            GetHostedGameAndPlayer(_clientId, out hostedGame, out player);
            player.readyStatus = true;
        }

        public static void StartGameIfAllReady(int _hostId)
        {
            HostedGame hostedGame;
            Player player;
            GetHostedGameAndPlayer(_hostId, out hostedGame, out player);
            foreach (Player gamePlayer in hostedGame.players)
            {
                if (!gamePlayer.readyStatus)
                    break;
            }
            foreach (Player gamePlayer in hostedGame.players)
            {
                ServerSend.SendStartMessageToClient(gamePlayer.clientId);
            }
        }

        public static void EndTurn(int _clientId)
        {
            HostedGame hostedGame;
            Player player;
            GetHostedGameAndPlayer(_clientId, out hostedGame, out player);
            hostedGame.IncrementTurn();
        }

        public static bool CheckIfValidTurn(int _clientId)
        {
            HostedGame hostedGame;
            Player player;
            GetHostedGameAndPlayer(_clientId, out hostedGame, out player);
            return player.playerNumber == hostedGame.currentPlayerTurnNumber;
        }

        private static void GetHostedGameAndPlayer(int _clientId, out HostedGame hostedGame, out Player player)
        {
            player = null;
            hostedGame = null;
            foreach (Player connectedPlayer in connectedPlayers)
            {
                if (connectedPlayer.clientId == _clientId)
                {
                    player = connectedPlayer;
                    break;
                }
            }
            foreach (HostedGame game in hostedGames)
            {
                if (player.currentGameHostId == game.players[0].clientId)
                {
                    hostedGame = game;
                    break;
                }
            }
        }
    }
}
