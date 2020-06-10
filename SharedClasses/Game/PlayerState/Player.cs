using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class Player
    {
        public int clientId { get; set; }
        public string username { get; set; }

        public int currentGameHostId { get; set; }
        public bool readyStatus { get; set; }
        public int playerNumber { get; set; }

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
