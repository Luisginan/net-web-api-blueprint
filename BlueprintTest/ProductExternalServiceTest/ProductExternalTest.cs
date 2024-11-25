using System.Net;
using Blueprint.CustomerModule.Services;
using Core.Base;
using Core.CExceptions;
using Core.Utils.DB;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlueprintTest.ProductExternalServiceTest;

public class ProductExternalTests
{
    private readonly Mock<ICache> _cacheMock;
    private readonly Mock<ILogger<ServiceBase>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly ProductExternal _productExternal;

    public ProductExternalTests()
    {
        _cacheMock = new Mock<ICache>();
        _loggerMock = new Mock<ILogger<ServiceBase>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var httpClient = new HttpClient(new MockHttpMessageHandler());
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _productExternal = new ProductExternal(_cacheMock.Object, _loggerMock.Object, _httpClientFactoryMock.Object);
    }

    [Fact]
    public void GetProduct_ReturnsProduct_WhenProductIdIsValid()
    {
        var product = _productExternal.GetProduct("1");
        Assert.NotNull(product);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public void GetProduct_ReturnsNull_WhenProductIdIsInvalid()
    {
        //get  exception
        var ex = Record.Exception(() => _productExternal.GetProduct("invalid"));
        Assert.NotNull(ex);
        Assert.IsType<ServiceException>(ex);
        Assert.Equal("Error while fetching data from API NotFound", ex.Message);
    }

    [Fact]
    public async Task GetProductAsync_ReturnsProduct_WhenProductIdIsValid()
    {
        var product = await _productExternal.GetProductAsync("1");
        Assert.NotNull(product);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public async Task GetProductAsync_ReturnsNull_WhenProductIdIsInvalid()
    {
       var ex = await Record.ExceptionAsync(() => _productExternal.GetProductAsync("invalid"));
        Assert.NotNull(ex);
        Assert.IsType<ServiceException>(ex);
        Assert.Equal("Error while fetching data from API NotFound", ex.Message);
    }
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"id\":\"1\",\"name\":\"Test Product\"}")
        };

        if (request.RequestUri != null && request.RequestUri.ToString().Contains("invalid"))
        {
            response = new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        return Task.FromResult(response);
    }
}