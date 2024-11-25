using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Blueprint.CustomerModule.Validators;
using Moq;

namespace BlueprintTest.CustomerServiceTest.GetTest;

public class CustomerServiceGetByEmailTest
{
    [Fact]
    public void GetCustomerByEmail_NonNull()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomerByEmail(It.IsAny<string>()))
            .Returns(new Customer
            {
                Id = 1,
                Name = "John Doe",
                Address = "123 Main",
                Email2 = "myemail@g.com",
                Phone = "123-456-7890"
            });

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customer = customerService.GetCustomerByEmail("myemail@g.com");

        Assert.NotNull(customer);
        Assert.Equal(1, customer.Id);
        Assert.Equal("John Doe", customer.Name);
        Assert.Equal("123 Main", customer.Address);
        Assert.Equal("myemail@g.com", customer.Email2);
        Assert.Equal("123-456-7890", customer.Phone);
        customerDal.Verify(x => x.GetCustomerByEmail("myemail@g.com"), Times.Once);
    }

    [Fact]
    public void GetCustomerByEmail_Null()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomerByEmail(It.IsAny<string>()))
            .Returns(null as Customer);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customer = customerService.GetCustomerByEmail("myemail@xxx.com");

        Assert.Null(customer);
        customerDal.Verify(x => x.GetCustomerByEmail("myemail@xxx.com"), Times.Once);
    }
}