using Sasl.Abstractions;
using Sasl.Services.Ldap;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sasl.Configuration;

public class ServerConfiguration
{
    private static Secrets LoadSecrets(string path)
    {
        var secretsFile = new FileInfo(Path.Combine(path, ".secrets.yaml"));
        if (!secretsFile.Exists)
            throw new FileNotFoundException($".secrets.yaml configuration MUST exist ({secretsFile})");

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<Secrets>(
            File.ReadAllText(secretsFile.FullName));
    }

    private static SaslConfiguration LoadConfiguration(string path)
    {
        var ymlFile = "sasl.yaml";
        var ldapFile = new FileInfo(Path.Combine(path, ymlFile));
        if (!ldapFile.Exists)
            throw new FileNotFoundException($"{ymlFile} configuration MUST exist ({ldapFile})");

        //yml contains a string containing your YAML
        var yml = File.ReadAllText($"{path}/{ymlFile}");

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithTypeMapping<IRealmConfiguration, LdapConfiguration>() // TODO: need to support more types
            .Build();
        return deserializer.Deserialize<SaslConfiguration>(
            File.ReadAllText(ldapFile.FullName));
    }

    public static ServerConfiguration Load(string path)
    {
        var saslConfig = LoadConfiguration(path);
        var secretsConfig = LoadSecrets(path);

        // build the effective configurations into a dictionary by
        // * taking first the configuration in the realm
        // * fallback to the defaults
        // and recombine the secrets specified in a separate file
        ServerConfiguration c = new();
        foreach (var kv in saslConfig.Realms)
        {
            kv.Value.Credentials = secretsConfig.Realms[kv.Key];
            c._realms.Add(kv.Key, kv.Value);
        }
        return c;
    }

    public IReadOnlyDictionary<string, RealmConfiguration> Realms => _realms;

    private Dictionary<string, RealmConfiguration> _realms = new();
};
