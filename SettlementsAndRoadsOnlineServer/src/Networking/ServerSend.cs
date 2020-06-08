using System;
using System.Collections.Generic;
using System.Text;
using SharedClasses;

namespace SettlementsAndRoadsOnlineServer.src
{
    class ServerSend
    {
        // Write the size of the packet to the front of it and call the specified Client's SendData function
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        private static void SendTCPDataToAllExceptOne(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        // Write the size of the packet to the front of it and call the specified Client's SendData function
        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        private static void SendUDPDataToAllExceptOne(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        #region Packets

        public static void Welcome(int _toClient, string _msg)
        {
            // using block automatically calls the Dispose methods
            // Create a welcome packet by passing in the int associated to the ServerPackets.welcome enum as an identifier for what packet type is being sent
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                // Populate the packet with the information desired
                _packet.Write(_msg);
                _packet.Write(_toClient);

                // Send the welcome packet off to the client with the designated id
                SendTCPData(_toClient, _packet);
            }
        }

        public static void SendChatMessageToClients(string _username, string _msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.chatMessageToClients))
            {
                packet.Write($"{_username}: {_msg}");

                SendTCPDataToAll(packet);
            }
        }

        public static void GameHostedSuccessToClient(int _toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.gameHostedSuccessfully))
            {
                SendTCPData(_toClient, packet);
            }
        }

        public static void SendStartMessageToClient(int _toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.startGame))
            {
                SendTCPData(_toClient, packet);
            }
        }
        #endregion
    }
}
