using Sasl.Abstractions;
using Sasl.Configuration;

namespace Sasl;

public class AuthenticationHandler : IRequestHandler
{
    private readonly ServerConfiguration _configuration;

    public AuthenticationHandler(ServerConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void HandleRequest(ISocket socket)
    {
        if (socket == null)
            throw new ArgumentNullException(nameof(socket));
        string account = "<unknown>";
        try
        {
            string login    = socket.ReadString();
            string password = socket.ReadString();
            string service  = socket.ReadString();
            string realm    = socket.ReadString();

            account = $"{login}@{realm}";
            string response = Authenticate(login, password, service, realm)
                ? "OK"
                : "NO";
            socket.SendString(response);
        }
        catch(Exception ex)
        {
            socket.SendString($"NO {ex.Message}");
        }
    }

    private bool Authenticate(string login, string password, string serviceName, string realm)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentNullException(nameof(login));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));

        if (serviceName != "ldap")
            throw new NotImplementedException($"Service {serviceName} is unsupported");

        if (!_configuration.Realms.ContainsKey(realm))
            throw new ArgumentException($"Realm {realm} is not configured");

        var service = _configuration.Realms[realm];
        return service.Authenticate(login, password, serviceName, realm);
    }
}
