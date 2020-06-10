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
}
