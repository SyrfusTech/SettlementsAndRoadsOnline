using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

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

        public Client(int _id)
        {
            id = _id;
            tcp = new TCP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
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

                // Get the NetworkStream from the socket and initialize the dataBuffer
                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                // Begin listening for data from the client, and if data arrives call the ReceiveCallback method
                // Data is read out into the receiveBuffer
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
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
                        //TODO: disconnect
                        return;
                    }

                    // Copy the data into a local variable to handle what to do with the data
                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));
                    // Finally re-open the NetworkStream to receive the next data
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error receiving TCP data: {e}");
                    //TODO: disconnect
                }
            }

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
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            Server.packetHandlers[packetId](id, packet);
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
        }
    }
}
