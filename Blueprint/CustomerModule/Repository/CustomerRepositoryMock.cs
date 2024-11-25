using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Models;
using Bogus;
using Core.CExceptions;

namespace Blueprint.CustomerModule.Repository;

[ExcludeFromCodeCoverage]
public class CustomerRepositoryMock : ICustomerRepository
{
    private readonly List<Customer> _customers;

    public CustomerRepositoryMock()
    {
        var faker = new Faker<Customer>()
            .RuleFor(c => c.Id, f => f.IndexFaker)
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.Address, f => f.Address.FullAddress())
            .RuleFor(c => c.Email2, f => f.Internet.Email())
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber());
        _customers = faker.Generate(100);
    }

    public Customer? GetCustomer(int id)
    {
        return (_customers ?? throw new InvalidOperationException()).Find(c => c.Id == id);
    }

    public void UpdateCustomer(Customer? customer, int key)
    {
        Debug.Assert(_customers != null, nameof(_customers) + " != null");
        var existingCustomer = _customers.Find(c => c.Id == key);
        if (existingCustomer == null) return;

        if (customer == null) return;
        existingCustomer.Name = customer.Name;
        existingCustomer.Address = customer.Address;
        existingCustomer.Email2 = customer.Email2;
        existingCustomer.Phone = customer.Phone;
    }

    public void DeleteCustomer(int id)
    {
        var existingCustomer = (_customers ?? throw new InvalidOperationException())
            .Find(c => c.Id == id);
        if (existingCustomer != null)
            _customers.Remove(existingCustomer);
        else
            throw new RepositoryException("Customer not found");
    }

    public List<Customer> GetListCustomer()
    {
        return _customers;
    }

    public Customer? GetCustomerByEmail(string email)
    {
        return _customers.Find(c => c.Email2 == email);
    }

    public Customer? GetCustomerByNameAndPhone(string name, string phone)
    {
        return _customers.Find(c => c.Name == name && c.Phone == phone) ?? null;
    }

    public void SaveCustomer(Customer? customer)
    {
       ArgumentNullException.ThrowIfNull(customer);
        
        if (_customers.Exists(c => c.Email2 == customer.Email2))
        {
            throw new RepositoryDataIsExistException("email", customer.Email2, "Customer already exists");
        }
        _customers.Add(customer);
    }
}