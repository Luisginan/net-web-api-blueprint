using AutoMapper;
using Blueprint.CustomerModule.Controllers;
using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Blueprint.CustomerModule.DTO;
using Core.Utils.DB;
using ICache = Core.Utils.DB.ICache;
using Newtonsoft.Json;

namespace BlueprintTest.CustomerControllerTest.GetTest;

public class CustomerControllerGetTest
{
    /// <summary>
    /// not found in cache but found in database
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Get_Success()
    {
        var mockCache = new Mock<ICache>();
        var mockCustomerService = new Mock<ICustomerService>();
        var mockLogger = new Mock<ILogger<CustomerController>>();
        var mockMapper = new Mock<IMapper>();
        var mockDbConnection = new Mock<IConnection>();

        // not found in cache
        mockCache.Setup(x => x.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);
        
        var customerFromDb = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Address = "123 Main",
            Email2 = "New York",
            Phone = "123-456-7890"
        };
            
        // found in database
        mockCustomerService.Setup(repo => repo.GetCustomerAsync(It.IsAny<int>()))
            .ReturnsAsync(customerFromDb);
            
        // assume mapper is working
        mockMapper.Setup(x => x.Map<CustomerResponse>(It.IsAny<Customer>()))
            .Returns(new CustomerResponse
            {
                Id = customerFromDb.Id,
                Name = customerFromDb.Name,
                Address = customerFromDb.Address,
                Email = customerFromDb.Email2,
                Phone = customerFromDb.Phone
            });
            
        // init controller
        var controller =
            new CustomerController(mockCustomerService.Object, mockCache.Object, mockLogger.Object, mockDbConnection.Object,  mockMapper.Object);

        // Act
        var result = await controller.Get(1);

        // Assert result should be ok and return the correct customer from database
        var viewResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsAssignableFrom<CustomerResponse>(viewResult.Value);

        Assert.Equal(1, responseDto.Id);
        Assert.Equal("John Doe", responseDto.Name);
        Assert.Equal("123 Main", responseDto.Address);
        Assert.Equal("New York", responseDto.Email);
        Assert.Equal("123-456-7890", responseDto.Phone);
    }

    /// <summary>
    /// not found in cache and notfound in database
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Get_NotFound()
    {
        var mockCustomerService = new Mock<ICustomerService>();
        var mockCache = new Mock<ICache>();
        var mockLogger = new Mock<ILogger<CustomerController>>();
        var mockMapper = new Mock<IMapper>();
        var mockDbConnection = new Mock<IConnection>();
            

        //not found in cache
        mockCache.Setup(x => x.GetStringAsync(It.IsAny<string>())).ReturnsAsync((string?)null);

        // not found in database
        mockCustomerService.Setup(repo => repo.GetCustomerAsync(It.IsAny<int>()))
            .ReturnsAsync((Customer?)null);

        // init controller
        var controller = new CustomerController(mockCustomerService.Object, mockCache.Object, mockLogger.Object,mockDbConnection.Object, mockMapper.Object);

        //act
        var result = await controller.Get(1);

        //assert : response should be (404) not found 
        Assert.IsType<NotFoundResult>(result);
    }

    /// <summary>
    /// Found in cache 
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Get_SuccessFromCache()
    {
        var mockCustomerService = new Mock<ICustomerService>();
        var mockLogger = new Mock<ILogger<CustomerController>>();
        var mockCache = new Mock<ICache>();
        var mockMapper = new Mock<IMapper>();
        var mockDbConnection = new Mock<IConnection>();
            
        var customerFromCache = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Address = "123 Main",
            Email2 = "New York",
            Phone = "123-456-7890"
        };
            
        // found in cache
        mockCache.Setup(x => x.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync(JsonConvert.SerializeObject(customerFromCache));
            
        // assume mapper is working
        mockMapper.Setup(x => x.Map<CustomerResponse>(It.IsAny<Customer>()))
            .Returns(new CustomerResponse
            {
                Id = customerFromCache.Id,
                Name = customerFromCache.Name,
                Address = customerFromCache.Address,
                Email = customerFromCache.Email2,
                Phone = customerFromCache.Phone
            });

        // init controller
        var controller = new CustomerController(mockCustomerService.Object, mockCache.Object, mockLogger.Object, mockDbConnection.Object, mockMapper.Object);

        // Act
        var result = await controller.Get(1);

        // Assert result should be ok and return the correct customer from cache
        var viewResult = Assert.IsType<OkObjectResult>(result);

        var responseDto = Assert.IsAssignableFrom<CustomerResponse>(
            viewResult.Value);

        Assert.Equal(customerFromCache.Id, responseDto.Id);
        Assert.Equal(customerFromCache.Name, responseDto.Name);
        Assert.Equal(customerFromCache.Address, responseDto.Address);
        Assert.Equal(customerFromCache.Email2, responseDto.Email);
        Assert.Equal(customerFromCache.Phone, responseDto.Phone);
    }
}