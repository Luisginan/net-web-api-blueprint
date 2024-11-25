using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Models;
using Newtonsoft.Json;
using NJsonSchema;

namespace Blueprint.CustomerModule.Validators;
[ExcludeFromCodeCoverage]
public class CustomerRuler : ICustomerRuler
{
    public List<string> Validate(Customer customer)
    {
        //get json from file
        var schema = JsonSchema.FromFileAsync("CustomerModule/Validators/Customer.json").Result;
        var dataJson = JsonConvert.SerializeObject(customer);
        var errors = schema.Validate(dataJson).ToList();
        
        //return error to list<string>
        return errors.Select(x => x.ToString()).ToList();
    }
}