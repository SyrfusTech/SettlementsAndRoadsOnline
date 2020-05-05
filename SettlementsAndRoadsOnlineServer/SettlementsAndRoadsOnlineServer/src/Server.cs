using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace SettlementsAndRoadsOnlineServer.src
{
    class Server
    {
        // Max players, port number, and Dictionary of connected clients
        public static int maxPlayers { get; private set; }
        public static int port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        // A listener that opens up a socket and waits for a client to ask if they can connect
        private static TcpListener tcpListener;

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

        // Initialize the clients dictionary with blank Clients ready to have their data filled
        private static void InitializeServerData()
        {
            for (int i = 1; i <= maxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived }
            };
            Console.WriteLine("Initialized packets.");
        }
    }
}
