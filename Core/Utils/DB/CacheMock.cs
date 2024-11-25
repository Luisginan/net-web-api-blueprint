using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class CacheMock : ICache
{
    public string? GetString(string key)
    {
        return null;
    }

    public async Task<string?> GetStringAsync(string key)
    {
        return await Task.FromResult<string?>(null);
    }

    public void Remove(string key)
    {

    }

    public async Task RemoveAsync(string key)
    {
        await Task.CompletedTask;
    }
    public void SetString(string key, string value)
    {

    }
    public async Task SetStringAsync(string key, string value)
    {
        await Task.CompletedTask;
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await Task.FromResult(true);
    }

    public bool KeyExists(string key)
    {
        return false;
    }
}