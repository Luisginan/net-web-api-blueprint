using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using Core.CExceptions;
using Core.Utils.DB;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Core.Base;

[ExcludeFromCodeCoverage]
public abstract class ServiceBase(ICache cache, ILogger<ServiceBase> logger, IHttpClientFactory httpClientFactory)
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
                logger.LogDebug("ServiceBase Cache Found: {info}", data);
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
                    logger.LogDebug("ServiceBase Cache Found: {info}", cachedData);
                    return JsonConvert.DeserializeObject<T>(cachedData ?? string.Empty);
                }
                catch (JsonException je)
                {
                    logger.LogError("ServiceBase Cache Error: {info}", je.Message);
                }
                catch (Exception e)
                {
                    logger.LogError("ServiceBase Cache Error: {info}", e.Message);
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
                    logger.LogInformation("ServiceBase Cache Found: {info}", data);
                    return JsonConvert.DeserializeObject<T>(data ?? string.Empty);
                }
                catch (JsonException je)
                {
                    logger.LogError("ServiceBase Cache Error: {info}", je.Message);
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
                    logger.LogError("ServiceBase ClearCache Error: {info}", ex.Message);
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
    protected T? GetData<T>(string url) where T : class
    {
        T? result;
        using var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(url);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = client.GetAsync(url).Result;
        if (response.IsSuccessStatusCode)
        {
            var responseString = response.Content.ReadAsStringAsync().Result;
            result = JsonConvert.DeserializeObject<T>(responseString);
        } else
        {
            throw new ServiceException("Error while fetching data from API " + response.StatusCode);
        }

        return result;
    }
    public string PostData(string url, string data)
    {
        string result;
        using var client = new HttpClient();
        client.BaseAddress = new Uri(url);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = client.PostAsync(url, new StringContent(data, Encoding.UTF8, "application/json")).Result;
        if (response.IsSuccessStatusCode)
        {
            result = response.Content.ReadAsStringAsync().Result;
        }else
        {
            throw new ServiceException("Error while posting data to API " + response.StatusCode);
        }
        return result;
    }

    //put data to api
    public string PutData(string url, string data)
    {
        var result = string.Empty;
        using var client = new HttpClient();
        client.BaseAddress = new Uri(url);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = client.PutAsync(url, new StringContent(data, Encoding.UTF8, "application/json")).Result;
        if (response.IsSuccessStatusCode)
        {
            result = response.Content.ReadAsStringAsync().Result;
        } else
        {
            throw new ServiceException("Error while updating data to API " + response.StatusCode);
        }

        return result;
    }

    //delete data from api
    public string DeleteData(string url)
    {
        string result;
        using var client = new HttpClient();
        client.BaseAddress = new Uri(url);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = client.DeleteAsync(url).Result;
        if (response.IsSuccessStatusCode)
        {
            result = response.Content.ReadAsStringAsync().Result;
        }
        else
        {
            throw new ServiceException("Error while deleting data from API " + response.StatusCode);
        }

        return result;
    }
}