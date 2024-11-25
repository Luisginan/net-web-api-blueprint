using System.Data;
using System.Diagnostics.CodeAnalysis;
using NawaDataDAL.Common;

namespace Core.Utils.DB;
[ExcludeFromCodeCoverage]
public class FieldParameter
{
    private readonly QueryParameter _queryParameter;
    public FieldParameter(string parameterName, object value)
    {
        _queryParameter = new QueryParameter(parameterName, value);
    }

    public FieldParameter(string parameterName, DbType dbType, object? value)
    {
        _queryParameter = new QueryParameter(parameterName, dbType) { Value = value };
    }
    
    public QueryParameter GetQueryParameter()
    {
        return _queryParameter;
    }
}