using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SettlementsAndRoadsOnlineServer.src.GameState;
using SharedClasses;

namespace SettlementsAndRoadsOnlineServer.src
{
    class ServerHandle
    {
        // The packet handler for when we receive a welcomeReceived message in the Client Class
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int clientIdCheck = _packet.ReadInt();
            string username = _packet.ReadString();

            ServerState.PlayerConnected(_fromClient, username);
            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint}: \"{username}\" connected successfully and is now player {_fromClient}.");
            if (_fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player \"{username}\" (ID: {_fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }
        }

        public static void ChatMessageReceived(int _fromClient, Packet _packet)
        {
            string username = _packet.ReadString();
            string msg = _packet.ReadString();

            ServerSend.SendChatMessageToClients(username, msg);
        }

        public static void JSONBoardReceivedForHost(int _fromClient, Packet _packet)
        {
            string jsonBoard = _packet.ReadString();

            GameLobbyInfo lobbyInfo = ServerState.AddNewHostedGame(_fromClient, jsonBoard);
            Player player = ServerState.GetPlayer(_fromClient);

            string jsonLobbyInfo = JsonSerializer.Serialize(lobbyInfo);
            string jsonPlayer = JsonSerializer.Serialize(player);

            Console.WriteLine($"JsonLobbyInfo: {jsonLobbyInfo}");
            Console.WriteLine($"JsonPlayer: {jsonPlayer}");

            ServerSend.GameHostedSuccessToClient(_fromClient, jsonLobbyInfo, jsonPlayer);
        }
    }
}
