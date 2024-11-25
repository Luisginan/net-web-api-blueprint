using Blueprint.CustomerModule.Models;

namespace Blueprint.CustomerModule.Services;

public interface ICustomerService
{
    Customer? GetCustomer(int id);
    Task<Customer?> GetCustomerAsync(int id);
    void UpdateCustomer(Customer customer, int key);
    Task UpdateCustomerAsync(Customer customer, int key);
    void DeleteCustomer(int id);
    Task DeleteCustomerAsync(int id);
    List<Customer> GetListCustomer();
    Task<List<Customer>> GetCustomersAsync();
    void InsertCustomer(Customer customer);
    Task InsertCustomerAsync(Customer customer);
    Customer? GetCustomerByEmail(string email);
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Customer? GetCustomerByNameAndPhone(string name, string phone);
    Task<Customer?> GetCustomerByNameAndPhoneAsync(string name, string phone);
    List<string> Validate(Customer customer);
}