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

    private Config(string publicKey, byte[] privateKey, string host, string configPath)
    {
        PrivateKey = privateKey;
        PublicKey = publicKey;
        Host = host;
        ConfigPath = configPath;
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
        
        Instance = new Config(data.PublicKey, Convert.FromBase64String(data.PrivateKey), data.Host, path);
    }

    private class ConfigData
    {
        public string? PublicKey { get; private set; }
        public string? PrivateKey { get; private set; }
        public string? Host { get; private set; }
    }
}