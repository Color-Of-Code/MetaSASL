namespace Sasl.Abstractions;

public interface IAuthenticationMechanism
{
    bool Authenticate(string login, string password, string service, string realm);
}
