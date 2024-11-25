using System;
using System.Data;
using Bogus;
using Core.Config;
using Core.Utils.DB;
using Core.Utils.Security;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CoreIntegrationTest.Utils.DB;

[TestSubject(typeof(NawaDaoRepository))]
public class NawaDaoRepositoryTest
{
    private readonly NawaDaoRepository _nawaDaoRepository;
    public NawaDaoRepositoryTest()
    {
        // setup code here
        var vault = new Mock<IVault>();
        var options = new Mock<IOptions<DatabaseConfig>>();
        
        vault.Setup(m => m.RevealSecret(It.IsAny<object>())).Returns(
            GetDatabaseConfig()
        );

        var connection = new Connection(vault.Object, options.Object);
        _nawaDaoRepository = new NawaDaoRepository(connection);
    }

    private static DatabaseConfig GetDatabaseConfig()
    {
        return new DatabaseConfig
        {
            Database = "blueprint",
            Password = "VCt/m8/zEfD5MN61wPTfrQ==",
            Port = "5432",
            Provider = "postgres",
            Server = "localhost",
            Type = "PostgreSQL",
            User = "postgres",
            CommandTimeout = "30",
            ConnectTimeout = "30",
            PoolSize = "100"
        };
    }

    [Fact]
    public void ExecuteScalar_InsertTest()
    {
        //testing
        var result = _nawaDaoRepository.ExecuteNonQuery("insert into customer (address, email, phone, name, is_active) VALUES (@address,@email,@phone,@name,@is_active)", new []
        {
             new FieldParameter("@address",DbType.String, "Pangandaran"),
             new FieldParameter("@email", DbType.String,"luis@g.com"),
             new FieldParameter("@phone", DbType.String, "0874545555"),
             new FieldParameter("@name",DbType.String,"Luis Ginanjar"),
             new FieldParameter("@is_active", DbType.Boolean, true)
        });

        CleaningCustomerByEmail();

        Assert.Equal(1,result);
    }

    [Fact]
    public void ExecuteScalar_UpdateTest()
    {
        var customer = CreateCustomerForPreparationData();
        InsertCustomerForPreparation(customer);
        var customerInDb = GetCustomerByEmail(customer);

        //do the update
        const string newAddress = "Updated Address";
        var result = _nawaDaoRepository.ExecuteNonQuery("update customer set address = @address where id = @id",
            new[]
            {
                new FieldParameter("@address", DbType.String, newAddress),
                new FieldParameter("@id", DbType.Int32, customerInDb.Id)
            });

        var updatedCustomerInDb = GetCustomerByEmail(customer);

        CleanDataCustomer(updatedCustomerInDb);

        //Assert
        Assert.Equal(1, result);
        Assert.Equal(newAddress, updatedCustomerInDb.Address);
    }

    /* Implement this function */
    [Fact]
    public void ExecuteScalar_DeleteTest()
    {
        // Arrange
        var customer = CreateCustomerForPreparationData();
        InsertCustomerForPreparation(customer);

        // Act
        var result = _nawaDaoRepository.ExecuteNonQuery("delete from customer where email=@email",
            new[] { new FieldParameter("@email", DbType.String, customer.Email2) });

        // Assert
        var customerInDb = GetCustomerByEmail(customer); // Fetching customer from DB after deletion
        Assert.Equal(1, result);
        Assert.Null(customerInDb); // If Successfully deleted, it should be null.
    }

    private void CleaningCustomerByEmail()
    {
        //cleaning
        _nawaDaoRepository.ExecuteNonQuery("delete from customer where email=@email", new[]
        {
            new FieldParameter("@email", "luis@g.com")
        });
    }

    [Fact]
    public void ExecuteRow_SingleCriteriaTest()
    {

        var customer = CreateCustomerForPreparationData();
        InsertCustomerForPreparation(customer:customer);
        
        // testing
        var customerFetched = _nawaDaoRepository.ExecuteRow<Customer>("select * from customer where email=@email",
            new []
            {
                new FieldParameter("@email",DbType.String,customer.Email2)
            });
        
       CleanDataCustomer(customerFetched);
        
        Assert.NotNull(customerFetched);
        Assert.Equal(customer.Email2, customerFetched.Email2);
    }

