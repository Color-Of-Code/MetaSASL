#pragma warning disable CA1852

using Sasl;
using Sasl.Configuration;

var configPath = Environment.GetEnvironmentVariable("SASL_CONFIGURATION_DIR")
                 ?? "/etc/metasasl";

var socketPath = Environment.GetEnvironmentVariable("SASL_SOCKET_FILE")
                 ?? "/var/run/metasasl/mux";

var configuration   = ServerConfiguration.Load(configPath);
var requestHandler  = new AuthenticationHandler(configuration);
var listeningSocket = new ListeningSocket(socketPath);

listeningSocket.Listen(requestHandler);
