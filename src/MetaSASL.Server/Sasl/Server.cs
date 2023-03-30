using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Threading.Tasks;
using Sasl.Abstractions;
using Sasl.Configuration;

namespace Sasl;

public class Server
{
    private readonly ServerConfiguration _configuration;
    private readonly string _path;
    private static readonly int MaxResponseLength = 1024;
    private static readonly int MaxRequestLength = 256;


    public Server(string path, ServerConfiguration configuration)
    {
        _configuration = configuration;
        _path = path;
    }

    public void Run()
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

        socket.Listen(16);
        Console.WriteLine("Awaiting clients...");
        while (true) // TODO: dynamic async dispatch
        {
            HandleRequest(socket.Accept());
        }
    }

    private static int ReadCount(Socket s)
    {
        byte[] dataCount = new byte[2];
        s.Receive(dataCount, 2, System.Net.Sockets.SocketFlags.None);
        int count = BinaryPrimitives.ReadInt16BigEndian(dataCount);
        if (count > MaxRequestLength) {
        	throw new Exception("data too long");
        }
        return count;
    }

    private static void WriteCount(Socket s, int count)
    {
        byte[] dataCount = new byte[2];
        if (count > MaxRequestLength) {
        	throw new Exception("data too long");
        }
        BinaryPrimitives.WriteInt16BigEndian(new Span<byte>(dataCount), (short)count);
        s.Send(dataCount, 2, System.Net.Sockets.SocketFlags.None);
    }

    // Strings are prepended with the length (2 bytes)
    private static string ReadString(Socket s)
    {
        int count = ReadCount(s);
        byte[] buffer = new byte[count];
        s.Receive(buffer, count, System.Net.Sockets.SocketFlags.None);
        return System.Text.Encoding.UTF8.GetString(buffer);
    }

    private void HandleRequest(Socket s)
    {
        string account = "<unknown>";
        try
        {
            string login    = ReadString(s);
            string password = ReadString(s);
            string service  = ReadString(s);
            string realm    = ReadString(s);

            account = $"{login}@{realm}";
            string response = Authenticate(login, password, service, realm);

            SendResponse(s, response, account);
        }
        catch(Exception ex)
        {
            SendResponse(s, $"NO {ex.Message}", account);
        }
    }

    private static void SendResponse(Socket s, string response, string account)
    {
        Console.WriteLine($"{DateTime.UtcNow.ToString("s")} {account}: {response}");
        if (response.Length > MaxResponseLength - 4)
            response = response.Substring(0, MaxResponseLength - 4);

        var dataToSend = System.Text.Encoding.UTF8.GetBytes(response);
        int count = dataToSend.Length;

        WriteCount(s, count);
        s.Send(dataToSend, count, System.Net.Sockets.SocketFlags.None);
    }

    private string Authenticate(string login, string password, string service, string realm)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentNullException(nameof(login));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        if (service != "ldap")
            throw new Exception($"Service {service} is unsupported");

        if (!_configuration.Realms.ContainsKey(realm))
            throw new Exception($"Realm {realm} is not configured");

        var config = _configuration.Realms[realm];
        IAuthenticationMechanism mechanism = new Mechanism.Ldap.LdapAuthentication(config.Settings, config.Credentials);
        var success = mechanism.Authenticate(login, password, service, realm);

        return success ? "OK" : "NO";
    }
}
