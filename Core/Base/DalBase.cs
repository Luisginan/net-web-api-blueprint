using System.Diagnostics.CodeAnalysis;
using Core.Utils.DB;

namespace Core.Base;

[ExcludeFromCodeCoverage]
public class DalBase<T> where T : class, new()
{
    protected readonly INawaDaoRepository NawaDao;
    protected readonly IQueryBuilderRepository QueryBuilder;

    protected DalBase(INawaDaoRepository nawaDaoRepository,
        IQueryBuilderRepository queryBuilderRepository)
    {
        NawaDao = nawaDaoRepository;
        QueryBuilder = queryBuilderRepository;
    }

    protected void Insert(T model)
    {
        NawaDao.Insert(model);
    }
    protected void Delete(int id)
    {
        NawaDao.Delete<T>(id);
    }

    protected T? Get(int id)
    {
        return NawaDao.Get<T>(id);
    }

    protected void Update(T model, int key)
    {
        NawaDao.Update(model, key);
    }
}