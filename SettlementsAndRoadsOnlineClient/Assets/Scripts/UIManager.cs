using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Again singleton, probably don't wanna have multiple input managers
    public static UIManager instance;

    // Some basic UI elements
    public GameObject startMenu;
    public InputField usernameField;

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

    // Called by clicking the Connect button
    public void ConnectToServer()
    {
        // Hide the connection UI
        startMenu.SetActive(false);
        usernameField.interactable = false;
        // Tell the Client.instance to Connect to the server
        Client.instance.ConnectToServer();
    }
}
