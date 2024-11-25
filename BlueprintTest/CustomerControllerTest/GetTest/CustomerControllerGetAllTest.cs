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

namespace BlueprintTest.CustomerControllerTest.GetTest;

public class CustomerControllerGetAllTest
{
    /// <summary>
    /// not found in cache but found in database
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetAll_Success()
    {
        var mockCache = new Mock<ICache>();
        var mockCustomerService = new Mock<ICustomerService>();
        var mockLogger = new Mock<ILogger<CustomerController>>();
        var mockMapper = new Mock<IMapper>();
        var mockDbConnection = new Mock<IConnection>();

        // not found in cache
        mockCache.Setup(x => x.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);
        var listCustomerFromDb = new List<Customer>
        {
            new()
            {
                Id = 1,
                Name = "John Doe",
                Address = "123 Main",
                Email2 = "john@gg.com",
                Phone = "123-456-7890",
                IsActive = true
            }, new()
            {
                Id = 2,
                Address = "New York",
                Phone = "334-456-7890",
                IsActive = true,
                Name = "Mary Jane",
            }};
            
        // found in database
        mockCustomerService.Setup(repo => repo.GetCustomersAsync())
            .ReturnsAsync(listCustomerFromDb);
            
        // assume mapper is working
        mockMapper.Setup(x => x.Map<List<CustomerResponse>>(listCustomerFromDb))
            .Returns([
                new()
                {
                    Id = listCustomerFromDb[0].Id,
                    Name =listCustomerFromDb[0].Name,
                    Address = listCustomerFromDb[0].Address,
                    Email = listCustomerFromDb[0].Email2,
                    Phone = listCustomerFromDb[0].Phone
                },
                new ()
                {
                    Id = listCustomerFromDb[1].Id,
                    Name = listCustomerFromDb[1].Name,
                    Address = listCustomerFromDb[1].Address,
                    Email = listCustomerFromDb[1].Email2,
                    Phone = listCustomerFromDb[1].Phone
                }
            ]);
            
        // init controller
        var controller =
            new CustomerController(mockCustomerService.Object, mockCache.Object, mockLogger.Object, mockDbConnection.Object,  mockMapper.Object);

        // Act
        var result = await controller.Get();

        // Assert result should be ok and return the correct customer from database
        var viewResult = Assert.IsType<OkObjectResult>(result);
        var lisResponseDto = Assert.IsAssignableFrom<List<CustomerResponse>>(viewResult.Value);

        Assert.Equal(2, lisResponseDto.Count);
        Assert.Equal(1, lisResponseDto[0].Id);
        Assert.Equal(2, lisResponseDto[1].Id);
    }
    
}