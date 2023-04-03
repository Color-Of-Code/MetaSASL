using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;

using Sasl.Abstractions;

namespace Sasl.Services.Ldap;

public class LdapAuthentication : IAuthenticationService
{
    private readonly LdapConfiguration _configuration;
    private readonly IRealmSecret _secret;

    public LdapAuthentication(IRealmConfiguration settings, IRealmSecret secret)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));
        _configuration = settings as LdapConfiguration;
        if (_configuration == null)
            throw new ArgumentException("configuration was not a LdapConfiguration");
        _secret = secret;
    }

    public bool Authenticate(string login, string password, string service, string realm)
    {
        var uri = new Uri(_configuration.Server);
        bool isLDAPS = uri.Scheme == "ldaps";

        var server = new LdapDirectoryIdentifier(uri.Host, uri.Port);
        using var connection = new LdapConnection(server)
        {
            AuthType = AuthType.Basic,
            Timeout = new TimeSpan(0, 0, _configuration.Timeout ?? 5)
        };

        var options = connection.SessionOptions;
        options.ProtocolVersion = 3; // LDAPv3
        options.ReferralChasing = _configuration.Deref ?? ReferralChasingOptions.None;
        options.SecureSocketLayer = isLDAPS;

        if (_secret != null)
        {
            // bind with bindDn and lookup user
            var credentials = new NetworkCredential()
            {
                UserName = _secret.Login,
                Password = _secret.Password
            };
            connection.Bind(credentials);
        }
        else
        {
            // anonymous bind
            connection.Bind();
        }

        var filter = _configuration.Filter.Replace("%U", login);
        var request = new SearchRequest(
            _configuration.SearchBase,
            filter,
            _configuration.Scope ?? SearchScope.Subtree);
        var response = connection.SendRequest(request) as SearchResponse;

        // the user must be THE one and only with that login
        if (response == null || response.Entries.Count != 1) return false;
        var user = response.Entries[0];

        // bind with user credentials (authenticate)
        var userCredentials = new NetworkCredential()
        {
            UserName = user.DistinguishedName,
            Password = password
        };
        connection.Bind(userCredentials);
        return true;
    }
}
