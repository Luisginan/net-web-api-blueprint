using Blueprint.CustomerModule.Controllers;
using Blueprint.CustomerModule.DTO;
using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BlueprintTest.ProductControllerTest;

public class ProductControllerGetTest
{
    [Fact]
    public async void GetProduct_Ok()
    {
        var mockProductExternal = new Mock<IProductExternal>();
        var productController = new ProductController(mockProductExternal.Object);

        mockProductExternal.Setup(x => x.GetProduct("1"))
            .Returns(new Product
            {
                Id = 1,
                Description = "Mock description",
                Brand = "Mock brand",
                Category = "Mock category",
                Thumbnail = "Mock thumbnail",
                Title = "Mock title"
            });
        var result  = await productController.Get("1");
        
        Assert.NotNull(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var productResponse = Assert.IsType<ProductResponse>(okObjectResult.Value);
        
        Assert.Equal(1, productResponse.Id);
        Assert.Equal("Mock description", productResponse.Description);
        Assert.Equal("Mock brand", productResponse.Brand);
        Assert.Equal("Mock category", productResponse.Category);
        Assert.Equal("Mock thumbnail", productResponse.Thumbnail);
        Assert.Equal("Mock title", productResponse.Title);
    }
    
    [Fact]
    public async void GetProduct_NotFound()
    {
        var mockProductExternal = new Mock<IProductExternal>();
        var productController = new ProductController(mockProductExternal.Object);

        mockProductExternal.Setup(x => x.GetProduct("1"))
            .Returns((Product?)null);
        var result  = await productController.Get("1");
        
        Assert.NotNull(result);
        Assert.IsType<NotFoundObjectResult>(result);
        
    }
}