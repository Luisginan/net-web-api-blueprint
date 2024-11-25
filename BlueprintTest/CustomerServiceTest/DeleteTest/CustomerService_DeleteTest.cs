using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Moq;
using Blueprint.CustomerModule.Validators;
using Core.CExceptions;

namespace BlueprintTest.CustomerServiceTest.DeleteTest;

public class CustomerServiceDeleteTest
{
    [Fact]
    public void DeleteCustomer_Success()
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
        customerService.DeleteCustomer(1);
           
        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
        customerDal.Verify(x => x.DeleteCustomer(1), Times.Once);
    }

    [Fact]
    public void DeleteCustomer_NotFound()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>())).Returns(null as Customer);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        Assert.Throws<DataNotFoundServiceException>(() => customerService.DeleteCustomer(1));
        customerDal.Verify(x => x.DeleteCustomer(It.IsAny<int>()), Times.Never);
        customerDal.Verify(x => x.GetCustomer(It.IsAny<int>()), Times.Once); 
    }
}