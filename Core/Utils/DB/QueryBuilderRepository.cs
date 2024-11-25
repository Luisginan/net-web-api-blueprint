using System.Diagnostics.CodeAnalysis;
using NawaDataDAL.Common;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class QueryBuilderRepository : IQueryBuilderRepository
{
    public void Register(NawaDAO.DBType dbType, string path)
    {
        QueryBuilder.Register(dbType, path);
    }

    public string GetQuery(string key)
    {
        return QueryBuilder.GetQuery(key);
    }

    public string GetQuery(string key, string connectionKey)
    {
        return QueryBuilder.GetQuery(key, connectionKey);
    }

    public string GetQuery(string key, NawaDAO.DBType dbType)
    {
        return QueryBuilder.GetQuery(key, dbType);
    }
}