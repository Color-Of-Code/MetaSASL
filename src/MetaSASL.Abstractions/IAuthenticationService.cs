namespace Sasl.Abstractions;

public interface IAuthenticationService
{
    // Return true if the user can be successfully authenticated by the service
    bool Authenticate(string login, string password, string service, string realm);
}
