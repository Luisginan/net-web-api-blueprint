using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Core.Config;
using Core.Utils.Security;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class Cache(
    IDistributedCache distributedCache,
    IOptions<CacheConfig> cacheConfig,
    IVault vault,
    ILogger<Cache> logger)
    : ICache
{
    private readonly CacheConfig _cacheConfig = vault.RevealSecret(cacheConfig.Value);
    private readonly ActivitySource _activitySource = new("cache");

    public async Task<string?> GetStringAsync(string key)
    {
        using var activity = _activitySource.StartActivity();
        var data= await distributedCache.GetStringAsync(key);
        logger.LogDebug("Cache GetStringAsync: {key} {data}", key, data);
        activity?.SetTag("key", key);
        return data;
    }

    public async Task SetStringAsync(string key, string value)
    {
        using var activity = _activitySource.StartActivity();
        await distributedCache.SetStringAsync(key, value, CacheOption);
        logger.LogDebug("Cache SetStringAsync: {key} {value}", key, value);
        activity?.SetTag("key", key);
        activity?.SetTag("DurationMinutes", _cacheConfig.DurationMinutes);
    }

    public async Task RemoveAsync(string key)
    {
        using var activity = _activitySource.StartActivity();
        await distributedCache.RemoveAsync(key);
        logger.LogDebug("Cache RemoveAsync: {key}", key);
        activity?.SetTag("key", key);
    }

    public string? GetString(string key)
    {
        using var activity = _activitySource.StartActivity();
        var data =  distributedCache.GetString(key);
        logger.LogDebug("Cache GetString: {key} {data}", key, data);
        activity?.SetTag("key", key);
        return data;
    }

    public void SetString(string key, string value)
    {
        using var activity = _activitySource.StartActivity();
        distributedCache.SetString(key, value, CacheOption);
        logger.LogDebug("Cache SetString: {key} {value}", key, value);
        activity?.SetTag("key", key);
        activity?.SetTag("DurationMinutes", _cacheConfig.DurationMinutes);
    }

    public void Remove(string key)
    {
        using var activity = _activitySource.StartActivity(ActivityKind.Client);
        distributedCache.Remove(key);
        logger.LogDebug("Cache Remove: {key}", key);
        activity?.SetTag("key", key);
    }
    public async Task<bool> KeyExistsAsync(string key)
    {
        using var activity = _activitySource.StartActivity();
        bool exists = string.IsNullOrEmpty(await distributedCache.GetStringAsync(key));
        logger.LogDebug("Cache RemoveAsync: {key}", key);
        activity?.SetTag("key", key);
        return exists;
    }

    public bool KeyExists(string key)
    {
        using var activity = _activitySource.StartActivity();
        bool exists = string.IsNullOrEmpty(distributedCache.GetString(key));
        logger.LogDebug("Cache KeyExists: {key}", key);
        activity?.SetTag("key", key);
        activity?.SetTag("DurationMinutes", _cacheConfig.DurationMinutes);
        return exists;
    }

    private DistributedCacheEntryOptions CacheOption =>
        new()
        {
            AbsoluteExpirationRelativeToNow = new TimeSpan(0, _cacheConfig.DurationMinutes, 0),
            SlidingExpiration = new TimeSpan(0, _cacheConfig.DurationMinutes / 2, 0)
        };
}