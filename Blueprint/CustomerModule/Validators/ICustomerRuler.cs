using Blueprint.CustomerModule.Models;

namespace Blueprint.CustomerModule.Validators;

public interface ICustomerRuler
{
    List<string> Validate(Customer customer);
}