    [Fact]
    public void ExecuteRow_MultipleCriteria()
    {
        // Arrange
        var customer = CreateCustomerForPreparationData();
        InsertCustomerForPreparation(customer);

        // Act
        var customerFetched = _nawaDaoRepository.ExecuteRow<Customer>(
            "select * from customer where email=@email and phone=@phone",
            new[]
            {
                new FieldParameter("@email", DbType.String, customer.Email2),
                new FieldParameter("@phone", DbType.String, customer.Phone)
            });

        CleanDataCustomer(customerFetched);

        // Assert
        Assert.NotNull(customerFetched);
        Assert.Equal(customer.Email2, customerFetched.Email2);
        Assert.Equal(customer.Phone, customerFetched.Phone);
    }

    [Fact]
    public void InsertObjectTest()
    {
        var customer = CreateCustomerForPreparationData();

        //testing
        var result = _nawaDaoRepository.Insert(customer);
        
        var customerInDb = GetCustomerByEmail(customer);

        CleanDataCustomer(customerInDb);

        // assert
        Assert.Equal(1, result);
        Assert.NotNull(customerInDb);
        Assert.Equal(customer.Name, customerInDb.Name);
        Assert.Equal(customer.Address, customerInDb.Address);
        Assert.Equal(customer.Email2, customerInDb.Email2);
        Assert.Equal(customer.Phone, customerInDb.Phone);
        Assert.Equal(customer.IsActive, customerInDb.IsActive);
    }

    private Customer GetCustomerByEmail(Customer customer)
    {
        var customerInDb = _nawaDaoRepository.ExecuteRow<Customer>(
            "select * from customer where email=@email",
            new[]
            {
                new FieldParameter("@email", DbType.String, customer.Email2)
            });
        
        return customerInDb;
    }

    private void CleanDataCustomer(Customer customerInDb)
    {
        // cleaning
        _nawaDaoRepository.ExecuteNonQuery("delete from customer where id=@id",
            new[] { new FieldParameter("@id", DbType.Int32, customerInDb.Id) });
    }

    private static Customer CreateCustomerForPreparationData()
    {
        var faker = new Faker();
        var customer = new Customer
        {
            Name = faker.Name.FullName(),
            Address = faker.Address.FullAddress(),
            Email2 = Guid.NewGuid() + "@g.com",
            Phone = faker.Phone.PhoneNumber(),
            IsActive = faker.Random.Bool()
        };
        return customer;
    }

    [Fact]
    public void GetObjectTest()
    {
        var customer = CreateCustomerForPreparationData();
        InsertCustomerForPreparation(customer);
        var customerExist = GetCustomerExistForPreparation(customer);
        
        //testing
        var customerInDb = _nawaDaoRepository.Get<Customer>(customerExist.Id);
        
        CleanDataCustomer(customerInDb);
        
        //assert
        Assert.NotNull(customerInDb);
        Assert.Equal(customer.Email2, customerInDb.Email2);

    }

    private Customer GetCustomerExistForPreparation(Customer customer)
    {
        var customerExist = _nawaDaoRepository.ExecuteRow<Customer>(
            "select * from customer where email=@email", new[]
            {
                new FieldParameter("@email", DbType.String, customer.Email2)
            });

        if (customerExist == null) throw new NullReferenceException("CustomerExist is null");
        return customerExist;
    }

    private void InsertCustomerForPreparation(Customer customer)
    {
        // Insert the customer to the database
        _nawaDaoRepository.ExecuteNonQuery(
            "insert into customer (address, email, phone, name, is_active) VALUES (@address, @email, @phone, @name, @is_active)",
            new[]
            {
                new FieldParameter("@address", DbType.String, customer.Address),
                new FieldParameter("@email", DbType.String, customer.Email2),
                new FieldParameter("@phone", DbType.String, customer.Phone),
                new FieldParameter("@name", DbType.String, customer.Name),
                new FieldParameter("@is_active", DbType.Boolean, customer.IsActive)
            });
    }

