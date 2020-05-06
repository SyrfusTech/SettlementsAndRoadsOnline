using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    // Packet handler for received Welcome message
    public static void Welcome(Packet _packet)
    {
        string msg = _packet.ReadString();
        int myId = _packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        Client.instance.myId = myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    // Packet handler for received UDPTest
    public static void UDPTest(Packet _packet)
    {
        string msg = _packet.ReadString();

        Debug.Log($"Received packet via UDP. Contains message: {msg}");
        ClientSend.UDPTestReceived();
    }

    public static void ReceiveChatMessage(Packet _packet)
    {
        string msg = _packet.ReadString();

        UIManager.instance.chatMessage.text = msg;
    }
}
