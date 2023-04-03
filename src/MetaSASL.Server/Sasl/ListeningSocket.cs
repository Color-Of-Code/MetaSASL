using System.Net.Sockets;
using Sasl.Abstractions;

namespace Sasl;

public class ListeningSocket
{
    private readonly string _path;


    public ListeningSocket(string path)
    {
        _path = path;
    }

    public void Listen(IRequestHandler handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var socket = PrepareSocket();

        socket.Listen(16);
        Console.WriteLine("Awaiting clients...");
        while (true) // TODO: dynamic async dispatch
        {
            var peerSocket = new AcceptedSocket(socket.Accept());
            handler.HandleRequest(peerSocket);
        }
    }

    private Socket PrepareSocket()
    {
        if (System.IO.File.Exists(_path))
            System.IO.File.Delete(_path);

        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Bind(new UnixDomainSocketEndPoint(_path));

        // set chmod 777 to allow all processes to communicate
        var process = new System.Diagnostics.Process()
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"777 \"{_path}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return socket;
    }
}
