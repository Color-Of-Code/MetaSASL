namespace Sasl.Abstractions;

public interface IRealmSecret
{
    // User login
    string Login { get; }
    // User password or token
    string Password { get; }
}
