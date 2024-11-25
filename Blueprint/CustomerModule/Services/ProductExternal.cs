using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Models;
using Core.Base;
using Core.Utils.DB;

namespace Blueprint.CustomerModule.Services;

public class ProductExternal(ICache cache, ILogger<ServiceBase> loggerBase,IHttpClientFactory httpClientFactory) : ServiceBase(cache, loggerBase, httpClientFactory), IProductExternal

{
    public Product? GetProduct(string productId) => GetData<Product>("https://dummyjson.com/products/" + productId);

    public Task<Product?> GetProductAsync(string productId) => Task.FromResult(GetProduct(productId));
    [ExcludeFromCodeCoverage]
    protected override string CacheKeyRoot => "Blueprint.CustomerModule.Services.ProductExternal";
}