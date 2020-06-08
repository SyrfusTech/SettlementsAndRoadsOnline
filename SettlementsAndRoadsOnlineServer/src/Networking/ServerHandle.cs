using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
