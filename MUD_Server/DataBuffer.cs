using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

public class DataBuffer : IDisposable
{
    private List<byte> buffer;
    private byte[] readBuffer;
    private int readPos;

    public DataBuffer()
    {
        buffer = new List<byte>();
        readPos = 0;
    }

    public DataBuffer(int packetId)
    {
        buffer = new List<byte>();
        readPos = 0;
        Write(packetId);
    }

    public DataBuffer(byte[] data)
    {
        buffer = new List<byte>();
        readPos = 0;
        SetBytes(data);
    }

    public void SetBytes(byte[] data)
    {
        Write(data);
        readBuffer = buffer.ToArray();
    }

    public void WriteLength()
    {
        // Insert the byte length of the packet at the very beginning
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
    }

    public void InsertInt(int input)
    {
        // Insert the int at the start of the buffer
        buffer.InsertRange(0, BitConverter.GetBytes(input));
    }

    public void InsertId(Guid input)
    {
        // Insert the id at the start of the buffer
        byte[] bytes = input.ToByteArray();
        buffer.InsertRange(0, bytes);   // convert the guid to byte[]
        InsertInt(bytes.Length);        // get the length of the byte[]
    }

    public int GetReadPos()
    {
        return readPos;
    }

    public byte[] ToArray()
    {
        readBuffer = buffer.ToArray();
        return readBuffer;
    }

    public int Count()
    {
        return buffer.Count;
    }

    public int Length()
    {
        return buffer.Count;
    }

    public int UnreadLength()
    {
        return buffer.Count - readPos;
    }

    public void Clear()
    {
        buffer.Clear(); // Clear the buffer.
        readBuffer = null;
        readPos = 0; // Reset readPos
    }

    public void Reset(bool reset = true)
    {
        if (reset)
        {
            buffer.Clear(); // Clear the buffer.
            readBuffer = null;
            readPos = 0; // Reset readPos
        }
        else
        {
            readPos -= 4;
        }
    }

    #region Write Data

    #region Numbers
    public void Write(byte input)
    {
        buffer.Add(input); // Since this is a byte list we can just enter the input
    }

    public void Write(byte[] input)
    {
        buffer.AddRange(input); // Since we're adding an array of bytes we need to use AddRange
    }

    public void Write(short input)
    {
        buffer.AddRange(BitConverter.GetBytes(input)); // Convert the input with BitConverter and then add the range to the buffer(BitConverter converts the input to an array)
    }

    public void Write(int input)
    {
        buffer.AddRange(BitConverter.GetBytes(input)); // Convert the input with BitConverter and then add the range to the buffer(BitConverter converts the input to an array)
    }

    public void Write(long input)
    {
        buffer.AddRange(BitConverter.GetBytes(input)); // Convert the input with BitConverter and then add the range to the buffer(BitConverter converts the input to an array)
    }

    public void Write(float input)
    {
        buffer.AddRange(BitConverter.GetBytes(input)); // Convert the input with BitConverter and then add the range to the buffer(BitConverter converts the input to an array)
    }
    #endregion Numbers

    public void Write(string input)
    {
        //buffer.AddRange(BitConverter.GetBytes(input.Length)); // Add the string length to the buffer
        Write(input.Length);    // Add the string length to the buffer
        buffer.AddRange(Encoding.ASCII.GetBytes(input)); // Convert the input string to bytes with Encoding.ASCII and then add it to the buffer
    }

    public void Write(Guid input)
    {
        byte[] bytes = input.ToByteArray();
        Write(bytes.Length);
        Write(bytes);
    }

    public void Write(Vector3 input)
    {
        Write(input.X);
        Write(input.Y);
        Write(input.Z);
    }

    public void Write(Quaternion input)
    {
        Write(input.X);
        Write(input.Y);
        Write(input.Z);
        Write(input.W);
    }

    #endregion Write Data
    #region Read Data

    public byte ReadByte(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            byte ret = readBuffer[readPos];
            if (peek)
            {
                //readPos++;
                //readPos += 1;
                readPos += sizeof(byte);
            }
            return ret;
        }
        else
        {
            throw new Exception($"DataBuffer [BYTE] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public byte[] ReadBytes(int length, bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            byte[] ret = buffer.GetRange(readPos, length).ToArray();

            if (peek && buffer.Count > readPos)
            {
                readPos += length;
            }
            return ret;
        }
        else
        {
            throw new Exception($"DataBuffer [BYTE[]] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public short ReadShort(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            short ret = BitConverter.ToInt16(readBuffer, readPos);
            if (peek)
            {
                //readPos += 2;
                readPos += sizeof(short);

            }
            return ret;
        }
        else
        {
            throw new Exception($"DataBuffer [SHORT] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public int ReadInteger(bool peek = true) { return ReadInt(peek); }
    public int ReadInt(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            int ret = BitConverter.ToInt32(readBuffer, readPos);
            if (peek)
            {
                //readPos += 4;
                readPos += sizeof(int);

            }
            return ret;
        }
        else
        {
            throw new Exception($"DataBuffer [INT] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public long ReadLong(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            long ret = BitConverter.ToInt64(readBuffer, readPos);
            if (peek)
            {
                //readPos += 8;
                readPos += sizeof(long);
            }
            return ret;
        }
        else
        {
            throw new Exception($"DataBuffer [LONG] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public float ReadFloat(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            float ret = BitConverter.ToSingle(readBuffer, readPos);

            if (peek)
            {
                //readPos += 4;
                readPos += sizeof(float);
            }
            return ret;
        }
        else
        {
            throw new Exception($"DataBuffer [FLOAT] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public bool ReadBool(bool peek = true)
    {
        if (buffer.Count > readPos)
        {
            bool ret = BitConverter.ToBoolean(readBuffer, readPos);
            if (peek)
            {
                //readPos++;
                //readPos += 1;
                readPos += sizeof(bool);
            }
            return ret;
        }
        else
        {
            throw new Exception($"DataBuffer [BOOL] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public string ReadString(bool peek = true)
    {
        try
        {
            int length = ReadInt(peek);
            string ret = Encoding.ASCII.GetString(readBuffer, readPos, length);
            if (peek && ret.Length > 0)
            {
                readPos += length;
            }
            return ret;
        }
        catch
        {
            throw new Exception($"DataBuffer [STRING] is past it's limit!\nBuffer size is: {buffer.Count}, readPos is: {readPos}");
        }
    }

    public Guid ReadGuid(bool peek = true)
    {
        int length = ReadInt(peek);
        return new Guid(ReadBytes(length, peek));
    }

    public Vector3 ReadVector3(bool peek = true)
    {
        return new Vector3(ReadFloat(peek), ReadFloat(peek), ReadFloat(peek));
    }

    public Quaternion ReadQuaternion(bool peek = true)
    {
        return new Quaternion(ReadFloat(peek), ReadFloat(peek), ReadFloat(peek), ReadFloat(peek));
    }
    #endregion Read Data

    private bool disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if (disposed == false)
        {
            if (disposing)
            {
                buffer.Clear();
                buffer = null;
                readBuffer = null;
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