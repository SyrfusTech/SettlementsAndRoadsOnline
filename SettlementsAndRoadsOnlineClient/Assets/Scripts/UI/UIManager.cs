using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using SharedClasses;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    // Again singleton, probably don't wanna have multiple input managers
    public static UIManager instance;

    public Transform uiCanvas;

    // Connection Game Objects
    public GameObject connectPanel;
    public InputField usernameInputField;
    public InputField ipAddressInputField;

    // Main Menu Game Objects
    public GameObject mainMenuPanel;

    // Host Game Game Objects
    public GameObject selectBoardPanel;
    public Transform selectBoardScrollViewContent;
    public Button boardsButtonPrefab;
    private string fileToLoad = null;

    // Lobby Game Objects
    public GameObject lobbyPanel;
    public Dropdown numPlayersDropdown;
    public Dropdown victoryPointsForWinDropdown;
    public Dropdown setupPhaseDropdown;
    public Text[] usernameTexts;
    public Text[] readyStatusTexts;

    // Manage Boards Game Objects
    public GameObject manageBoardsPanel;
    public Transform boardsScrollViewContent;

    public GameObject confirmationPanel;
    public Text deleteConfirmationText;

    // Boards Editor Game Objects
    public GameObject boardsEditorPanel;
    public GameObject boardsEditorParent;
    public HexagonGridEditManager hexagonGridEditManager;
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

    public void Join()
    {

    }

    public void OpenPlayerLobby()
    {
        selectBoardPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }
    #endregion

    #region HostGame
    public void HostGame()
    {
        mainMenuPanel.SetActive(false);
        selectBoardPanel.SetActive(true);

        LoadBoardList(selectBoardScrollViewContent);
    }

    public void Host()
    {
        // Send board and client information to Server requesting a new game be created under client's username.
        if (fileToLoad != null)
        {
            SendSelectedBoard();
        }
    }

    public void SendSelectedBoard()
    {
        string fileName = fileToLoad;
        if (fileName != null)
        {
            string jsonBoard = SaveManager.LoadDataAsJSON(fileName);
            // Send the string to the Server
            Debug.Log(jsonBoard);
            // TEMPORARY CODE (This should be called when the server confirms the game has been created)
            OpenHostLobby();
        }
    }

    public void OpenHostLobby()
    {
        selectBoardPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        numPlayersDropdown.interactable = true;
        victoryPointsForWinDropdown.interactable = true;
        setupPhaseDropdown.interactable = true;
    }
    #endregion

    #region Lobby

    public void NumPlayersDropdownChanged()
    {
        // TODO: Check to see if current client is host and send the update to all other players
        usernameTexts[0].text = numPlayersDropdown.value.ToString();
    }

    public void VictoryPointsForWinDropdownChanged()
    {
        // TODO: Check to see if current client is host and send the update to all other players
        readyStatusTexts[0].text = victoryPointsForWinDropdown.value.ToString();
    }

    public void SetupPhaseDropdownChanged()
    {
        // TODO: Check to see if current client is host and send the update to all other players
        Debug.Log(setupPhaseDropdown.value);
    }


    public void ReadyUp()
    {
        // If client readies up then send packet of ready data to Server (distributed to all other players).
    }

    public void StartGame()
    {

    }

    public void CloseGame()
    {
        // TODO: Check if Host, if so inform all other players of the game closing
        //       Else inform hostedGame that player is leaving.
        ReturnToMainMenu();
    }

    #endregion

    #region ManageBoards
    public void GetTextFromButton(string _name)
    {
        fileToLoad = _name;
    }

    public void ManageBoards()
    {
        // Open Manage Boards Panel
        mainMenuPanel.SetActive(false);
        manageBoardsPanel.SetActive(true);

        LoadBoardList(boardsScrollViewContent);
    }

    public void LoadBoardList(Transform _scrollViewContent)
    {
        for (int i = 0; i < _scrollViewContent.childCount; i++)
        {
            Destroy(_scrollViewContent.GetChild(i).gameObject);
        }

        // Populate the UI with the list of boards in the directory specified for saving boards.
        string[] files = SaveManager.GetBoardNames();
        foreach (string file in files)
        {
            Button boardsButton = GameObject.Instantiate(boardsButtonPrefab);
            boardsButton.transform.SetParent(_scrollViewContent, false);
            boardsButton.onClick.AddListener(delegate { GetTextFromButton(Path.GetFileNameWithoutExtension(file)); });
            boardsButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = Path.GetFileNameWithoutExtension(file);
        }
    }

    public void Manage()
    {
        // Unload the BoardSelectionPanel's scene and load in the ManageBoards' scene and load in the JSON tilemap.
        if (fileToLoad != null)
        {
            manageBoardsPanel.SetActive(false);
            boardsEditorPanel.SetActive(true);
            boardsEditorParent.SetActive(true);
            boardNameInputField.text = fileToLoad;
            LoadBoard();
        }
    }

    public void CreateNewBoard()
    {
        manageBoardsPanel.SetActive(false);
        boardsEditorPanel.SetActive(true);
        boardsEditorParent.SetActive(true);
        boardNameInputField.text = "";
        hexagonGridEditManager.ClearAllTiles();
        hexagonGridEditManager.hexTiles = new List<HexTile>();
    }

    public void DeleteBoard()
    {
        confirmationPanel.SetActive(true);
        deleteConfirmationText.text = "Delete\n" + fileToLoad + "?";
    }

    public void DeleteConfirmed()
    {
        SaveManager.DeleteBoard(fileToLoad);
        fileToLoad = null;
        LoadBoardList(boardsScrollViewContent);
        confirmationPanel.SetActive(false);
    }

    public void DeleteCanceled()
    {
        confirmationPanel.SetActive(false);
    }

    public void HexTileSelection(int _type)
    {
        hexagonGridEditManager.SetHexPlacementType(_type);
    }

    public void DiceNumberTileSelection(int _type)
    {
        hexagonGridEditManager.SetDiceNumberPlacementType(_type);
    }

    public void RobberSelection()
    {
        hexagonGridEditManager.SetRobberPlacement();
    }

    public void EraseSelection()
    {
        hexagonGridEditManager.SetErasePlacement();
    }

    public void SaveBoard()
    {
        string fileName = boardNameInputField.text;
        // Checks if name of board is not empty.
        if (!fileName.Equals(""))
        {
            SaveManager.SaveData(hexagonGridEditManager.hexTiles, fileName);
        }
    }

    public void LoadBoard()
    {
        string fileName = boardNameInputField.text;
        if (!fileName.Equals(""))
        {
            List<HexTile> hexTiles = SaveManager.LoadData(fileName);
            hexagonGridEditManager.ClearAllTiles();
            hexagonGridEditManager.LoadBoard(hexTiles);
        }
    }
    #endregion

    public void ReturnToMainMenu()
    {
        boardsEditorParent.SetActive(false);
        for (int i = 0; i < uiCanvas.childCount; i++)
        {
            uiCanvas.GetChild(i).gameObject.SetActive(false);
        }

        mainMenuPanel.SetActive(true);
    }
}
