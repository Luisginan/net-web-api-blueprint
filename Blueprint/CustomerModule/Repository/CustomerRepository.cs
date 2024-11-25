using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Models;
using Core.Base;
using Core.CExceptions;
using Core.Utils.DB;

namespace Blueprint.CustomerModule.Repository;

[ExcludeFromCodeCoverage]
public class CustomerRepository(INawaDaoRepository nawaDaoRepository, IQueryBuilderRepository queryBuilderRepository)
    : DalBase<Customer>(nawaDaoRepository, queryBuilderRepository ), ICustomerRepository
{
    public void DeleteCustomer(int id)
    {
        Delete(id);
    }

    public Customer? GetCustomer(int id)
    {
        return Get(id);
    }

    public void SaveCustomer(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullRepositoryException(nameof(customer), "Customer is null");
            
        Insert(customer);
    }

    public void UpdateCustomer(Customer customer, int key)
    {
        if (customer == null)
            throw new ArgumentNullRepositoryException(nameof(customer),"Customer is null");
        Update(customer, key);
    }

    public Customer? GetCustomerByEmail(string email)
    {
        var customer = NawaDao.ExecuteRow<Customer>(QueryBuilder.GetQuery("blueprint.getCustomerByEmail"), 
            [new FieldParameter("@email", email)]);
        return customer;
    }

    public Customer? GetCustomerByNameAndPhone(string name, string phone)
    {
        var customer = NawaDao.ExecuteRow<Customer>(QueryBuilder.GetQuery("blueprint.getCustomerByNameAndPhone"), 
        [ new FieldParameter("@name", name), 
            new FieldParameter("@phone", phone) ]);
        return customer;
    }

    public List<Customer> GetListCustomer()
    {
        return NawaDao.ExecuteTable<Customer>(QueryBuilder.GetQuery("blueprint.getCustomers"), new List<FieldParameter>());
    }
}