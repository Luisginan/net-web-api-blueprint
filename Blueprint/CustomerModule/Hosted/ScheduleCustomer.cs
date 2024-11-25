using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Services;
using Core.Base;

namespace Blueprint.CustomerModule.Hosted;

[ExcludeFromCodeCoverage]
public class CustomerScheduler(ILogger<CustomerScheduler> logger,
    ILogger<ProducerBase> baseLogger, 
    IServiceProvider serviceProvider) :
    SchedulerBase<Customer>(baseLogger)
{
    
    protected override void OnPrepare()
    {
       logger.LogInformation("SchedulerCustomer Preparation..");
    }

    protected override List<Customer> GetData()
    {
        using var scope = serviceProvider.CreateScope();
        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        var result = customerService.GetListCustomer();
        logger.LogInformation("SchedulerCustomer GetData successful: {info}", result.Count);
        return result;
    }

    protected override void OnExecuteRequest(Customer item)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

            item.IsActive = true;
            customerService.UpdateCustomer(item, item.Id);
            logger.LogInformation("SchedulerCustomer OnExecuteRequest successful: {info}", "Customer updated");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Data Customer with id: {item.Id} error. : {ex.Message}");
        }
    }

    protected override int Delay()
    {
        return 10000;
    }
}