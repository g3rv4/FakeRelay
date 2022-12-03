using System.Security.Cryptography;
using Jil;

namespace FakeRelay.Core;

public class Config
{
    public static Config? Instance { get; private set; }
    
    public byte[] PrivateKey { get; }
    public string PublicKey { get; }
    public string Host { get; }
    
    public string ConfigPath { get; }
    public string? HomeRedirect { get; }

    private Config(string publicKey, byte[] privateKey, string host, string configPath, string? homeRedirect)
    {
        PrivateKey = privateKey;
        PublicKey = publicKey;
        Host = host;
        ConfigPath = configPath;
        HomeRedirect = homeRedirect;
    }

    public static void Init(string path)
    {
        if (Instance != null)
        {
            return;
        }

        var data = JSON.Deserialize<ConfigData>(File.ReadAllText(path));
        if (data.PublicKey == null || data.PrivateKey == null || data.Host == null)
        {
            throw new Exception("Missing config parameters");
        }
        
        using var rsa = RSA.Create();
        rsa.ImportFromPem(data.PrivateKey.ToCharArray());
        
        Instance = new Config(data.PublicKey, rsa.ExportRSAPrivateKey(), data.Host, path, data.HomeRedirect);
    }

    public static void CreateConfig(string path, string host, string publicKey, string privateKey)
    {
        if (File.Exists(path))
        {
            throw new Exception("Can't create a new config file, there's one at " + path);
        }

        var data = new ConfigData { Host = host, PublicKey = publicKey, PrivateKey = privateKey };
        File.WriteAllText(path, JSON.Serialize(data));
    }

    private class ConfigData
    {
        public string? PublicKey { get; set; }
        public string? PrivateKey { get; set; }
        public string? Host { get; set; }
        public string? HomeRedirect { get; set; }
    }
}
