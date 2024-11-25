using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Moq;
using Blueprint.CustomerModule.Validators;
using Core.CExceptions;

namespace BlueprintTest.CustomerServiceTest.InsertTest;

public class CustomerServiceInsertTest
{
    [Fact]
    public void SaveCustomer_Success()
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
            Email2 = "myemail@g.com",
            Phone = "123-456-7890"
        };

        customerService.InsertCustomer(customer);
        customerDal.Verify(x => x.GetCustomerByEmail(customer.Email2), Times.Once);
        customerDal.Verify(x => x.SaveCustomer(customer), Times.Once);
    }

    [Fact]
    public void SaveCustomer_AlreadyExists()
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
        var customer = new Customer
        {
            Name = "John Doe",
            Address = "123 Main",
            Email2 = "New York",
            Phone = "123-456-7890"
        };

        Assert.Throws<DataIsExistServiceException>(() => customerService.InsertCustomer(customer));
        customerDal.Verify(x => x.GetCustomerByEmail(customer.Email2), Times.Once);
        customerDal.Verify(x => x.SaveCustomer(customer), Times.Never);
    }
}