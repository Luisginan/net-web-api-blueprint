using System.Diagnostics.CodeAnalysis;
using Core.CExceptions;
using Core.Utils.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Core.Base;

[ExcludeFromCodeCoverage]
[ApiController]
public abstract class SuperController(ICache cache, ILogger logger, IConnection dbConnection) : ControllerBase
{
    protected abstract string CacheKeyRoot { get; }
        
    private string GetCacheKey(string key)
    {
            return CacheKeyRoot + ".single." + key;
        }

    private string GetListCacheKey(string key)
    {
            return CacheKeyRoot + ".list." + key;
        }
    protected T? UseListCache<T>(string key, Func<T> action)
    {
            var cacheKey = GetListCacheKey(key);
            var data = cache.GetString(cacheKey);
            if (!data.IsNullOrEmpty())
            {
                logger.LogDebug("SuperController Cache Found: {info}", data);
                return JsonConvert.DeserializeObject<T>(data ?? string.Empty);
            }
            var result = action();
            if (result != null)
                cache.SetString(cacheKey, JsonConvert.SerializeObject(result));
            return result;
        }

    protected async Task<T?> UseListCacheAsync<T>(string key, Func<Task<T>> action)
    {
            var cacheKey = GetListCacheKey(key);

            var cachedData = await cache.GetStringAsync(cacheKey);

            if (!cachedData.IsNullOrEmpty())
            {
                try
                {
                    logger.LogDebug("SuperController Cache Found: {info}", cachedData);
                    return JsonConvert.DeserializeObject<T>(cachedData ?? string.Empty);
                }
                catch (JsonException je)
                {
                    logger.LogError("SuperController Cache Error: {info}", je.Message);
                }
                catch (Exception e)
                {
                    logger.LogError("SuperController Cache Error: {info}", e.Message);
                }
            }
            var result = await action();
            if (result != null)
                await cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(result));

            return result;
        }

    protected T? UseCache<T>(string key, Func<T> action)
    {
            var cacheKey = GetCacheKey(key);
            var data = cache.GetString(cacheKey);
            if (!data.IsNullOrEmpty())
            {
                return JsonConvert.DeserializeObject<T>(data ?? string.Empty);
            }
            var result = action();
            if (result != null)
                cache.SetString(cacheKey, JsonConvert.SerializeObject(result));
            return result;
        }

    protected async Task<T?> UseCacheAsync<T>(string key, Func<Task<T>> action)
    {
            var data = await cache.GetStringAsync(GetCacheKey(key));
            
            if (!data.IsNullOrEmpty())
            {
                try
                {
                    logger.LogInformation("SuperController Cache Found: {info}", data);
                    return JsonConvert.DeserializeObject<T>(data ?? string.Empty);
                }
                catch (JsonException je)
                {
                    logger.LogError("SuperController Cache Error: {info}", je.Message);
                }
            }
            
            var result = await action();
            if (result != null)
                await cache.SetStringAsync(GetCacheKey(key), JsonConvert.SerializeObject(result));
            return result;
        }
    protected void ClearCache(string key, Action action)
    {
            action();
            cache.Remove(GetCacheKey(key));
        }

    protected void ClearListCache(string key, Action action)
    {
            action();
            cache.Remove(GetListCacheKey(key));
        }

    protected async Task ClearCacheAsync(string key, Func<Task>? action)
    {
            if (action != null)
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    logger.LogError("SuperController ClearCache Error: {info}", ex.Message);
                }

                await cache.RemoveAsync(GetCacheKey(key));
            }
    }

    protected async Task ClearListCacheAsync(string key, Func<Task>? action)
    {
        // Allow null actions for optional cache clearing
        if (action != null)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                // Handle potential errors in the action
                logger.LogError(ex, "SuperController ClearListCacheAsync Error: {info}", ex.Message);
            }
        }
        await cache.RemoveAsync(GetListCacheKey(key));
    }
        
    protected async Task UseDbTransaction(Func<Task>? action)
    {
        try
        {
            dbConnection.BeginTransaction();
            if (action != null)
            {
                await action();
                dbConnection.Commit();
            }
            else
            {
                throw new DbTransactionException("UseDbTransaction Action is null");
            }
        }
        catch (Exception ex)
        {
            dbConnection.Rollback();
            logger.LogError(ex, "SuperController UseDbTransaction Error: {info}", ex.Message);
            throw;
        }
    }

    protected string GetApiKey()
    {            
        var apiKey = HttpContext.Request.Headers["api-key"].ToString();
        return string.IsNullOrEmpty(apiKey)?"":apiKey;
    }

    protected Partner GetPartner()
    {
        var appId = HttpContext.Request.Headers["appId"].ToString();
        var partnerName = HttpContext.Request.Headers["partnerName"].ToString();
        var principle = HttpContext.Request.Headers["principleName"].ToString();
        var roles = HttpContext.Request.Headers["roles"].ToString();

        var partner = new Partner
        {
            AppId = string.IsNullOrEmpty(appId) ? "" : appId,
            PartnerName = string.IsNullOrEmpty(partnerName) ? "" : partnerName,
            PrincipleName = string.IsNullOrEmpty(principle) ? "" : principle,
            Roles = string.IsNullOrEmpty(roles) ? "" : roles
        };
                
        return partner;
    }

    protected string GetAppId()
    {
        return GetPartner().AppId;
    }
    protected string GetPartnerName()
    {
        var partner = GetPartner();
        return string.IsNullOrEmpty(partner.PartnerName) ? partner.PrincipleName : partner.PartnerName;
    }


}