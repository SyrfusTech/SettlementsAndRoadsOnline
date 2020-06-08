using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedClasses;

public static class ClientState
{
    public static Player clientPlayer;

    public static void Initialize(int _clientId, string _username)
    {
        clientPlayer = new Player(_clientId, _username);
    }

    public static void SetPlayerNumber(int _playerNumber)
    {
        clientPlayer.playerNumber = _playerNumber;
    }

    public static void JoinHostedGame(int _hostId)
    {
        clientPlayer.currentGameHostId = _hostId;
    }

    public static void ReadyUp()
    {
        clientPlayer.readyStatus = true;
    }
}
