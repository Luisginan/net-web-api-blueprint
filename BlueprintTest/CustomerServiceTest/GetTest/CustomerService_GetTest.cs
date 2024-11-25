using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Blueprint.CustomerModule.Validators;
using Moq;

namespace BlueprintTest.CustomerServiceTest.GetTest;

public class CustomerServiceGetTest
{
    [Fact]
    public void GetCustomer_NonNull()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>()))
            .Returns(new Customer
            {
                Id = 1,
                Name = "John Doe",
                Address = "123 Main",
                Email2 = "myemail@g.com",
                Phone = "123-456-7890"
            });

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customer = customerService.GetCustomer(1);

        Assert.NotNull(customer);
        Assert.Equal(1, customer.Id);
        Assert.Equal("John Doe", customer.Name);
        Assert.Equal("123 Main", customer.Address);
        Assert.Equal("myemail@g.com", customer.Email2);
        Assert.Equal("123-456-7890", customer.Phone);
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
    }

    [Fact]
    public void GetCustomer_Null()
    {
        var customerDal = new Mock<ICustomerRepository>();
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>()))
            .Returns((Customer?) null);

        var customerService = new CustomerService(customerDal.Object, new CustomerRuler());
        var customer = customerService.GetCustomer(1);

        Assert.Null(customer);
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
    }

}