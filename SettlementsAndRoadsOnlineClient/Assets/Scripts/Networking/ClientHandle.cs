using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using SharedClasses;
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
        ClientState.Initialize(Client.instance.myId, Client.instance.username);
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void ReceiveChatMessage(Packet _packet)
    {
        string msg = _packet.ReadString();
    }

    public static void ReceiveHostSuccess(Packet _packet)
    {
        string jsonLobbyInfo = _packet.ReadString();
        string jsonPlayer = _packet.ReadString();

        Debug.Log(jsonLobbyInfo);
        Debug.Log(jsonPlayer);

        GameLobbyInfo lobbyInfo = JsonUtility.FromJson<GameLobbyInfo>(jsonLobbyInfo);
        Player player = JsonUtility.FromJson<Player>(jsonPlayer);

        Debug.Log(lobbyInfo.maxPlayers);
        Debug.Log(player.username);

        ClientState.clientPlayer = player;

        UIManager.instance.OpenHostLobby(lobbyInfo);
    }
}
