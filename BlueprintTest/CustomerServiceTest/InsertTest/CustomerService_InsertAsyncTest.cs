using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Blueprint.CustomerModule.Validators;
using Core.CExceptions;
using Moq;

namespace BlueprintTest.CustomerServiceTest.InsertTest;

public class CustomerServiceInsertAsyncTest
{
    [Fact]
    public async Task insertCustomerAsync_Success()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomerByEmail(It.IsAny<string>()))
            .Returns((Customer?)null);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);

        var customer = new Customer
        {
            Name = "John Doe",
            Address = "123 Main",
            Email2 = "New York",
            Phone = "123-456-7890"
        };

        await customerService.InsertCustomerAsync(customer);
        customerDal.Verify(x=> x.SaveCustomer(customer), Times.Once);
    }

    [Fact]
    public async Task insertCustomerAsync_AlreadyExists()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomerByEmail(It.IsAny<string>())).Returns(new Customer
        {
            Id = 1,
            Name = "John Doe",
            Address = "123 Main",
            Email2 = "myemail@g.com",
            Phone = "123-456-7890"
        });

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customer = new Customer
        {
            Name = "John Doe",
            Address = "123 Main",
            Email2 = "myemail@g.com",
            Phone = "123-456-7890"
        };

        await Assert.ThrowsAsync<DataIsExistServiceException>(() => customerService.InsertCustomerAsync(customer));
        customerDal.Verify(x => x.GetCustomerByEmail("myemail@g.com"), Times.Once);
        customerDal.Verify(x => x.SaveCustomer(customer), Times.Never);
    }
}