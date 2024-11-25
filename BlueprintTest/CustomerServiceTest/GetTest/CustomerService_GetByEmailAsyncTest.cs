using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Blueprint.CustomerModule.Validators;
using Moq;

namespace BlueprintTest.CustomerServiceTest.GetTest;

public class CustomerServiceGetByEmailAsyncTest
{

    [Fact]
    public async Task GetCustomerByEmailAsync_NonNull()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomerByEmail(It.IsAny<string>())).Returns(new Customer
        {
            Id = 1,
            Name = "John Doe",
            Address = "123 Main",
            Email2 = "myemail@xxx.com",
            Phone = "123-456-7890"
        });

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customer = await customerService.GetCustomerByEmailAsync("myemail@xxx.com");

        customerDal.Verify(x => x.GetCustomerByEmail("myemail@xxx.com"), Times.Once);
        Assert.NotNull(customer);
        Assert.Equal(1, customer.Id);
        Assert.Equal("John Doe", customer.Name);
        Assert.Equal("123 Main", customer.Address);
        Assert.Equal("myemail@xxx.com", customer.Email2);
        Assert.Equal("123-456-7890", customer.Phone);
    }

    [Fact]
    public async Task GetCustomerByEmailAsync_Null()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomerByEmail(It.IsAny<string>()))
            .Returns(null as Customer);

        var customerService = new CustomerService(customerDal.Object,customerRule.Object);
        var customer = await customerService.GetCustomerByEmailAsync("myemail@xxx.com");
            
        Assert.Null(customer);
        customerDal.Verify(x => x.GetCustomerByEmail("myemail@xxx.com"), Times.Once);
    }
}