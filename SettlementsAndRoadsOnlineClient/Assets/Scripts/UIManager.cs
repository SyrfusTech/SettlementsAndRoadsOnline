﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Again singleton, probably don't wanna have multiple input managers
    public static UIManager instance;

    // Some basic UI elements
    public GameObject connectMenu;
    public GameObject chatMenu;
    public InputField usernameField;
    public InputField ipAddressField;
    public InputField chatInputField;
    public Text[] chatSlots = new Text[11];

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
        connectMenu.SetActive(false);
        usernameField.interactable = false;
        ipAddressField.interactable = false;
        chatMenu.SetActive(true);
        for (int i = 0; i < 11; i++)
        {
            chatSlots[i] = GameObject.Find(i.ToString()).GetComponent<Text>();
        }

        Client.instance.username = usernameField.text;
        Client.instance.ip = ipAddressField.text;
        // Tell the Client.instance to Connect to the server
        Client.instance.ConnectToServer();
    }

    public void SendChatMessage()
    {
        ClientSend.SendChatMessage(chatInputField.text);
        chatInputField.text = "";
    }
}
