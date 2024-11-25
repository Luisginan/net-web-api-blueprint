using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Moq;
using Blueprint.CustomerModule.Validators;

namespace BlueprintTest.CustomerServiceTest.GetTest;

public class CustomerServiceGetAsyncTest
{
    [Fact]
    public async Task GetCustomersAsync_NonEmpty()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetListCustomer()).Returns([
            new Customer
            {
                Id = 1,
                Name = "John Doe",
                Address = "123 Main",
                Email2 = "New York",
                Phone = "123-456-7890"
            },

            new Customer
            {
                Id = 2,
                Name = "Jane Doe",
                Address = "456 Main",
                Email2 = "Los Angeles",
                Phone = "123-456-7891"
            }
        ]);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customers = await customerService.GetCustomersAsync();

        //assert
        customerDal.Verify(x => x.GetListCustomer(), Times.Once);
        Assert.NotNull(customers);
        Assert.Equal(2, customers.Count);
        Assert.Equal(1, customers[0].Id);
        Assert.Equal("John Doe", customers[0].Name);
        Assert.Equal("123 Main", customers[0].Address);
        Assert.Equal("New York", customers[0].Email2);
        Assert.Equal("123-456-7890", customers[0].Phone);
        Assert.Equal(2, customers[1].Id);
        Assert.Equal("Jane Doe", customers[1].Name);
        Assert.Equal("456 Main", customers[1].Address);
        Assert.Equal("Los Angeles", customers[1].Email2);
        Assert.Equal("123-456-7891", customers[1].Phone);
    }

    [Fact]
    public async Task GetCustomersAsync_Empty()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetListCustomer()).Returns([]);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customers = await customerService.GetCustomersAsync();

        Assert.NotNull(customers);
        Assert.Empty(customers);
        customerDal.Verify(x => x.GetListCustomer(), Times.Once);
    }

    [Fact]
    public async Task GetCustomersAsync_Null()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetListCustomer()).Returns([]);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customers = await customerService.GetCustomersAsync();

        Assert.Empty(customers);
        customerDal.Verify(x => x.GetListCustomer(), Times.Once);
    }

    [Fact]
    public async Task GetCustomerAsync_NonNull()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>())).Returns(new Customer
        {
            Id = 1,
            Name = "John Doe",
            Address = "123 Main",
            Email2 = "New York",
            Phone = "123-456-7890"
        });

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customer = await customerService.GetCustomerAsync(1);

        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
        Assert.NotNull(customer);
        Assert.Equal(1, customer.Id);
        Assert.Equal("John Doe", customer.Name);
        Assert.Equal("123 Main", customer.Address);
        Assert.Equal("New York", customer.Email2);
        Assert.Equal("123-456-7890", customer.Phone);
    }

    [Fact]
    public async Task GetCustomerAsync_Empty()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>())).Returns(null as Customer);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var customer = await customerService.GetCustomerAsync(1);

        Assert.Null(customer);
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
    }
}