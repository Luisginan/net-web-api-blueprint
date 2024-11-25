using Blueprint.CustomerModule.Models;

namespace Blueprint.CustomerModule.Services;

public interface IProductExternal
{
    Product? GetProduct(string productId);
    Task<Product?> GetProductAsync(string productId);
}