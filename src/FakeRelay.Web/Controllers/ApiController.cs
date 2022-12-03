using System.Collections.Immutable;
using FakeRelay.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace FakeRelay.Web.Controllers;

public class ApiController : Controller
{
    private readonly IMemoryCache _memoryCache;

    public ApiController(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    private async Task<string?> GetHostFromRequest()
    {
        if (Request.Headers.Authorization.Count != 1)
        {
            return null;
        }
        
        if (!_memoryCache.TryGetValue("tokenToHost", out ImmutableDictionary<string, string> tokenToHost))
        {
            tokenToHost = await ApiKeysHelper.GetTokenToHostAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            _memoryCache.Set("tokenToHost", tokenToHost, cacheEntryOptions);
        }
        
        var token = Request.Headers.Authorization[0].Replace("Bearer ", "");
        tokenToHost.TryGetValue(token, out var host);
        return host;
    }

    [Route("index"), HttpPost]
    public async Task<ActionResult> DoIndex(string statusUrl)
    {
        var host = await GetHostFromRequest();
        if (host == null)
        {
            return Unauthorized();
        }
        
        var response = await MastodonHelper.EnqueueStatusToFetchAsync(host, statusUrl);
        return Content(response, "application/activity+json");
    }
}
