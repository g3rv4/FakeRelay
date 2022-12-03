using System.Collections.Immutable;
using System.Security.Cryptography;
using Jil;

namespace FakeRelay.Core.Helpers;

public static class ApiKeysHelper
{
    private static string? _tokensFilePath;
    private static string TokensFilePath => _tokensFilePath ??= Config.Instance.ConfigPath.Replace(".json", "-tokens.json");

    public static async Task<ImmutableDictionary<string, string>> GetTokenToHostAsync()
    {
        if (!File.Exists(TokensFilePath))
        {
            return ImmutableDictionary<string, string>.Empty;
        }
        
        var content = await File.ReadAllTextAsync(TokensFilePath);
        return JSON.Deserialize<Dictionary<string, string>>(content)
            .ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public static async Task<string> UpdateTokenForHostAsync(string host)
    {
        var dict = await GetTokenToHostAsync();
        var hostToKeys = dict.ToLookup(d => d.Value, d => d.Key, StringComparer.OrdinalIgnoreCase);
        if (!hostToKeys.Contains(host))
        {
            throw new ArgumentException("The host doesn't have a key", nameof(host));
        }

        foreach (var key in hostToKeys[host])
        {
            dict = dict.Remove(key);
        }

        return await AddTokenForHostAsync(host, dict);
    }

    public static async Task<string> AddTokenForHostAsync(string host) =>
        await AddTokenForHostAsync(host, await GetTokenToHostAsync());

    public static async Task DeleteTokenForHostAsync(string host)
    {
        var dict = await GetTokenToHostAsync();
        var hostToKeys = dict.ToLookup(d => d.Value, d => d.Key, StringComparer.OrdinalIgnoreCase);
        if (!hostToKeys.Contains(host))
        {
            throw new ArgumentException("The host does not have a key", nameof(host));
        }

        foreach (var key in hostToKeys[host])
        {
            dict = dict.Remove(key);
        }
        
        var content = JSON.Serialize(dict);
        await File.WriteAllTextAsync(TokensFilePath, content);
    }

    private static async Task<string> AddTokenForHostAsync(string host, ImmutableDictionary<string, string> dict)
    {
        var hostToKeys = dict.ToLookup(d => d.Value, d => d.Key, StringComparer.OrdinalIgnoreCase);
        if (hostToKeys.Contains(host))
        {
            throw new ArgumentException("The host already has a key", nameof(host));
        }
        
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var content = JSON.Serialize(dict.Add(key, host));
        await File.WriteAllTextAsync(TokensFilePath, content);

        return key;
    }
}