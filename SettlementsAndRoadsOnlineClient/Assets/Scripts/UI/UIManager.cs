using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using SharedClasses;
using UnityEngine.UI;
using System.IO;

public class UIManager : MonoBehaviour
{
    // Again singleton, probably don't wanna have multiple input managers
    public static UIManager instance;

    // Connection Game Objects
    public GameObject connectPanel;
    public InputField usernameInputField;
    public InputField ipAddressInputField;

    // Main Menu Game Objects
    public GameObject mainMenuPanel;

    // Manage Boards Game Objects
    public GameObject manageBoardsPanel;
    public HexagonGridManager hexagonGridManager;
    public InputField boardNameInputField;

    private void Awake()
    {
        // Singleton stuff
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void ConnectToServer()
    {
        // Hide the connection UI
        connectPanel.SetActive(false);
        usernameInputField.interactable = false;
        ipAddressInputField.interactable = false;

        Client.instance.username = usernameInputField.text;
        Client.instance.ip = ipAddressInputField.text;
        // Tell the Client.instance to Connect to the server
        Client.instance.ConnectToServer();

        mainMenuPanel.SetActive(true);
    }

    #region JoinGame
    public void JoinGame()
    {
        // Get list of available games from the Server.
        // Add that list to a dropdown list.
        // Join button that sends client information to server
        // and requests to be added to the list of players for the selected game.
        // Opens up "PlayerLobbyPanel" and populates the fields with data retrieved from the server.
        // If client receives a packet of lobby data, update the client's GUI.
    }

    public void ReadyUp()
    {
        // If client readies up then send packet of ready data to Server (distributed to all other players).
    }
    #endregion

    #region HostGame
    public void HostGame()
    {
        // Send board and client information to Server requesting a new game be created under client's username.
        // Opens up "HostLobbyPanel" and populates the fields with data retrieved from the server.
        // If host receives a packet of lobby data, update the host's GUI (from a player joining or readying up).
        // If host updates any fields, send packet of lobby data to Server (distributed to all other players).
    }

    public void StartGame()
    {
        // If all connected players are ready, then send StartGame and LobbyData packets to the Server.
        // Server side the LobbyData and StartGame packets will be distributed to each player in the game
        // (including the host) and the Handle for StartGame will unload the MainMenu's scene and load in
        // the Game scene populated with the LobbyData being used as settings.
        // From there the Game scene's GameLoop will handle the rest.
    }
    #endregion

    #region ManageBoards
    public void ManageBoards()
    {
        // Open Manage Boards Panel
        mainMenuPanel.SetActive(false);
        manageBoardsPanel.SetActive(true);
        // Populate the UI with the list of boards in the directory specified for saving boards.

        // Select a board (or the create new board option) from the dropdown list and click the ManageButton.
    }

    public void Manage()
    {
        // Unload the MainMenu's scene and load in the ManageBoards' scene and load in the JSON tilemap.
    }

    public void HexTileSelection(int _type)
    {
        hexagonGridManager.SetHexPlacementType(_type);
    }

    public void DiceNumberTileSelection(int _type)
    {
        hexagonGridManager.SetDiceNumberPlacementType(_type);
    }

    public void RobberSelection()
    {
        hexagonGridManager.SetRobberPlacement();
    }

    public void EraseSelection()
    {
        hexagonGridManager.SetErasePlacement();
    }

    public void SaveBoard()
    {
        string fileName = boardNameInputField.text;
        // Checks if name of board is not empty.
        if (!fileName.Equals(""))
        {
            SaveManager.SaveData(hexagonGridManager.hexTiles, fileName);
        }
    }

    public void LoadBoard()
    {
        string fileName = boardNameInputField.text;
        if (!fileName.Equals(""))
        {
            List<HexTile> hexTiles = SaveManager.LoadData(fileName);
            hexagonGridManager.ClearAllTiles();
            hexagonGridManager.LoadBoard(hexTiles);
        }
    }

    public void ReturnToMainMenu()
    {
        // Unload the ManageBoards' scene and load the MainMenu's scene.
    }
    #endregion
}
