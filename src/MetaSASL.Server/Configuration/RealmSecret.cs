using Sasl.Abstractions;

namespace Sasl.Configuration;

public class RealmSecret : IRealmSecret
{
    public string Login { get; set; }
    public string Password { get; set; }

    public override string ToString()
    {
        var pseudoPassword = string.IsNullOrEmpty(Password) ? "not set" : "***";
        return $"Secret[{Login} (pass: {pseudoPassword}])";
    }
}
