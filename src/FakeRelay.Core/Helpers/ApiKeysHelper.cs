using System.Collections.Immutable;
using System.Security.Cryptography;
using Jil;

namespace FakeRelay.Core.Helpers;

public static class ApiKeysHelper
{
    private static string? _tokensFilePath;
    private static string TokensFilePath =>
        _tokensFilePath ??= Config.Instance.ConfigPath.Replace(".json", "-tokens.json");
    
    private static string? _notesFilePath;
    private static string NotesFilePath =>
        _notesFilePath ??= Config.Instance.ConfigPath.Replace(".json", "-notes.json");

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

    public static async Task<Dictionary<string, string>> GetHostToNotesAsync()
    {
        if (!File.Exists(NotesFilePath))
        {
            return new Dictionary<string, string>();
        }
        
        var content = await File.ReadAllTextAsync(NotesFilePath);
        return JSON.Deserialize<Dictionary<string, string>>(content)
            // set the comparer so that it's case insensitive
            .ToDictionary(e => e.Key, e => e.Value, StringComparer.OrdinalIgnoreCase);
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

    public static async Task UpdateNotesForHostAsync(string host, string? notes)
    {
        var tokensToHost = await GetTokenToHostAsync();
        if (!tokensToHost.Values.Contains(host))
        {
            throw new Exception($"There's no entry for {host}");
        }

        var notesDict = await GetHostToNotesAsync();
        if (notes.HasValue())
        {
            notesDict[host] = notes;
        }
        else if (notesDict.ContainsKey(host))
        {
            notesDict.Remove(host);
        }
        else
        {
            return;
        }

        var content = JSON.Serialize(notesDict);
        await File.WriteAllTextAsync(NotesFilePath, content);
    }

    public static async Task<string> AddTokenForHostAsync(string host, string? notes)
    {
        var token = await AddTokenForHostAsync(host, await GetTokenToHostAsync());
        
        if (notes.HasValue())
        {
            await UpdateNotesForHostAsync(host, notes);
        }

        return token;
    }

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
