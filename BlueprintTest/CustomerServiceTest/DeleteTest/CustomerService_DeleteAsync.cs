using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Moq;
using Blueprint.CustomerModule.Validators;
using Core.CExceptions;

namespace BlueprintTest.CustomerServiceTest.DeleteTest;

public class CustomerServiceDeleteAsync
{
    [Fact]
    public async Task DeleteCustomerAsync_Success()
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
        await customerService.DeleteCustomerAsync(1);

        customerDal.Verify(x => x.GetCustomer(1), Times.Once);
        customerDal.Verify(x => x.DeleteCustomer(1), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomerAsync_NotFound()
    {
        var customerDal = new Mock<ICustomerRepository>();
        var customerRule = new Mock<ICustomerRuler>();
        customerDal.Setup(x => x.GetCustomer(It.IsAny<int>())).Returns(null as Customer);

        var customerService = new CustomerService(customerDal.Object, customerRule.Object);
        await Assert.ThrowsAsync<DataNotFoundServiceException>(() => customerService.DeleteCustomerAsync(1));
        customerDal.Verify(x => x.DeleteCustomer(1), Times.Never);
    }

}