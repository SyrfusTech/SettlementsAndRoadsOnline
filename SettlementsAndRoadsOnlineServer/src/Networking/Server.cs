using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SharedClasses;
using System.Runtime.CompilerServices;

namespace SettlementsAndRoadsOnlineServer.src
{
    class Server
    {
        // Max players, port number, and Dictionary of connected clients
        public static int maxPlayers { get; private set; }
        public static int port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        // Create a delegate and Dictionary for handling which packets have arrived based on a signature
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        // A listener that opens up a socket and waits for a client to ask if they can connect
        private static TcpListener tcpListener;
        // A "listener" (actually a socket itself) for receiving and sending data between the server
        // and all of the other clients
        private static UdpClient udpListener;

        // Initialize data and start listening on a tcp socket for a client to attempt connecting
        public static void Start(int _maxPlayers, int _port)
        {
            // Data initialization
            maxPlayers = _maxPlayers;
            port = _port;

            Console.WriteLine("Starting server");
            InitializeServerData();

            // Initialize the tcp listener with any IPAddress on our port
            tcpListener = new TcpListener(IPAddress.Any, port);
            // Start listening and set the method to be called when a client asks to connect
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            // Open the udp socket on the port and start listening
            udpListener = new UdpClient(port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on port {port}.");
        }

        // This function gets called if a TCP socket connected with a client
        private static void TCPConnectCallback(IAsyncResult result)
        {
            // Store the information of the client who wishes to connect into a TcpClient socket object
            // At the same time stop the listener from accepting new connections
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            // Because we want to be listening for new connections as often as possible, immediately restart
            // listening for tcp connection attempts
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}.");

            // Loop over all possible client slots in the Dictionary of clients
            for (int i = 1; i <= maxPlayers; i++)
            {
                // Adding the client attempting to connect to the first empty slot
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            // As the print statement designates, the only way to reach this point is for all maxPlayers slots
            // in the clients Dictionary to have non-null tcp sockets (ie full)
            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                // Get the client's "endpoint" and the packet sent as a byte[]
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(_result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                // Create a packet out of the data
                using (Packet packet = new Packet(data))
                {
                    // Make sure the clientId is a valid client id
                    int clientId = packet.ReadInt();
                    if (clientId <= 0 || clientId > maxPlayers)
                    {
                        return;
                    }
                    
                    // If the current client's not connected then connect
                    if (clients[clientId].udp.endPoint == null)
                    {
                        clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    // Compare the clientId's endPoint with the clientEndPoint passed in for
                    // validation purposes
                    if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        clients[clientId].udp.HandleData(packet);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error receiving UDP data: {e}");
            }
        }

        // Send the UDPData to the desired endpoint
        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {e}");
            }
        }

        private static void InitializeServerData()
        {
            // Initialize the clients dictionary with blank Clients ready to have their data filled
            for (int i = 1; i <= maxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            // Initialize the packetHandlers Dictionary with the exhaustive list of packets possible to be received
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                // The first int is the enum of the packet received (the identifier), the second is the function with matching parameter list as the delegate to be called
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.chatMessageToServer, ServerHandle.ChatMessageReceived },
            };
            Console.WriteLine("Initialized packets.");
        }
    }
}
