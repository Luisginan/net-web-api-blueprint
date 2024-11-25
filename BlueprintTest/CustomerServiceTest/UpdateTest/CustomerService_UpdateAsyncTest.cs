using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Moq;
using Blueprint.CustomerModule.Validators;
using Core.CExceptions;

namespace BlueprintTest.CustomerServiceTest.UpdateTest;

public class CustomerServiceUpdateAsyncTest
{
    [Fact]
    public async Task UpdateCustomerAsync_Success()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
            
        var customer = new Customer
        {
            Id = 1,
            Name = "Jane Doe",
            Address = "456 Main",
            Email2 = "myemail@g.com",
            Phone = "123-456-7891"
        };
            
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>()))
            .Returns(customer);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        await customerService.UpdateCustomerAsync(customer, customer.Id);
            
        customerDal.Verify(x => x.GetCustomer(customer.Id), Times.Once);
        customerDal.Verify(x => x.UpdateCustomer(customer, customer.Id), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomerAsync_NotFound()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>()))
            .Returns((Customer?)null);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customer = new Customer
        {
            Id = 1,
            Name = "Jane Doe",
            Address = "456 Main",
            Email2 = "Los Angeles",
            Phone = "123-456-7891"
        };

        await Assert.ThrowsAsync<DataNotFoundServiceException>(() => customerService.UpdateCustomerAsync(customer, 1));
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
        customerDal.Verify(x => x.UpdateCustomer(customer, 1), Times.Never);
    }
}