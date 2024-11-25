using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Validators;
using Core.CExceptions;

namespace Blueprint.CustomerModule.Services;

[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
public class CustomerService(ICustomerRepository customerRepository, ICustomerRuler customerRuler) : ICustomerService
{
    public Customer? GetCustomer(int id)
    {
        return customerRepository.GetCustomer(id);
    }

    public void UpdateCustomer(Customer customer, int key)
    {
            
        var customerExisting = GetCustomer(key);
        if (customerExisting == null)
            throw new DataNotFoundServiceException("id", key.ToString(), "Customer not found");
        ValidateEmail(customer, key, customerExisting);
        ValidatePhone(customer, key, customerExisting);
        customerRepository.UpdateCustomer(customer, key);
    }

    private void ValidatePhone(Customer customer, int key, Customer customerExisting)
    {
        ArgumentNullException.ThrowIfNull(customer);
        if (customerExisting.Phone == customer.Phone) return;
        var customerByNameAndPhone = GetCustomerByNameAndPhone(customer.Name, customer.Phone);
        if (customerByNameAndPhone == null) return;
        if (customerByNameAndPhone.Id != key)
            throw new DataIsExistServiceException("Customer with same name and phone already exists");
    }

    private void ValidateEmail(Customer customer, int key, Customer customerExisting)
    {
        ArgumentNullException.ThrowIfNull(customer);
        if (customerExisting.Email2 == customer.Email2) return;
        var customerByEmail = GetCustomerByEmail(customer.Email2);
        if (customerByEmail == null) return;
        if (customerByEmail.Id != key)
            throw new DataIsExistServiceException("id", key.ToString(),"Email is not available");
    }

    public void DeleteCustomer(int id)
    {
        if (GetCustomer(id) == null)
            throw new DataNotFoundServiceException("id", id.ToString(), "Customer not found");
        customerRepository.DeleteCustomer(id);
    }

    public List<Customer> GetListCustomer()
    {
        return customerRepository.GetListCustomer();
    }

    public void InsertCustomer(Customer customer)
    {
        if (GetCustomerByEmail(customer.Email2) != null)
            throw new DataIsExistServiceException("email", customer.Email2, "Email already exists");

        customerRepository.SaveCustomer(customer);
    }

    public Customer? GetCustomerByEmail(string email)
    {
        return customerRepository.GetCustomerByEmail(email);
    }

    public Customer? GetCustomerByNameAndPhone(string name, string phone)
    {
        return customerRepository.GetCustomerByNameAndPhone(name, phone);
    }

    public Task<Customer?> GetCustomerAsync(int id)
    {
        return Task.FromResult(GetCustomer(id));
    }

    public Task UpdateCustomerAsync(Customer customer, int key)
    {
        return Task.Run(()=> UpdateCustomer(customer, key));
    }

    public Task DeleteCustomerAsync(int id)
    {
        return Task.Run(()=> DeleteCustomer(id));
    }

    public Task<List<Customer>> GetCustomersAsync()
    {
        return Task.FromResult(GetListCustomer());
    }

    public Task InsertCustomerAsync(Customer customer)
    {
        return Task.Run(()=> InsertCustomer(customer));
    }

    public Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        return Task.FromResult(GetCustomerByEmail(email));
    }

    public Task<Customer?> GetCustomerByNameAndPhoneAsync(string name, string phone)
    {
        return Task.FromResult(GetCustomerByNameAndPhone(name, phone));
    }

    public List<string> Validate(Customer customer)
    {
        return customerRuler.Validate(customer);
    }
}