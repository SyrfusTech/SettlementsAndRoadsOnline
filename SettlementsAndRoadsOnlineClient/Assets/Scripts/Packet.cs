using System;
using System.Collections.Generic;
using System.Text;

namespace SettlementsAndRoadsOnlineServer.src
{
    // A label for something sent from server to client
    public enum ServerPackets
    {
        welcome = 1
    }

    // A label for something sent from client to server
    public enum ClientPackets
    {
        welcomeReceived = 1
    }

    public class Packet : IDisposable
    {
        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;

        // Create an emopty packet with no id at the front of the buffer
        public Packet()
        {
            buffer = new List<byte>(); // Intitialize buffer
            readPos = 0; // Set readPos to 0
        }

        // Create an empty packe with an id at the front of the buffer
        public Packet(int _id)
        {
            buffer = new List<byte>(); // Intitialize buffer
            readPos = 0; // Set readPos to 0

            Write(_id); // Write packet id to the buffer
        }

        // Create a packet and populate it's buffer with the passed in byte[]
        public Packet(byte[] _data)
        {
            buffer = new List<byte>(); // Intitialize buffer
            readPos = 0; // Set readPos to 0

            SetBytes(_data);
        }

        #region Functions

        // Write the byte[] to the buffer and then set the readableBuffer
        public void SetBytes(byte[] _data)
        {
            Write(_data);
            readableBuffer = buffer.ToArray();
        }

        // Write the size of the data in bytes as the first 4 (sizeof(int)) bytes in the buffer
        public void WriteLength()
        {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // Insert the byte length of the packet at the very beginning
        }

        // Write any arbitrary int to the first 4 bytes of the buffer
        public void InsertInt(int _value)
        {
            buffer.InsertRange(0, BitConverter.GetBytes(_value)); // Insert the int at the start of the buffer
        }

        // Returns the packet as a readable byte[]
        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        // Returns the number of bytes in the buffer
        public int Length()
        {
            return buffer.Count; // Return the length of buffer
        }

        // Returns the number of bytes that have yet to be read
        public int UnreadLength()
        {
            return Length() - readPos; // Return the remaining length (unread)
        }

        // Based on boolean passed, either reset the buffer to allow for Packet object
        // to be reused, or back-up the readPos by 1 int
        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                buffer.Clear(); // Clear buffer
                readableBuffer = null;
                readPos = 0; // Reset readPos
            }
            else
            {
                readPos -= 4; // "Unread" the last read int
            }
        }
        #endregion

        #region Write Data

        // Write a single byte to the tail of the buffer
        public void Write(byte _value)
        {
            buffer.Add(_value);
        }

        // Write an array of bytes to the tail of the buffer
        public void Write(byte[] _value)
        {
            buffer.AddRange(_value);
        }

        // Write a short (2 Bytes) to the tail of the buffer
        public void Write(short _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }

        // Write an int (4 Bytes) to the tail of the buffer
        public void Write(int _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }

        // Write a long (8 Bytes) to the tail of the buffer
        public void Write(long _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }

        // Write a float (4 Bytes) to the tail of the buffer
        public void Write(float _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }

        // Write a bool (1 Byte) to the tail of the buffer
        public void Write(bool _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }

        // Write a string (variable bytes) to the tail of the buffer
        public void Write(string _value)
        {
            Write(_value.Length); // Add the length of the string to the packet
            buffer.AddRange(Encoding.ASCII.GetBytes(_value)); // Add the string itself
        }
        #endregion

        #region Read Data

        // Read a single byte of data
        public byte ReadByte(bool _moveReadPos = true)
        {
            // Make sure that we still have at least one byte left to read
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                byte value = readableBuffer[readPos]; // Get the byte at readPos' position
                if (_moveReadPos)
                {
                    readPos += 1; // Increase readPos by 1
                }
                return value; // Return the byte
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        // Reads _length number of bytes
        public byte[] ReadBytes(int _length, bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                byte[] value = buffer.GetRange(readPos, _length).ToArray(); // Get the bytes at readPos' position with a range of _length
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += _length; // Increase readPos by _length
                }
                return value; // Return the bytes
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        // Reads one short (2 Bytes)
        public short ReadShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                short value = BitConverter.ToInt16(readableBuffer, readPos); // Convert the bytes to a short
                if (_moveReadPos)
                {
                    // If _moveReadPos is true and there are unread bytes
                    readPos += 2; // Increase readPos by 2
                }
                return value; // Return the short
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        // Reads one int (4 Bytes)
        public int ReadInt(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                int value = BitConverter.ToInt32(readableBuffer, readPos); // Convert the bytes to an int
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 4; // Increase readPos by 4
                }
                return value; // Return the int
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        // Reads one long (8 Bytes)
        public long ReadLong(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                long value = BitConverter.ToInt64(readableBuffer, readPos); // Convert the bytes to a long
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 8; // Increase readPos by 8
                }
                return value; // Return the long
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        // Reads on float (4 Bytes)
        public float ReadFloat(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                float value = BitConverter.ToSingle(readableBuffer, readPos); // Convert the bytes to a float
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 4; // Increase readPos by 4
                }
                return value; // Return the float
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        // Reads one bool (1 Byte)
        public bool ReadBool(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                bool value = BitConverter.ToBoolean(readableBuffer, readPos); // Convert the bytes to a bool
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 1; // Increase readPos by 1
                }
                return value; // Return the bool
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        // Reads a string from the packet
        public string ReadString(bool _moveReadPos = true)
        {
            try
            {
                int length = ReadInt(); // Get the length of the string
                string value = Encoding.ASCII.GetString(readableBuffer, readPos, length); // Convert the bytes to a string
                if (_moveReadPos && value.Length > 0)
                {
                    // If _moveReadPos is true string is not empty
                    readPos += length; // Increase readPos by the length of the string
                }
                return value; // Return the string
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }
        #endregion

        // Code for the IDisposable interface
        private bool disposed = false;

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
