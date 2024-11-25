using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Models;

namespace Blueprint.CustomerModule.Services;

[ExcludeFromCodeCoverage]
public class ProductExternalMock : IProductExternal
{
    public Product GetProduct(string productId)
    {
        // generate a mock Product instance
        return new Product
        {
            Id = int.Parse(productId),
            Title = "Mock title",
            Description = "Mock description",
            Price = 99.99m,
            DiscountPercentage = 10,
            Rating = 4.5,
            Stock = 100,
            Brand = "Mock brand",
            Category = "Mock category",
            Thumbnail = "Mock thumbnail",
        };
    }

    public async Task<Product?> GetProductAsync(string productId)
    {
        // simply return the result of synchronous GetProduct
        // as an already completed Task
        return await Task.FromResult(GetProduct(productId));
    }
}