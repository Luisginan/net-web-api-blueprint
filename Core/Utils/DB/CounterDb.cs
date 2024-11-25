using System.Data;
using Core.Base;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Core.Config;
using Core.Utils.Security;
using Microsoft.Extensions.Options;
using NawaDataDAL.Common;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class CounterDb:ICounterDb
{
    private static bool _isConfigure;
    private static readonly object LockObj = new();
    private const string ConnectionKey = "CounterDB";
    private NawaConnection? _connection = null;
    private QueryParameter[] _parameters = [];
    public CounterDb(IVault vault, IOptions<DatabaseConfig> option)
    {
        lock (LockObj)
        {
            if (!_isConfigure)
            {
                var databaseConfig = vault.RevealSecret(option.Value);
    
                NawaDAO.Config(databaseConfig.Type,
                    databaseConfig.Server,
                    databaseConfig.Database,
                    databaseConfig.User,
                    databaseConfig.Password,
                    databaseConfig.Port,
                    databaseConfig.CommandTimeout,
                    databaseConfig.PoolSize,
                    databaseConfig.ConnectTimeout,
                    "false",
                    "",
                    "",
                    ConnectionKey
                );
                _isConfigure = true;
            } 
        }
    }
    public (long,long, DateTime) GetCurrentAndSetNewNextValue(string tableName, string columnName, string partnerCode, string reset, int countdata, int timeOutSecond = 5)
    {
        _connection = NawaDAO.CreateConnection(ConnectionKey);

        var param =
            new List<FieldParameter>
            {
                new FieldParameter("@TableName", tableName),
                new FieldParameter("@ColumnName", columnName),
                new FieldParameter("@PartnerCode", partnerCode),
                new FieldParameter("@Reset", reset.ToLower())
            };
        _parameters = param.Select(x => x.GetQueryParameter()).ToArray();

        var datarow =  BeginRowLock(timeOutSecond);
        if (datarow == null)
        {
            throw new ArgumentNullException("Counter Data");
        }

        var value = ConvertDataRow<CounterDbModel>(datarow);
        
        long currentValue = value.NextValue;
        DateTime currentDateTime = DateTime.Now;
        DateTime existingDate = value.ResetTime ?? DateTime.Now;
        switch (reset.ToLower()) {
            case "d":
                if (existingDate.Date != currentDateTime.Date)
                {
                    existingDate = DateTime.Today;
                    currentValue = 1;
                }
                else {
                    existingDate = existingDate.Date;
                }
                break;
            case "m":
                if (existingDate.Year == currentDateTime.Year && existingDate.Month == currentDateTime.Month)
                {
                    existingDate = new DateTime(existingDate.Year, existingDate.Month, 1);
                }
                else
                {
                    existingDate = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);
                    currentValue = 1;
                }
                break;
            case "y":
                if (existingDate.Year == currentDateTime.Year)
                {
                    existingDate = new DateTime(existingDate.Year, 1, 1);
                }
                else
                {
                    existingDate = new DateTime(currentDateTime.Year, 1, 1);
                    currentValue = 1;
                }
                break;
        }
        long newValue = currentValue + countdata;
        var pk = SetValue(tableName, columnName, partnerCode, reset, newValue, existingDate);
        if (pk <= 0) {
            throw new Exception("Failed to update counter");
        }
        _connection.Dispose();
        return (currentValue, newValue-1, existingDate);
    }

  
    private DataRow BeginRowLock(int timeOutSecond = 5)
    {

        var result = NawaDAO.ExecuteRow(@"
            BEGIN; 
            SET statement_timeout = '" + $"{timeOutSecond}s" + @"';
            select ms_counter_next_value, ms_counter_reset_time from ol_ms_counter oml where ms_counter_table_name = @TableName and ms_counter_columns_name = @ColumnName and ms_counter_partner_id = @PartnerCode and Lower(ms_counter_reset)=@Reset FOR UPDATE;
        ", _parameters, _connection!, CommandType.Text, ConnectionKey);
        
        return result;
    }

    private void commit()
    {
        NawaDAO.ExecuteNonQuery("COMMIT;", [], _connection, CommandType.Text, ConnectionKey);
    }
    private void rollback()
    {
        NawaDAO.ExecuteNonQuery("ROLLBACK;", [], _connection, CommandType.Text, ConnectionKey);
    }
    private long SetValue(string tableName, string columnName, string partnerCode, string reset, long newValue, DateTime date)
    {
        try
        {
            var param =
                new List<FieldParameter>
                {
                    new FieldParameter("@TableName", tableName),
                    new FieldParameter("@ColumnName", columnName),
                    new FieldParameter("@PartnerCode", partnerCode),
                    new FieldParameter("@Reset", reset.ToLower()),
                    new FieldParameter("@NextValue", newValue),
                    new FieldParameter("@date", date),
                };
            var lsParameter = param.Select(x => x.GetQueryParameter()).ToArray();

            var pkobj = NawaDAO.ExecuteScalar(
                @"UPDATE ol_ms_counter SET ms_counter_reset_time = @date, ms_counter_next_value = @NextValue, ms_counter_updated_date = now()
                where ms_counter_table_name = @TableName and ms_counter_columns_name = @ColumnName and ms_counter_partner_id = @PartnerCode and Lower(ms_counter_reset)=@Reset returning ms_counter_id;
                ", lsParameter, _connection, CommandType.Text, ConnectionKey);
            var pk = Convert.ToInt64(pkobj);
            commit();
            return pk;
        }
        catch (Exception ex)
        {
            rollback();
            return 0;
        }
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
}

