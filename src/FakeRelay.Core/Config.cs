using System.Security.Cryptography;
using Jil;

namespace FakeRelay.Core;

public class Config
{
    public static Config Instance { get; private set; } = new();
    
    public byte[] PrivateKey { get; }
    public string PublicKey { get; }
    public string Host { get; }
    
    public string ConfigPath { get; }
    public string? HomeRedirect { get; }
    public string? GrafanaHost { get; }
    public string? GrafanaKey { get; }
    public int? GrafanaDataSourceId { get; }

    private Config()
    {
        PrivateKey = Array.Empty<byte>();
        PublicKey = Host = ConfigPath = "";
    }

    private Config(string publicKey, byte[] privateKey, string host, string configPath, string? homeRedirect, string? grafanaHost, string? grafanaKey, int? grafanaDataSourceId)
    {
        PrivateKey = privateKey;
        PublicKey = publicKey;
        Host = host;
        ConfigPath = configPath;
        HomeRedirect = homeRedirect;
        GrafanaHost = grafanaHost;
        GrafanaKey = grafanaKey;
        GrafanaDataSourceId = grafanaDataSourceId;
    }

    public static void Init(string path)
    {
        if (Instance.Host != "")
        {
            return;
        }

        if (!File.Exists(path))
        {
            throw new Exception("Config file does not exist. Run the config command to create it.");
        }

        var data = JSON.Deserialize<ConfigData>(File.ReadAllText(path));
        if (data.PublicKey == null || data.PrivateKey == null || data.Host == null)
        {
            throw new Exception("Missing config parameters");
        }
        
        using var rsa = RSA.Create();
        rsa.ImportFromPem(data.PrivateKey.ToCharArray());
        
        Instance = new Config(data.PublicKey, rsa.ExportRSAPrivateKey(), data.Host, path, data.HomeRedirect, data.GrafanaHost, data.GrafanaKey, data.GrafanaDataSourceId);
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
        public string? GrafanaHost { get; set; }
        public string? GrafanaKey { get; set; }
        public int? GrafanaDataSourceId { get; set; }
    }
}
