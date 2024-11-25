using System.Diagnostics.CodeAnalysis;
using NawaDataDAL.Common;

namespace Core.Utils.DB;
[ExcludeFromCodeCoverage]
public static class DbQuery
{
    public static void SetQueryLocation(this WebApplicationBuilder builder, string location)
    {
        new Query().SetQueryLocation(location);
    }
}
[ExcludeFromCodeCoverage]
public class Query
{
    public void SetQueryLocation(string location)
    {
        QueryBuilder.Register(NawaDAO.DBType.PostgreSQL, location);
    }
}