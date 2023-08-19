using System.Collections.Immutable;
using System.Text.RegularExpressions;
using FakeRelay.Core;
using FakeRelay.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Prometheus;

namespace FakeRelay.Web.Controllers;

public class ApiController : Controller
{
    private readonly IMemoryCache _memoryCache;
    private static readonly Counter IndexRequests =
        Metrics.CreateCounter("index_requests", "Requests to index statuses", "instance");
    private static readonly Counter IndexRequestsTimedOut =
        Metrics.CreateCounter("index_requests_timed_out", "Requests to index statuses that timed out", "instance");
    private static readonly Counter IndexRequestsFailed =
        Metrics.CreateCounter("index_requests_failed", "Requests to index statuses that failed", "instance");

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

        string? response;
        try
        {
            response = await MastodonHelper.EnqueueStatusToFetchAsync(host, statusUrl);
            IndexRequests.WithLabels(host).Inc();
        }
        catch (TaskCanceledException)
        {
            response = "2s timeout exceeded";
            IndexRequestsTimedOut.WithLabels(host).Inc();
        }
        catch (Exception e)
        {
            response = $"Error: {e.Message}";
            Console.WriteLine($"Error indexing for host {host}: {e}");
            IndexRequestsFailed.WithLabels(host).Inc();
        }

        Response.Headers["instance"] = host;
        return Content(response, "application/activity+json");
    }

    [Route("index-posts-count")]
    public async Task<ActionResult> IndexedPostsCount(string period)
    {
        if (Config.Instance.GrafanaHost.IsNullOrEmpty() || Config.Instance.GrafanaKey.IsNullOrEmpty() || !Config.Instance.GrafanaDataSourceId.HasValue)
        {
            return NotFound();
        }

        if (period.IsNullOrEmpty() || !Regex.IsMatch(period, "^[0-9]+[mhd]$"))
        {
            return BadRequest();
        }
        
        var host = await GetHostFromRequest();
        if (host == null)
        {
            return Unauthorized();
        }

        var count = await GrafanaHelper.GetCountInPeriod(host, period);
        return Content(count.ToString());
    }
}
