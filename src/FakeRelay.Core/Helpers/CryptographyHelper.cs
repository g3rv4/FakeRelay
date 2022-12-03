using System.Security.Cryptography;
using System.Text;

namespace FakeRelay.Core.Helpers;

public class CryptographyHelper
{
    public static string GetSHA256Digest(string content)
    {
        using var sha256 = SHA256.Create();
        var hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hashValue);
    }

    public static string Sign(string stringToSign)
    {
        using var rsaProvider = new RSACryptoServiceProvider();
        using var sha256 = SHA256.Create();
        rsaProvider.ImportRSAPrivateKey(Config.Instance.PrivateKey, out _);
    
        var signature = rsaProvider.SignData(Encoding.UTF8.GetBytes(stringToSign), sha256);
        return Convert.ToBase64String(signature);
    }

    public static (string publicKey, string privateKey) GenerateKeys()
    {
        using var rsa = RSA.Create(4096);
        
        var pubKeyPem = new string(PemEncoding.Write("PUBLIC KEY", rsa.ExportSubjectPublicKeyInfo()));
        var privKeyPem = new string(PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey()));

        return (pubKeyPem, privKeyPem);
    }
}
