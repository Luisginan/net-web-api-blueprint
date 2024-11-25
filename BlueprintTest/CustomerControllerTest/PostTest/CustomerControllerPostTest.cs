using AutoMapper;
using Blueprint.CustomerModule.Controllers;
using Blueprint.CustomerModule.DTO;
using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Services;
using Core.Utils.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ICache = Core.Utils.DB.ICache;

namespace BlueprintTest.CustomerControllerTest.PostTest;

public class CustomerControllerPostTest
{
     [Fact]
    public async Task Post_Success()
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
            Email2 = "John.Doe@example.com",
            Phone = "123-456-7890"
        };
            
        // found in database
        mockCustomerService.Setup(repo => repo.GetCustomerByEmailAsync(It.IsAny<string>()))
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
        var customerRequest = new CustomerRequest
        {
            Name = "John Doe",
            Address = "123 Main",
            Email = "John.Doe@example.com",
            Phone = "123-456-7890",
        };
        
        var result = await controller.Post(customerRequest);

        // Assert result should be ok and return the correct customer from database
        var viewResult = Assert.IsType<AcceptedResult>(result);
        var responseDto = Assert.IsAssignableFrom<CustomerResponse>(viewResult.Value);

        Assert.Equal(1, responseDto.Id);
        Assert.Equal("John Doe", responseDto.Name);
        Assert.Equal("123 Main", responseDto.Address);
        Assert.Equal("John.Doe@example.com", responseDto.Email);
        Assert.Equal("123-456-7890", responseDto.Phone);
        
        mockCustomerService.Verify(x => x.InsertCustomerAsync(It.IsAny<Customer>()), Times.Once);
        mockCustomerService.Verify(x => x.GetCustomerByEmailAsync(customerRequest.Email), Times.Once);
        mockCache.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
        mockCache.Verify(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task Post_FailedInsertData()
    {
        var mockCache = new Mock<ICache>();
        var mockCustomerService = new Mock<ICustomerService>();
        var mockLogger = new Mock<ILogger<CustomerController>>();
        var mockMapper = new Mock<IMapper>();
        var mockDbConnection = new Mock<IConnection>();
        
            
        // found in database
        mockCustomerService.Setup(repo => repo.GetCustomerByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Customer?) null);
        
        // init controller
        var controller =
            new CustomerController(mockCustomerService.Object, mockCache.Object, mockLogger.Object, mockDbConnection.Object,  mockMapper.Object);

        // Act
        var customerRequest = new CustomerRequest
        {
            Name = "John Doe",
            Address = "123 Main",
            Email = "John.Doe@example.com",
            Phone = "123-456-7890",
        };
        
        var result = await controller.Post(customerRequest);

        // Assert result should be ok and return the correct customer from database
        var viewResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Customer failed to be created",viewResult.Value);
        mockCustomerService.Verify(x => x.InsertCustomerAsync(It.IsAny<Customer>()), Times.Once);
        mockCustomerService.Verify(x => x.GetCustomerByEmailAsync(customerRequest.Email), Times.Once);
        mockCache.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
        mockCache.Verify(x => x.SetStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task Post_FailedModelState()
    {
        var mockCache = new Mock<ICache>();
        var mockCustomerService = new Mock<ICustomerService>();
        var mockLogger = new Mock<ILogger<CustomerController>>();
        var mockMapper = new Mock<IMapper>();
        var mockDbConnection = new Mock<IConnection>();
        
        // init controller
        var controller =
            new CustomerController(mockCustomerService.Object, mockCache.Object, mockLogger.Object, mockDbConnection.Object,  mockMapper.Object);

        // Act
        var customerRequest = new CustomerRequest
        {
            Name = "John Doe",
            Address = "123 Main",
            Email = "", // email required but filled with empty string for error testing
            Phone = "123-456-7890",
        };
        
        controller.ModelState.AddModelError("Email","Email is required");
        var result = await controller.Post(customerRequest);

        // Assert result should be ok and return the correct customer from database
        var viewResult = Assert.IsType<BadRequestObjectResult>(result);
        var error = Assert.IsAssignableFrom<SerializableError>(viewResult.Value);
        
        Assert.Single(error);
        Assert.True(error.ContainsKey("Email"));
        var errorMessage = error["Email"] as string[];
        Assert.NotNull(errorMessage);
        Assert.Equal("Email is required",errorMessage[0]);
    }
}