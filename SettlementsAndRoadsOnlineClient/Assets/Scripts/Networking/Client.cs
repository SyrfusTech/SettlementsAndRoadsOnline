using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using SharedClasses;
using System.Linq.Expressions;

public class Client : MonoBehaviour
{
    // Singleton cause any one player should never have more than one Client associated to them
    public static Client instance;
    // The same buffer size of 4 MB as the Server
    public static int dataBufferSize = 4096;

    // The ip address to connect to.  Defaulted to the ip address for connecting to oneself (same machine)
    public string ip = "127.0.0.1";
    // The client's username.
    public string username = "Player";
    // The same port number as the Server
    public int port = 25570;
    // A personal ID (Not sure what it's going to be for yet, but atm it does literally nothing
    public int myId = 0;
    // TCP class which, same as the Server, holds the data required for the tcp communication
    public TCP tcp;
    // UDP class which, same as the Server, holds the data required for the udp communication
    public UDP udp;

    // Keep track of whether or not we are connected to the server
    private bool isConnected = false;

    // In this case the packetHandler delegate does not need an id because this client holds it's own id and only ever sends to the server not the other clients
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

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
        udp = new UDP();
    }

    // Handling the case where the editor is one of the clients
    private void OnApplicationQuit()
    {
        Disconnect();
    }

    // Initialize all the client data and call the TCP.Connect() function
    public void ConnectToServer()
    {
        InitializeClientData();

        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        // tcp socket
        public TcpClient socket;

        // NetworkStream and data buffer for the tcp IO channel
        private NetworkStream stream;
        // Packet for unpacking the receiveBuffer
        private Packet receivedData;
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

            receivedData = new Packet();

            // Wait for some data to be sent over the stream and if so store it in the receiveBuffer and
            // call the ReceiveCallback method
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data to server via TCP: {e}");
            }
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
                    // The data is non-existant so failure so let's disconnect this instance
                    instance.Disconnect();
                    return;
                }

                // Copy the data to a local buffer and handle the data locally
                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                // Handle the receivedData
                receivedData.Reset(HandleData(data));

                // Start reading from the Network stream again
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                // Something occurred at some point to cause an exception so we disconnect
                Disconnect();
            }
        }

        // Identical to the Server's Handle Data function
        private bool HandleData(byte[] _data)
        {
            int packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        // Close out all the TCP data and then close the sockets
        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        // Udp socket and stored endpoint of the server
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        // The Connect function takes in a localPort different to the server port
        // The passed in local port is determined from the TCP socket's LocalEndPoint
        public void Connect(int _localPort)
        {
            // Setup and connect the udp socket
            socket = new UdpClient(_localPort);
            socket.Connect(endPoint);

            // Listen for incoming data packets
            socket.BeginReceive(ReceiveCallback, null);

            // Send an empty packet for udp initialization
            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet)
        {
            try
            {
                // Client ID inserted at the start of the packet to tell server
                // which client sent the info because server side the udp socket
                // doesn't inherently know this information
                _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data to server via UDP: {e}");
            }
        }

        // When we receive data packets
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                // store the data
                byte[] data = socket.EndReceive(_result, ref endPoint);
                // start listening for data again
                socket.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            // Extract the data (minus the length)
            using (Packet packet = new Packet(_data))
            {
                int packetLength = packet.ReadInt();
                _data = packet.ReadBytes(packetLength);
            }

            // Call the packet handler associated to the sent packet
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(_data))
                {
                    int packetId = packet.ReadInt();
                    packetHandlers[packetId](packet);
                }
            });
        }

        // Null the UDP data
        private void Disconnect()
        {
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

    private void InitializeClientData()
    {
        // Exhaustive list of packet handlers that can be received
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.chatMessageToClients, ClientHandle.ReceiveChatMessage },
            { (int)ServerPackets.gameHostedSuccessfully, ClientHandle.ReceiveHostSuccess }
        };
        Debug.Log("Initialized packets.");
    }

    // Instance Disconnect which closes out the sockets
    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from server.");
        }
    }
}
