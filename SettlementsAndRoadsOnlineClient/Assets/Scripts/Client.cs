﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    // Singleton cause any one player should never have more than one Client associated to them
    public static Client instance;
    // The same buffer size of 4 MB as the Server
    public static int dataBufferSize = 4096;

    // The ip address to connect to.  In this case hard coded to the ip address for connecting to oneself (same machine)
    public string ip = "127.0.0.1";
    // The same port number as the Server
    public int port = 25570;
    // A personal ID (Not sure what it's going to be for yet, but atm it does literally nothing
    public int myId = 0;
    // TCP class which, same as the Server, holds the data required for the tcp communication
    public TCP tcp;

    private void Awake()
    {
        // Ensure that any one player can never instantiate more than one Client
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

    private void Start()
    {
        tcp = new TCP();
    }

    // Just an access point for the TCP's Connect() method
    public void ConnectToServer()
    {
        tcp.Connect();
    }

    public class TCP
    {
        // tcp socket
        public TcpClient socket;

        // NetworkStream and data buffer for the tcp IO channel
        private NetworkStream stream;
        private byte[] receiveBuffer;

        public void Connect()
        {
            // Initialize the tcp socket with it's correct send and receive buffer sizes
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            // Initialize the receive buffer with the correct buffer size
            receiveBuffer = new byte[dataBufferSize];

            // Begin attempting to connect to the ip:port using the newly initialized socket and call ConnectCallback when successful
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            // Stop the socket from trying to connect anymore
            socket.EndConnect(_result);
             
            // If the connection fails, bail out (probably in the future have some kinda logic
            // For what to do when connection fails
            if (!socket.Connected)
            {
                return;
            }

            // Get the NetworkStream from the new connection
            stream = socket.GetStream();
            
            // Wait for some data to be sent over the stream and if so store it in the receiveBuffer and
            // call the ReceiveCallback method
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                // Stop the input stream, storing the size of the data received
                int byteLength = stream.EndRead(_result);
                // If the data is non-existant failure has occurred
                if (byteLength <= 0)
                {
                    //TODO: disconnect
                    return;
                }

                // Copy the data to a local buffer and handle the data locally
                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                //TODO: handle data
                // Start reading from the Network stream again
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                //TODO: disconnect
            }
        }
    }
}