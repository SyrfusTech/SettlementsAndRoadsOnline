using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO.Pipes;
using SharedClasses;

namespace SettlementsAndRoadsOnlineServer.src
{
    class Client
    {
        // A client can send 4 mb of data per buffer
        public static int dataBufferSize = 4096;

        // id is just assigned when the Client object is initialized, doesn't have anything to do
        // with the actual connected client other than their position in the dictionary
        public int id;
        // TCP is a class holding the socket, a NetworkStream which will function as the IO stream
        // for this individual client, and a dataBuffer to store the IO
        public TCP tcp;
        // UDP similarily to TCP holds all the data required for the Client to be communicated
        // with via UdpClient sockets
        public UDP udp;

        public Client(int _id)
        {
            id = _id;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            // Packet to unpack the receiveBuffer into
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            // Setup all the Tcp socket data and then wait for data to be sent along the NetworkStream
            public void Connect(TcpClient _socket)
            {
                // Connect this Client's tcp socket and then set the receive and send buffer sizes
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                // Get the NetworkStream from the socket and initialize the dataBuffer/packet
                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                // Begin listening for data from the client, and if data arrives call the ReceiveCallback method
                // Data is read out into the receiveBuffer
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                // Send the welcome packet to the newly connected client
                ServerSend.Welcome(id, "Welcome to the server!");
            }

            // Call this function to send the passed in packet to this client over tcp socket
            public void SendData(Packet _packet)
            {
                try
                {
                    // Only send the data if the socket is not empty
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {e}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    // Stop the stream from sending in more data and store the number of bytes of data
                    // that are arriving
                    int byteLength = stream.EndRead(_result);
                    // A data arrived but there is no data so something went wrong
                    if (byteLength <= 0)
                    {
                        // Disconnect the current client's sockets
                        Server.clients[id].Disconnect();
                        return;
                    }

                    // Copy the data into a local variable to handle what to do with the data
                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    // Handle the data (ie unpack it and then do something with it)
                    receivedData.Reset(HandleData(data));
                    // Finally re-open the NetworkStream to receive the next data
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error receiving TCP data: {e}");
                    // Any kind of exception makes this client disconnect
                    Server.clients[id].Disconnect();
                }
            }

            private bool HandleData(byte[] _data)
            {
                // First populate the packet with the received data in byte format
                int packetLength = 0;
                receivedData.SetBytes(_data);
                // Then make sure that there is at least an int to be read as all packets must start with an int dictating their size
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    // Check to see if the packet sent was empty after the length
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                // Continue looping so long as the length is non-zero and the remaining data left to be read in the current packet
                // is larger than or equal to the packet length
                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    // Read the packet data into a byte array
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    // Do something with the data on the main thread
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        // That something is by calling the packet handler corresponding the the enum-id at the start of the packet
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            Server.packetHandlers[packetId](id, packet);
                        }
                    });

                    // Reset the packet length and check to see if there is another packet that was sent over the network, if so read in the packet length
                    // and restart the while loop
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

            // TCP disconnect closing the socket and the TCP data
            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            // IPEndPoint storing the endpoint for this specific client
            public IPEndPoint endPoint;
            // The id of the client
            private int id;

            public UDP(int _id)
            {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            // Data handling is generally more simple for UDP because packets are sent one at a time
            // so there's no possibility for the packet to be split between messages
            public void HandleData(Packet _packet)
            {
                // Read the packet length and then read the packet data
                int packetLength = _packet.ReadInt();
                byte[] packetBytes = _packet.ReadBytes(packetLength);

                // Call the packet handler based on the packet id at the start of
                // the packet data
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](id, packet);
                    }
                });
            }

            // nullify the endpoint but don't close the socket cause the
            // udp socket is used for all clients
            public void Disconnect()
            {
                endPoint = null;
            }
        }

        // Disconnect a single client from the server
        private void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}
