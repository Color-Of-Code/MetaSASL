namespace Sasl.Abstractions;

// Abstracts the methods we need on a socket for our purpose,
// hiding all the other concerns. For the purpose of this authentication
// we only care about sending and receiving strings over the socket.
public interface ISocket
{
    string ReadString();
    void SendString(string response);
}