    [Fact]
    public void UpdateObject()
    {
        var customer = CreateCustomerForPreparationData();
        InsertCustomerForPreparation(customer:customer);
        var customerFetched = GetCustomerExistForPreparation(customer: customer);

        // Modify the customer details
        customerFetched.Name = "New Name";

        // Perform update
        var result = _nawaDaoRepository.Update(customerFetched, customerFetched.Id);
        if (result != 1)
            throw new Exception("result should be 1");

        var updatedCustomer = GetCustomerByEmail(customerFetched);

        CleanDataCustomer(updatedCustomer);

        // Assert
        Assert.NotNull(updatedCustomer);
        Assert.Equal("New Name", updatedCustomer.Name);
    }

    [Fact]
    public void DeleteObjectTest()
    {
        var customer = CreateCustomerForPreparationData();
        InsertCustomerForPreparation(customer);
        var customerExist = GetCustomerExistForPreparation(customer);
        
        //testing
        var result = _nawaDaoRepository.Delete<Customer>(customerExist.Id);

        var customerDeleted = GetCustomerByEmail(customerExist);

        Assert.NotNull(customerExist);
        Assert.Equal(1, result);
        Assert.Null(customerDeleted);
    }

    [Fact]
    public void Transaction_SuccessTest()
    {
        var vault = new Mock<IVault>();
        vault.Setup(m => m.RevealSecret(It.IsAny<object>()))
            .Returns(GetDatabaseConfig);
        var option = new Mock<IOptions<DatabaseConfig>>();
        var connection = new Connection(vault.Object, option.Object);
        var dao = new NawaDaoRepository(connection);
        connection.BeginTransaction();
        for (var i = 0; i < 5; i++)
        {
            var faker = new Faker();
            var customer = new Customer
            {
                Email2 = i + "g.com",
                Address = "Pangandaran212",
                IsActive = true,
                Phone = faker.Phone.PhoneNumber(),
                Name = faker.Name.FullName()
            };

            dao.Insert(customer);
        }
        connection.Commit();
        
        //cleaning
        var count = dao.ExecuteTable<Customer>(
                "SELECT id FROM customer where address=@address", new []
            {
                new FieldParameter("@address", DbType.String, "Pangandaran212")
            })
            .Count;

        //cleaning
        dao.ExecuteNonQuery("delete from customer where address=@address", new[]
        {
            new FieldParameter("@address", DbType.String, "Pangandaran212")
        });
        
        
        Assert.Equal(5, count);
    }
    
    [Fact]
    public void Transaction_FailedRollbackTest()
    {
        var vault = new Mock<IVault>();
        vault.Setup(m => m.RevealSecret(It.IsAny<object>()))
            .Returns(GetDatabaseConfig);
        var option = new Mock<IOptions<DatabaseConfig>>();
        var connection = new Connection(vault.Object, option.Object);
        var dao = new NawaDaoRepository(connection);
        connection.BeginTransaction();
        try
        {
            for (var i = 0; i < 5; i++)
            {
                var faker = new Faker();
                var customer = new Customer
                {
                    Email2 = i + "g.com",
                    Address = "Pangandaran111",
                    IsActive = true,
                    Phone = faker.Phone.PhoneNumber(),
                    Name = faker.Name.FullName()
                };

                dao.Insert(customer);
                if (i == 4)
                    throw new Exception("Failed for rollback");
            }

            connection.Commit();
        }
        catch
        {
            connection.Rollback();
        }
        finally
        {
            connection.Dispose();
        }
        
        var count = _nawaDaoRepository.ExecuteTable<Customer>(
                "SELECT id FROM customer where address=@address", new []
                {
                    new FieldParameter("@address", DbType.String, "Pangandaran111")
                })
            .Count;

        //cleaning
        _nawaDaoRepository.ExecuteNonQuery("delete from customer where address=@address", new[]
        {
            new FieldParameter("@address", DbType.String, "Pangandaran111")
        });
        
        
        Assert.Equal(0, count);
    }
    
}