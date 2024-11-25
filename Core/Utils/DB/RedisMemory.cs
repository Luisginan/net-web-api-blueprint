using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Core.Config;
using Core.Utils.Security;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class RedisMemory : ICache
{
    private readonly CacheConfig _cacheConfig;
    private readonly ActivitySource _activitySource = new("cache");
    private readonly IDatabase _database;
    private readonly ILogger<RedisMemory> _logger;

    public RedisMemory(
        IOptions<CacheConfig> cacheConfig,
        IVault vault,
        ILogger<RedisMemory> logger)
    {
        _logger = logger;
        _cacheConfig = vault.RevealSecret(cacheConfig.Value);
        var config = new ConfigurationOptions
        {
            EndPoints = { $"{_cacheConfig.Server}:{_cacheConfig.Port}" },
            AbortOnConnectFail = false
        };
        var connectionMultiplexer = ConnectionMultiplexer.Connect(config);
        _database = connectionMultiplexer.GetDatabase();
        if (connectionMultiplexer.IsConnected)
        {
            _logger.LogDebug("RedisMemory : Redis Cache is connected");
        }
        else
        {
            _logger.LogDebug("RedisMemory : Redis Cache is not connected");
        }
    }

    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            if (!IsAccessable()) return null;
            using var activity = _activitySource.StartActivity();
            var data = await _database.StringGetAsync(key);
            _logger.LogDebug("RedisMemory GetStringAsync: {key} {data}", key, data);
            activity?.SetTag("key", key);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "RedisMemory GetStringAsync Error : " + ex.Message);
            return null;
        }
    }
    public bool IsAccessable()
    {
        return !(_database == null || (_database != null && !_database.Multiplexer.IsConnected));
    }


    public async Task SetStringAsync(string key, string value)
    {
        try
        {
            if (!IsAccessable()) return;

            using var activity = _activitySource.StartActivity();
            await _database.StringSetAsync(key, value, TimeSpan.FromMinutes(_cacheConfig.DurationMinutes));
            _logger.LogDebug("RedisMemory SetStringAsync: {key}", key);
            activity?.SetTag("key", key);
            activity?.SetTag("DurationMinutes", _cacheConfig.DurationMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "RedisMemory SetStringAsync Error : " + ex.Message);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            if (!IsAccessable()) return;
            using var activity = _activitySource.StartActivity();
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug("RedisMemory RemoveAsync: {key}", key);
            activity?.SetTag("key", key);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "RedisMemory RemoveAsync Error : " + ex.Message);
        }
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        try
        {
            if (!IsAccessable()) return false;
            using var activity = _activitySource.StartActivity();
            bool exists  = await _database.KeyExistsAsync(key);
            _logger.LogDebug("RedisMemory KeyExistsAsync: {key}", key);
            activity?.SetTag("key", key);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "RedisMemory RemoveAsync Error : " + ex.Message);
            return false;
        }
    }

    public string? GetString(string key)
    {
        try
        {
            if (!IsAccessable()) return null;
            using var activity = _activitySource.StartActivity();
            var data = _database.StringGet(key);
            _logger.LogDebug("RedisMemory GetString: {key}", key);
            activity?.SetTag("key", key);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "RedisMemory GetString Error : " + ex.Message);
            return null;
        }
    }

    public void SetString(string key, string value)
    {
        try
        {
            if (!IsAccessable()) return;
            using var activity = _activitySource.StartActivity();
            _database.StringSet(key, value, TimeSpan.FromMinutes(_cacheConfig.DurationMinutes));
            _logger.LogDebug("RedisMemory SetString: {key}", key);
            activity?.SetTag("key", key);
            activity?.SetTag("DurationMinutes", _cacheConfig.DurationMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "RedisMemory SetString Error : " + ex.Message);
        }
    }
    public bool KeyExists(string key)
    {
        try
        {
            if (!IsAccessable()) return false;

            using var activity = _activitySource.StartActivity(ActivityKind.Client);
            bool exists = _database.KeyExists(key);
            _logger.LogDebug("RedisMemory KeyExists: {key}", key);
            activity?.SetTag("key", key);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "RedisMemory Remove Error : " + ex.Message);
            return false;
        }
    }

    public void Remove(string key)
    {
        try
        {
            if (!IsAccessable()) return;

            using var activity = _activitySource.StartActivity(ActivityKind.Client);
            _database.KeyDelete(key);
            _logger.LogDebug("RedisMemory Remove: {key}", key);
            activity?.SetTag("key", key);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "RedisMemory Remove Error : " + ex.Message);
        }
    }
}