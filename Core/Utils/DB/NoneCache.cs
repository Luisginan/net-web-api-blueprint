using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB;
[ExcludeFromCodeCoverage]
public class NoneCache(ILogger<NoneCache> logger): ICache
{
    public async Task<string?> GetStringAsync(string key)
    {
        return await Task.Run(() => GetString(key));
    }

    public async Task SetStringAsync(string key, string value)
    {
        await Task.Run(() =>
        {
            SetString(key, value);
        });
    }

    public async Task RemoveAsync(string key)
    {
        await Task.Run(() => Remove(key));
    }

    public void Remove(string key)
    {
        logger.LogDebug("NoneCache Remove: {key}", "Cache is disabled");
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await Task.Run(() => { return KeyExists(key); });
    }

    public bool KeyExists(string key)
    {
        logger.LogDebug("NoneCache KeyExists: {key}", "Cache is disabled");
        return false;
    }

    public string? GetString(string key)
    {
        logger.LogDebug("NoneCache GetString: {key} ", "Cache is disabled");
        return "";
    }

    public void SetString(string key, string value)
    {
        logger.LogDebug("NoneCache SetString: {key}", "Cache is disabled");
    }
}