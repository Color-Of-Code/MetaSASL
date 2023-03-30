using Sasl.Abstractions;

namespace Sasl.Configuration;

public class RealmConfiguration
{
    public string Type { get; set; }

    public IRealmConfiguration Settings { get; set; }
    public IRealmSecret Credentials { get; set; }

    public override string ToString()
    {
        return $"Realm[{Type}: {Settings}]";
    }
}
