namespace Sasl.Abstractions;

public interface IAuthenticationService
{
    bool Authenticate(string login, string password, string service, string realm);
}
