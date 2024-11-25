using Blueprint.CustomerModule.Models;

namespace Blueprint.CustomerModule.Repository;

public interface ICustomerRepository
{
    Customer? GetCustomer(int id);
    void SaveCustomer(Customer customer);

    void UpdateCustomer(Customer customer, int key);
    void DeleteCustomer(int id);
    List<Customer> GetListCustomer();
    Customer? GetCustomerByEmail(string email);
    Customer? GetCustomerByNameAndPhone(string name, string phone);
}