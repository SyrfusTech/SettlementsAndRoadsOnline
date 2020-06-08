using System.Collections;
using System.Collections.Generic;
using SharedClasses;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    // Function to send TCPData packets to the server
    private static void SendTCPData(Packet _packet)
    {
        // Always write the length of the packet first
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    // Function to send UDPData packets to the server
    private static void SendUDPData(Packet _packet)
    {
        // Always write the length of the packet first
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets

    // Welcome Received packet that we can send to Server
    public static void WelcomeReceived()
    {
        // As always add the ClientPackets.welcomeReceived enum to the start of the packet so that we know what kind of packet has just arrived on Server side
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.instance.myId);
            packet.Write(Client.instance.username);

            SendTCPData(packet);
        }
    }

    public static void SendChatMessage(string _msg)
    {
        using (Packet packet = new Packet((int)ClientPackets.chatMessageToServer))
        {
            packet.Write(Client.instance.username);
            packet.Write(_msg);

            SendTCPData(packet);
        }
    }

    public static void SendJSONBoardForHost(string _jsonBoard)
    {
        using (Packet packet = new Packet((int)ClientPackets.jsonBoardToServerForHost))
        {
            packet.Write(_jsonBoard);

            SendTCPData(packet);
        }
    }

    #endregion
}
