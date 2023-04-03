using System.Buffers.Binary;
using System.Net.Sockets;
using Sasl.Abstractions;

namespace Sasl;

public class AcceptedSocket : ISocket
{
    private readonly Socket _socket;
    private static readonly int MaxResponseLength = 1024;
    private static readonly int MaxRequestLength = 256;

    public AcceptedSocket(Socket socket)
    {
        _socket = socket;
    }

    private int ReadCount()
    {
        byte[] dataCount = new byte[2];
        _socket.Receive(dataCount, 2, System.Net.Sockets.SocketFlags.None);
        int count = BinaryPrimitives.ReadInt16BigEndian(dataCount);
        if (count > MaxRequestLength) {
        	throw new InvalidDataException("data too long");
        }
        return count;
    }

    private void WriteCount(int count)
    {
        byte[] dataCount = new byte[2];
        if (count > MaxRequestLength) {
        	throw new InvalidDataException("data too long");
        }
        BinaryPrimitives.WriteInt16BigEndian(new Span<byte>(dataCount), (short)count);
        _socket.Send(dataCount, 2, System.Net.Sockets.SocketFlags.None);
    }

    // Strings are prepended with the length (2 bytes)
    public string ReadString()
    {
        int count = ReadCount();
        byte[] buffer = new byte[count];
        _socket.Receive(buffer, count, System.Net.Sockets.SocketFlags.None);
        return System.Text.Encoding.UTF8.GetString(buffer);
    }

    public void SendString(string response)
    {
        if (String.IsNullOrEmpty(response))
            throw new ArgumentNullException(nameof(response));

        // Console.WriteLine($"{DateTime.UtcNow.ToString("s")} {account}: {response}");
        if (response.Length > MaxResponseLength - 4)
            response = response.Substring(0, MaxResponseLength - 4);

        var dataToSend = System.Text.Encoding.UTF8.GetBytes(response);
        int count = dataToSend.Length;

        WriteCount(count);
        _socket.Send(dataToSend, count, System.Net.Sockets.SocketFlags.None);
    }
}
