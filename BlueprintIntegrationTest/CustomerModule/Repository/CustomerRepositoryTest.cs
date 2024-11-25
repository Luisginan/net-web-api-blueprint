using System;
using System.Collections.Generic;
using System.Data;
using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Repository;
using Bogus;
using Core.Utils.DB;
using JetBrains.Annotations;
using Xunit;

namespace BlueprintIntegrationTest.CustomerModule.Repository;

[TestSubject(typeof(CustomerRepository))]
public class CustomerRepositoryTest : RepositoryBaseTest
{
    private readonly CustomerRepository _customerRepository;

    public CustomerRepositoryTest()
    {
        _customerRepository = new CustomerRepository(Repository, QueryBuilder);
    }


    [Fact]
    public void TestDeleteCustomer()
    {
        var customer = GenerateCustomer();
        InsertCustomerToDb(customer);
        var customerInDb = GetCustomerFromDb(customer);
        if (customerInDb == null)
            throw new NullReferenceException("CustomerInDb is null");
        //test
        _customerRepository.DeleteCustomer(customerInDb.Id);
        
        //assert
        var customerAfterDelete = GetCustomerFromDb(customerInDb);
        Assert.Null(customerAfterDelete);
    }

    private static Customer GenerateCustomer()
    {
        //preparation
        var faker = new Faker();
        var customer = new Customer
        {
            Email2 = faker.Person.Email,
            Name = faker.Person.FullName,
            Phone = faker.Person.Phone,
            Address = faker.Person.Address.Street,
            IsActive = true
        };
        return customer;
    }

    private void InsertCustomerToDb(Customer customer)
    {
        Repository.ExecuteNonQuery(
            "INSERT INTO customer (email, name, phone, address, is_active) VALUES (@Email2, @Name, @Phone, @Address, @IsActive)",
            new List<FieldParameter>
            {
                new("@Email2", customer.Email2),
                new("@Name", customer.Name),
                new("@Phone", customer.Phone),
                new("@Address", customer.Address),
                new("@IsActive", customer.IsActive)
            });
    }

    private Customer GetCustomerFromDb(Customer customer)
    {
        var customerInDb = Repository.ExecuteRow<Customer>("select * from customer where email=@email",
            fieldParameter: new[]
            {
                new FieldParameter("@email", DbType.String, customer.Email2)
            });
        return customerInDb;
    }

    [Fact]
    public void TestGetCustomer()
    {
        //preparation
        var customer = GenerateCustomer();
        InsertCustomerToDb(customer);
        var customerInDb = GetCustomerFromDb(customer);
        //test
        var result = _customerRepository.GetCustomer(customerInDb.Id);
        if (result == null)
            throw new NullReferenceException("result customer is null");

        //cleaning
        DeleteCustomerFromDb(result);
        //assert
        
        Assert.Equal(customer.Email2, result.Email2);
    }

    private void DeleteCustomerFromDb(Customer result)
    {
        Repository.ExecuteNonQuery("delete from customer where id=@id",
            fieldParameter: new[]
            {
                new FieldParameter("@id", DbType.Int32, result.Id)
            });
    }

    [Fact]
    public void TestSaveCustomer()
    {
        // preparation
        var customer = GenerateCustomer();
        // test
        _customerRepository.SaveCustomer(customer);
        var customerInDb = GetCustomerFromDb(customer);
        
        // cleaning
        DeleteCustomerFromDb(customerInDb);
        // assert
        Assert.NotNull(customerInDb);
        Assert.Equal(customer.Email2, customerInDb.Email2);
        
    }

    [Fact]
    public void TestUpdateCustomer()
    {
        var customer = GenerateCustomer();
        InsertCustomerToDb(customer);
        var customerInDb = GetCustomerFromDb(customer);
        if (customerInDb == null)
            throw new NullReferenceException("CustomerInDb is null");
        
        
        //test
        customerInDb.Email2 = "xxx.h.com";
        _customerRepository.UpdateCustomer(customerInDb, customerInDb.Id);
        var customerAfterUpdate = GetCustomerFromDb(customerInDb);
        if (customerAfterUpdate == null)
            throw new NullReferenceException("CustomerAfterUpdate is null");
        
        //cleaning
        DeleteCustomerFromDb(customerAfterUpdate);
        //assert
        Assert.Equal("xxx.h.com", customerAfterUpdate.Email2);
    }

    [Fact]
    public void TestGetCustomerByEmail()
    {
        // preparation
        var customer = GenerateCustomer();
        InsertCustomerToDb(customer);
        // test
        var result = _customerRepository.GetCustomerByEmail(customer.Email2);
        
        // cleaning
        DeleteCustomerFromDb(result);
        // assert
        Assert.NotNull(result);
        Assert.Equal(customer.Email2, result.Email2);
    }

    [Fact]
    public void TestGetCustomerByNameAndPhone()
    {
        // preparation
        var customer = GenerateCustomer();
        InsertCustomerToDb(customer);
        // test
        var result = _customerRepository.GetCustomerByNameAndPhone(customer.Name, customer.Phone);
        if (result == null)
            throw new NullReferenceException("result is null");
        // cleaning
        DeleteCustomerFromDb(result);
        // assert
        Assert.Equal(customer.Name, result.Name);
        Assert.Equal(customer.Phone, result.Phone);
    }

    [Fact]
    public void TestGetCustomers()
    {
        // preparation
        var customers = new List<Customer>
        {
            GenerateCustomer(),
            GenerateCustomer(),
            GenerateCustomer()
        };
        foreach (var customer in customers) InsertCustomerToDb(customer);
        
        var result = _customerRepository.GetListCustomer();
        Assert.Equal(customers.Count, result.Count);
    }
   
}