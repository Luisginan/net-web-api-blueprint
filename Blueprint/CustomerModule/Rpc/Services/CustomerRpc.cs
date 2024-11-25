using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Services;
using Grpc.Core;

namespace Blueprint.CustomerModule.Rpc.Services;
[ExcludeFromCodeCoverage]
public class CustomerRpc(ICustomerService service) : CustomerService.CustomerServiceBase
{
    public override Task<CustomerResponse> GetCustomer(CustomerRequest request, ServerCallContext context)
    {
        var customer = service.GetCustomer(request.Id);
        if (customer == null)
        {
            return Task.FromResult(new CustomerResponse {IsNull = true});
        }
        return Task.FromResult(new CustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email2
        });
    }
}