using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Moq;
using Blueprint.CustomerModule.Validators;
using Core.CExceptions;

namespace BlueprintTest.CustomerServiceTest.UpdateTest;

public class CustomerServiceUpdateTest
{
    [Fact]
    public void UpdateCustomer_Success()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>()))
            .Returns(new Customer
            {
                Id = 1,
                Name = "John Doe",
                Address = "123 Main",
                Email2 = "NewYork@gg.com",
                Phone = "123-456-7890"
            });
            
        var newCustomer = new Customer
        {
            Id = 1,
            Name = "Jane Doe",
            Address = "456 Main",
            Email2 = "LosAngeles@gg.com",
            Phone = "123-456-888"
        };

            
        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        customerService.UpdateCustomer(newCustomer, 1);
            
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
        customerDal.Verify(x => x.UpdateCustomer(newCustomer, 1), Times.Once);
    }

    [Fact]
    public void UpdateCustomer_SameEmailSuccess()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        var newCustomer = new Customer
        {
            Id = 1,
            Name = "Jane Doe",
            Address = "456 Main",
            Email2 = "myemail@g.com"
        };
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>()))
            .Returns(newCustomer);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        customerService.UpdateCustomer(newCustomer, 1);
            
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
        customerDal.Verify(x => x.UpdateCustomer(newCustomer, 1), Times.Once);
    }

    [Fact]
    public void UpdateCustomer_SamePhoneSuccess()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
            
        var newCustomer = new Customer
        {
            Id = 1,
            Name = "Jane Doe",
            Address = "456 Main",
            Phone = "123-456-7890",
            Email2 = "myemail@g.com"
        };
            
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>()))
            .Returns(newCustomer);
            
        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        customerService.UpdateCustomer(newCustomer, 1);
            
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
        customerDal.Verify(x => x.UpdateCustomer(newCustomer, 1), Times.Once);
    }

    [Fact]
    public void UpdateCustomer_NotFound()
    {
        var customerDal = new Mock<ICustomerRepository>();
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>()))
            .Returns((Customer?)null);

        var customerService = new CustomerService(customerDal.Object, new CustomerRuler());
        var customer = new Customer
        {
            Id = 1,
            Name = "Jane Doe",
            Address = "456 Main",
            Email2 = "Los Angeles",
            Phone = "123-456-7891"
        };

        Assert.Throws<DataNotFoundServiceException>(() => customerService.UpdateCustomer(customer, 1));
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
        customerDal.Verify(x => x.UpdateCustomer(customer, 1), Times.Never);
    }

    [Fact]
    public void UpdateCustomer_EmailAlreadyExist()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();

        var customerExisting = new Customer
        {
            Id = 1,
            Name = "Jane Doe",
            Address = "456 Main",
            Email2 = "myemail@g.com"
        };

        var otherCustomer = new Customer
        {
            Id = 2,
            Name = "Janet",
            Address = "4567 Main",
            Email2 = "222@g.com"
        };
            
        var newCustomer = new Customer
        {
            Id = 1,
            Name = "Jane Doe",
            Address = "456 Main",
            Email2 = "222@g.com"
        };
            
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>()))
            .Returns(customerExisting);
        customerDal.Setup(x => x.GetCustomerByEmail(It.IsAny<string>()))
            .Returns(otherCustomer);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);

        var message = Assert.Throws<DataIsExistServiceException>(() => customerService.UpdateCustomer(newCustomer, 1)).Message;
        Assert.Equal("Email is not available [key: id, value: 1]", message);
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
        customerDal.Verify(x => x.GetCustomerByEmail(newCustomer.Email2), Times.Once);
    }

    [Fact]
    public void UpdateCustomer_PhoneAlreadyExist()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
            
        var customerExisting = new Customer
        {
            Id = 1,
            Name = "Jane Doe",
            Address = "456 Main",
            Phone = "123-456-7890",
            Email2 = "myemail@g.com"
        };

        var otherCustomer = new Customer
        {
            Id = 2,
            Name = "Janet",
            Address = "4567 Main",
            Phone = "123-456-7891",
            Email2 = "los@hh.com"
        };

        var newCustomer = new Customer
        {
            Id = 1,
            Name = "Jane Doe Magnan",
            Address = "456 Main",
            Phone = "123-456-7891",
            Email2 = "myemail@g.com"
        };
            
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>()))
            .Returns(customerExisting);
            
        customerDal.Setup(x => x.GetCustomerByNameAndPhone(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(otherCustomer);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        var message = Assert.Throws<DataIsExistServiceException>(() => customerService.UpdateCustomer(newCustomer, 1)).Message;
            
        Assert.Equal("Customer with same name and phone already exists", message);
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
        customerDal.Verify(x => x.GetCustomerByNameAndPhone(newCustomer.Name, newCustomer.Phone), Times.Once);
    }
}