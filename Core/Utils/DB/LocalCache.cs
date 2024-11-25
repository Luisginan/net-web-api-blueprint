using System.Diagnostics.CodeAnalysis;
using Core.Config;
using Core.Utils.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Core.Utils.DB;
[ExcludeFromCodeCoverage]
public class LocalCache(IMemoryCache memoryCache,  
    IOptions<CacheConfig> cacheConfig,
    IVault vault,
    ILogger<LocalCache> logger) : ICache
{
    private readonly CacheConfig _cacheConfig = vault.RevealSecret(cacheConfig.Value);
    public async Task<string?> GetStringAsync(string key)
    {
        var result = await Task.Run(() => GetString(key));
        return result;
    }

    public async Task SetStringAsync(string key, string value)
    {
        await Task.Run(() =>
        {
            SetString(key, value);
        });
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await Task.Run(() =>
        {
            var result = memoryCache.Get<string>(key);
            logger.LogDebug("LocalCache KeyExistsAsync: {key} {data}", key, result);
            return result != null;
        });
    }
    public async Task RemoveAsync(string key)
    {
        await Task.Run(() => memoryCache.Remove(key));
    }

    public void Remove(string key)
    {
        memoryCache.Remove(key);
        logger.LogDebug("LocalCache Remove: {key}", key);
    }

    public string? GetString(string key)
    {
        var result=  memoryCache.Get<string>(key);
        logger.LogDebug("LocalCache GetString: {key} {data}", key, result);
        return result; 
    }
    public bool KeyExists(string key) {
        var result = memoryCache.Get<string>(key);
        logger.LogDebug("LocalCache KeyExists: {key} {data}", key, result);
        return result != null;
    }
    public void SetString(string key, string value)
    {
        memoryCache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheConfig.DurationMinutes)
        });
        logger.LogDebug("LocalCache SetString: {key} {value}", key, value);
    }
}