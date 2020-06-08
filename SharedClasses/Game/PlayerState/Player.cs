using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    public class Player
    {
        public int clientId;
        public string username;

        public int currentGameHostId;
        public bool readyStatus;
        public int playerNumber;

        public Player(int _clientId, string _username, int _currentGameHostId = -1, bool _readyStatus = false, int _playerNumber = -1)
        {
            clientId = _clientId;
            username = _username;

            currentGameHostId = _currentGameHostId;
            readyStatus = _readyStatus;
            playerNumber = _playerNumber;
        }
    }
}
