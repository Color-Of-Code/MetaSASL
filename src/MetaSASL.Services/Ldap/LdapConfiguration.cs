using System.DirectoryServices.Protocols;

using Sasl.Abstractions;

namespace Sasl.Services.Ldap;

public class LdapConfiguration : IRealmConfiguration
{
    public SearchScope? Scope { get; set; }
    public int? Timeout { get; set; }
    public ReferralChasingOptions? Deref { get; set; }
    public string Filter { get; set; }
    public string Server { get; set; }
    public string SearchBase { get; set; }
};
