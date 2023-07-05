using System.Net.Http.Headers;

namespace FakeRelay.Core.Helpers;

public static class MastodonHelper
{
    public static async Task<string> EnqueueStatusToFetchAsync(string targetHost, string statusUrl)
    {
        if (statusUrl.StartsWithCI($"https://{targetHost}") || statusUrl.StartsWithCI($"http://{targetHost}"))
        {
            return "Status ignored, it's local";
        }
        
        return await SendMessageToInboxAsync(targetHost, $@"{{
    ""@context"": ""https://www.w3.org/ns/activitystreams"",
    ""actor"": ""https://{Config.Instance.Host}/actor"",
    ""id"": ""https://{Config.Instance.Host}/activities/{Guid.NewGuid()}"",
    ""object"": ""{statusUrl}"",
    ""to"": [
        ""https://{Config.Instance.Host}/followers""
    ],
    ""type"": ""Announce""
}}");
    }

    public static async Task<string> SendMessageToInboxAsync(string targetHost, string content)
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        client.DefaultRequestHeaders.Add("User-Agent", $"FakeRelay (hosted at {Config.Instance.Host})");

        var date = DateTime.UtcNow;
        
        var digest = CryptographyHelper.GetSHA256Digest(content);
        var requestContent = new StringContent(content);

        requestContent.Headers.Add("Digest", "SHA-256=" + digest);

        var stringToSign = $"(request-target): post /inbox\ndate: {date.ToString("R")}\nhost: {targetHost}\ndigest: SHA-256={digest}\ncontent-length: {content.Length}";
        var signature = CryptographyHelper.Sign(stringToSign);
        requestContent.Headers.Add("Signature", $@"keyId=""https://{Config.Instance.Host}/actor#main-key"",algorithm=""rsa-sha256"",headers=""(request-target) date host digest content-length"",signature=""{signature}""");

        requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/activity+json");
        client.DefaultRequestHeaders.Date = date;

        try
        {
            var response = await client.PostAsync($"https://{targetHost}/inbox", requestContent);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception {e.Message} when connecting to {targetHost}");
            throw;
        }
    }

    public static async Task ProcessInstanceFollowAsync(ActivityPubModel request)
    {
        var host = request.ActorUrl.Host;
        await SendMessageToInboxAsync(host, $@"{{""@context"": ""https://www.w3.org/ns/activitystreams"", ""type"": ""Accept"", ""to"": [""https://{host}/actor""], ""actor"": ""https://{Config.Instance.Host}/actor"", ""object"": {{""type"": ""Follow"", ""id"": ""{request.Id}"", ""object"": ""https://{Config.Instance.Host}/actor"", ""actor"": ""{request.Actor}""}}, ""id"": ""https://{Config.Instance.Host}/activities/{Guid.NewGuid()}""}}");
    }

    public static string GetActorStaticContent() =>
        $@"{{""@context"": ""https://www.w3.org/ns/activitystreams"", ""endpoints"": {{""sharedInbox"": ""https://{Config.Instance.Host}/inbox""}}, ""followers"": ""https://{Config.Instance.Host}/followers"", ""following"": ""https://{Config.Instance.Host}/following"", ""inbox"": ""https://{Config.Instance.Host}/inbox"", ""name"": ""FakeRelay"", ""type"": ""Application"", ""id"": ""https://{Config.Instance.Host}/actor"", ""publicKey"": {{""id"": ""https://{Config.Instance.Host}/actor#main-key"", ""owner"": ""https://{Config.Instance.Host}/actor"", ""publicKeyPem"": ""{Config.Instance.PublicKey.Replace("\n", "\\n")}""}}, ""summary"": ""FakeRelay bot"", ""preferredUsername"": ""relay"", ""url"": ""https://{Config.Instance.Host}/actor""}}";

    public static string GetActorWebFinger() =>
        $@"{{""subject"": ""acct:relay@{Config.Instance.Host}"", ""aliases"": [""https://{Config.Instance.Host}/actor""], ""links"": [{{""href"": ""https://{Config.Instance.Host}/actor"", ""rel"": ""self"", ""type"": ""application/activity+json""}}, {{""href"": ""https://{Config.Instance.Host}/actor"", ""rel"": ""self"", ""type"": ""application/ld+json; profile=\""https://www.w3.org/ns/activitystreams\""""}}]}}";
}
