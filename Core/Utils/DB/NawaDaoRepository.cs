using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Core.CExceptions;
using NawaDataDAL.Common;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class NawaDaoRepository : INawaDaoRepository, ILogDb
{
    private readonly NawaConnection _nawaCon;
    private readonly IConnection _connection;

    public NawaDaoRepository(IConnection connection)
    {
        _connection = connection;
        var c = _connection.GetConnection();
        _nawaCon = c as NawaConnection ?? throw new ConnectionDaoException("Connection is null");
    }

    public int ExecuteNonQuery(string cmdText, IEnumerable<FieldParameter>? fieldParameter = null )
    {
        var parameters = ConvertParameters(fieldParameter);
        return NawaDAO.ExecuteNonQuery(cmdText, parameters, _nawaCon);
    }

    private static QueryParameter[] ConvertParameters(IEnumerable<FieldParameter>? fieldParameter)
    {
        fieldParameter ??= new List<FieldParameter>();
        return fieldParameter.Select(x => x.GetQueryParameter()).ToArray();
    }

    public object ExecuteScalar(string cmdText, IEnumerable<FieldParameter>? fieldParameter = null)
    {
        var parameters = ConvertParameters(fieldParameter);
        return NawaDAO.ExecuteScalar(cmdText, parameters, _nawaCon);
    }

    public DataRow ExecuteRow(string cmdText, IEnumerable<FieldParameter>? fieldParameter = null)
    {
        return NawaDAO.ExecuteRow(cmdText, ConvertParameters(fieldParameter), _nawaCon);
    }

    public T? ExecuteRow<T>(string cmdText, IEnumerable<FieldParameter>? fieldParameter = null) where T : class, new()
    {
        var parameters = ConvertParameters(fieldParameter);
        return Execute<T>(cmdText, parameters);
    }


    private T? Execute<T>(string cmdText, QueryParameter[] queryParameter)
        where T : class, new()
    {
        var dataRow = NawaDAO.ExecuteRow(cmdText, queryParameter, _nawaCon);
        return ConvertDataRow<T>(dataRow);
    }

    public DataTable ExecuteTable(string cmdText, IEnumerable<FieldParameter>? fieldParameter = null)
    {
        var parameters = ConvertParameters(fieldParameter);
        return NawaDAO.ExecuteTable(cmdText, parameters, _nawaCon);
    }

    public List<T> ExecuteTable<T>(string cmdText, IEnumerable<FieldParameter>? fieldParameter = null)
        where T : class, new()
    {
        var parameters = ConvertParameters(fieldParameter);
        var dataTable = NawaDAO.ExecuteTable(cmdText, parameters, _nawaCon);
        List<T> results = [];
        results.AddRange(from DataRow row in dataTable.Rows select ConvertDataRow<T>(row));
        return results;
    }

    public int ExecuteNonQueryWithBuilder(string queryKey, IEnumerable<FieldParameter>? fieldParameter = null)
    {
        var parameters = ConvertParameters(fieldParameter);
        var query = QueryBuilder.GetQuery(queryKey);
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentNullException(nameof(queryKey), $"query {queryKey} not found");
        return NawaDAO.ExecuteNonQuery(query, parameters, _nawaCon);
    }


    public object ExecuteScalarWithBuilder(string queryKey, IEnumerable<FieldParameter>? fieldParameter = null)
    {
        var parameters = ConvertParameters(fieldParameter);
        var query = QueryBuilder.GetQuery(queryKey);
        if (string.IsNullOrWhiteSpace(query))
            throw new QueryNotFoundDaoException(nameof(queryKey), $"query {queryKey} not found");
        return NawaDAO.ExecuteScalar(query, parameters, _nawaCon);
    }

    public DataRow ExecuteRowWithBuilder(string queryKey, IEnumerable<FieldParameter>? fieldParameter = null)
    {
        var parameters = ConvertParameters(fieldParameter);
        var query = QueryBuilder.GetQuery(queryKey);
        if (string.IsNullOrWhiteSpace(query))
            throw new QueryNotFoundDaoException(nameof(queryKey), $"query {queryKey} not found");
        return NawaDAO.ExecuteRow(query, parameters, _nawaCon);
    }

    public T? ExecuteRowWithBuilder<T>(string queryKey, IEnumerable<FieldParameter>? fieldParameter = null)
        where T : class, new()
    {
        var parameters = ConvertParameters(fieldParameter);
        var query = QueryBuilder.GetQuery(queryKey);
        if (string.IsNullOrWhiteSpace(query))
            throw new QueryNotFoundDaoException(nameof(queryKey), $"query {queryKey} not found");
        var dataRow = NawaDAO.ExecuteRow(query, parameters, _nawaCon);
        return ConvertDataRow<T>(dataRow);
    }

    public DataTable ExecuteTableWithBuilder(string queryKey, IEnumerable<FieldParameter>? fieldParameter = null)
    {
        var parameters = ConvertParameters(fieldParameter);
        var query = QueryBuilder.GetQuery(queryKey);
        if (string.IsNullOrWhiteSpace(query))
            throw new QueryNotFoundDaoException(nameof(queryKey), $"query {queryKey} not found");
        return NawaDAO.ExecuteTable(query, parameters, _nawaCon);
    }

    public List<T> ExecuteTableWithBuilder<T>(string queryKey, IEnumerable<FieldParameter>? fieldParameter = null)
        where T : class, new()
    {
        var parameters = ConvertParameters(fieldParameter);
        var query = QueryBuilder.GetQuery(queryKey);
        if (string.IsNullOrWhiteSpace(query))
            throw new QueryNotFoundDaoException(nameof(queryKey), $"query {queryKey} not found");
        var dataTable = NawaDAO.ExecuteTable(query, parameters, _nawaCon);
        List<T> results = [];
        results.AddRange(from DataRow row in dataTable.Rows select ConvertDataRow<T>(row));
        return results;
    }

    private static PropertyInfo? GetPropertyByAttributeName<T>(string name) where T : class
    {
        var type = typeof(T);
        var properties = type.GetProperties();
        PropertyInfo? result = null;
        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<FieldAttribute>();

            if (attribute == null) continue;
            if (attribute.Name != name) continue;
                
            result = property;
            break;
        }

        return result;
    }

    private static T? ConvertDataRow<T>(DataRow? row) where T : class, new()
    {
        if (row == null) return default;
            
        T obj = new();

        foreach (DataColumn column in row.Table.Columns)
        {
            var propertyName = column.ColumnName;

            var propertyInfo = GetPropertyByAttributeName<T>(propertyName);
            if (propertyInfo == null)
            {
                propertyInfo = typeof(T).GetProperty(propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            }


            if (propertyInfo == null || row[column] == DBNull.Value) continue;
            
            var targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
            var value = Convert.ChangeType(row[column], targetType);
            propertyInfo.SetValue(obj, value);
        }

        return obj;
    }

    public IConnection GetConnection()
    {
        return _connection;
    }

    public T? Get<T>(int key) where T : class, new()
    {
        var query = GenerateQuerySelect<T>();
        var parameters = GetSelectParameters<T>(key);
        return Execute<T>(query, parameters);
    }

    private static QueryParameter[] GetSelectParameters<T>(int key) where T : class, new()
    {
        var primaryKey = typeof(T).GetCustomAttribute<TableAttribute>()?.PrimaryKey;
        var queryParameters = new QueryParameter[1];
        queryParameters[0] = new QueryParameter("@" + primaryKey, key);
        return queryParameters;
    }

    private static string GenerateQuerySelect<T>() where T : class, new()
    {
        var attribute = typeof(T).GetCustomAttribute<TableAttribute>();
        var tableName = attribute?.Name;
        var primaryKey = attribute?.PrimaryKey;
        tableName ??= typeof(T).Name;
        var query = "SELECT * FROM " + tableName + " Where " + primaryKey + "=@" + primaryKey;
        return query;
    }

    public int Insert<T>(T model) where T : class, new()
    {
        var query = GenerateInsertQuery(model);
        var parameters = GetInsertParameters(model);
        return NawaDAO.ExecuteNonQuery(query, parameters, _nawaCon);
    }

    public int Update<T>(T model, int key) where T : class, new()
    {
        var query = GenerateUpdateQuery(model);
        var parameters = GenerateUpdateParameter(model, key);
        return NawaDAO.ExecuteNonQuery(query, parameters, _nawaCon);
    }

    private static QueryParameter[] GenerateUpdateParameter<T>(T model, int key)
    {
        var properties = model?.GetType().GetProperties();
        var primaryKey = model?.GetType().GetCustomAttribute<TableAttribute>()?.PrimaryKey;

        if (properties == null)
            throw new PropertyGeneratedIsNullDaoException(nameof(properties), "properties is null");
            
        var queryParameters = GeQueryParametersForUpdate(model, key, properties, primaryKey);

        return queryParameters;
    }

    private static QueryParameter[] GeQueryParametersForUpdate<T>(T model, int key, PropertyInfo[] properties,
        string? primaryKey)
    {
        var queryParameters = new QueryParameter[properties.Length];
        var i = 0;
        foreach (var property in properties)
        {
            if (property == null)
                throw new PropertyGeneratedIsNullDaoException(nameof(properties), "properties is null");
                
            if (property.GetCustomAttribute<FieldAttribute>() == null) continue;
                
            GetParameter(model, key, primaryKey, property, queryParameters, i);

            i++;
        }

        return queryParameters;
    }

    private static void GetParameter<T>(T model, int key, string? primaryKey, PropertyInfo property,
        QueryParameter[] queryParameters, int i)
    {
        var attribute = property.GetCustomAttribute<FieldAttribute>();
        var column = attribute != null ? attribute.Name : property.Name;
                
        if (column == primaryKey)
            queryParameters[i] = new QueryParameter("@" + column, key);
        else
        {
            var value = property.GetValue(model);
            if (value != null)
                queryParameters[i] = new QueryParameter("@" + column, value);
            else
                queryParameters[i] = new QueryParameter("@" + column, DBNull.Value);
        }
    }


    private static string GenerateUpdateQuery<T>(T model)
    {
        var attObj = model?.GetType().GetCustomAttribute<TableAttribute>();
        var primaryKey = attObj?.PrimaryKey;
        var tableName = attObj?.Name;
        var query = new StringBuilder("UPDATE ")
            .Append(tableName)
            .Append(" SET ");
        
        var properties = model?.GetType().GetProperties();
        if (properties == null)
            throw new PropertyGeneratedIsNullDaoException(nameof(properties), "properties is null");

        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<FieldAttribute>() == null) continue;

            var attribute = property.GetCustomAttribute<FieldAttribute>();
            var column = attribute != null ? attribute.Name : property.Name;

            if (column == primaryKey) continue;

            query.Append(column)
                .Append(" = @")
                .Append(column)
                .Append(',');
        }

        query.Length--; // Remove the last comma
        query.Append(" WHERE ")
            .Append(primaryKey)
            .Append("= @")
            .Append(primaryKey);
        
        return query.ToString();
    }

    public int Delete<T>(int key) where T : class, new()
    {
        var model = new T();
        var query = GenerateDeleteQuery(model);
        var parameters = GetDeleteParameter(model, key);
        return NawaDAO.ExecuteNonQuery(query, parameters, _nawaCon);
    }

    private static QueryParameter[] GetDeleteParameter<T>(T? model, int key)
    {
        var primaryKey = model?.GetType().GetCustomAttribute<TableAttribute>()?.PrimaryKey;
        var queryParameters = new QueryParameter[1];
        queryParameters[0] = new QueryParameter("@" + primaryKey, key);
        return queryParameters;
    }

    private static string GenerateDeleteQuery<T>(T? model)
    {
        var primaryKey = model?.GetType().GetCustomAttribute<TableAttribute>()?.PrimaryKey;
        var tableName = model?.GetType().GetCustomAttribute<TableAttribute>()?.Name;
        var query = $"DELETE FROM {tableName} WHERE {primaryKey} = @{primaryKey}";
        return query;
    }

    private static string GenerateInsertQuery<T>(T model)
    {
        
        var attObj = model?.GetType().GetCustomAttribute<TableAttribute>();
        var type = model?.GetType();
        var tableName = attObj == null ? type?.Name : attObj.Name;

        var primaryKey = attObj?.PrimaryKey;

        var query = new StringBuilder("INSERT INTO ")
            .Append(tableName)
            .Append(" (");
        var values = new StringBuilder(" VALUES (");
        var properties = model?.GetType().GetProperties();

        if (properties == null)
            throw new PropertyGeneratedIsNullDaoException(nameof(properties),"Properties is null");

        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<FieldAttribute>() == null) continue;
            var attribute = property.GetCustomAttribute<FieldAttribute>();
            var column = attribute != null ? attribute.Name : property.Name;

            if (column == primaryKey) continue;
            query.Append(column).Append(',');
            values.Append('@').Append(column).Append(',');
        }

        query.Length--; // Remove the last comma
        values.Length--; // Remove the last comma
        query.Append(')').Append(values).Append(')');
        return query.ToString();
    }

    private static QueryParameter[] GetInsertParameters<T>(T model)
    {
        var properties = model?.GetType().GetProperties();
        if (properties == null) throw new PropertyGeneratedIsNullDaoException(nameof(properties), "No properties found");
            
        var queryParameters = new QueryParameter[properties.Length];
        var i = 0;
        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<FieldAttribute>() == null) continue;
            var attribute = property.GetCustomAttribute<FieldAttribute>();
            var column = attribute != null ? attribute.Name : property.Name;

            queryParameters[i] = new QueryParameter("@" + column, property.GetValue(model) ?? throw new InvalidOperationException());

            i++;
        }

        return queryParameters;

    }
}