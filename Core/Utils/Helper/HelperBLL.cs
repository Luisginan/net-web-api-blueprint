using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Core.Config;
using Core.Utils.DB;
using Core.Utils.Security;
using Microsoft.Extensions.Options;
using NawaDataDAL.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Utils.Helper;

[ExcludeFromCodeCoverage]
public class HelperBll : IHelperBll
{
    private static readonly string MachineName = Environment.MachineName;
    private static bool _isConfigure;
    private static readonly object LockObj = new();
    private const string ConnectionKey = "LogApiTransaction";
    public HelperBll(IVault vault, IOptions<DatabaseConfig> option)
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

    private void Log(object objectMessage, string logStatus, Exception? exception = null)
    {
        try
        {
            var conn = NawaDAO.CreateConnection(ConnectionKey);
            FieldParameter[] fieldParameter =
            [
                new FieldParameter("@LogStatus", DbType.String,logStatus),
                new FieldParameter("@LogInfo", DbType.String, Convert.ToString(objectMessage)),
                new FieldParameter("@LogDescription", DbType.String, exception == null ? Convert.ToString(objectMessage) : Convert.ToString(exception)),
                new FieldParameter("@LogCreatedDate", DbType.DateTime, DateTime.Now)
            ];
            var parameters = ConvertParameters(fieldParameter);
            //var query = queryBuilder.GetQuery("OneLoan.HelperBLL.LogInfo");
            string cmdText = "INSERT INTO LogConsoleService(LogStatus, LogInfo, LogDescription, LogCreatedDate) VALUES(@LogStatus, @LogInfo, @LogDescription, @LogCreatedDate)";
            NawaDAO.ExecuteNonQuery(cmdText, parameters, conn, CommandType.Text, ConnectionKey);
            conn.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(Convert.ToString(ex));

            throw;
        }
    }

    public void LogInfo(object objectMessage)
    {
        Log(objectMessage, "Info");
    }

    public void LogError(object message, Exception exception)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(exception);
            
            Log(message,"ERROR", exception);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(Convert.ToString(ex));
            throw;
        }
    }


    public JObject ToJObject(ConcurrentDictionary<string, object> input)
    {
        ArgumentNullException.ThrowIfNull(input);
        JObject jObject = new();

        foreach (var kvp in input)
        {
            var property = new JProperty(kvp.Key, kvp.Value);
            jObject.Add(property);
        }

        return jObject;
    }

    public long InsertLogTransactionApi(string apiRequestId, object header, string tag, string type, string level, string endPoint, string apiMethod, string message, object requestBody, string requestIdApp, string contractNoApp)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentNullException(nameof(tag));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentNullException(nameof(type));

        if (string.IsNullOrWhiteSpace(level))
            throw new ArgumentNullException(nameof(level));

        if (string.IsNullOrWhiteSpace(endPoint))
            throw new ArgumentNullException(nameof(endPoint));

        if (string.IsNullOrWhiteSpace(apiMethod))
            throw new ArgumentNullException(nameof(apiMethod));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));
        
        ArgumentNullException.ThrowIfNull(requestBody);
        
        var conn = NawaDAO.CreateConnection(ConnectionKey);
        FieldParameter[] fieldParameter =
        [
            new FieldParameter("@timestamp_request", DateTime.Now),
            new FieldParameter("@tag", DbType.String, tag),
            new FieldParameter("@type", DbType.String,type),
            new FieldParameter("@host", DbType.String,MachineName),
            new FieldParameter("@level", DbType.String, level),
            new FieldParameter("@message", DbType.String, $"{endPoint}||{apiMethod}||{message}"),
            new FieldParameter("@request_body", DbType.String, JsonConvert.SerializeObject(requestBody)),
            new FieldParameter("@app_request_id", DbType.String, requestIdApp ),
            new FieldParameter("@app_contract_no", DbType.String, contractNoApp),
            new FieldParameter("@api_request_id",apiRequestId),
            new FieldParameter("@header",JsonConvert.SerializeObject(header))
        ];
        var parameters = ConvertParameters(fieldParameter);
        //var query = queryBuilder.GetQuery("OneLoan.HelperBLL.InsertLogTransactionAPI");
        string cmdText = "insert into OL_LOG_TRANSACTION_API (TIMESTAMP_REQUEST, TAG, TYPE, HOST, LEVEL, API_REQUEST_ID, HEADER, MESSAGE, REQUEST_BODY, app_request_id, app_contract_no, active, createddate, lastupdatedate) values (@timestamp_request, @tag, @type, @host, @level, @api_request_id, @header, @message, REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@request_body,'\\n',''),'\\r',''),'\"<','<'),'>\"','>'),'\\\"','\"'), @app_request_id, @app_contract_no, '1', now(), now()) returning PK_LOG_TRANSACTION_API_ID";
        var logId = NawaDAO.ExecuteScalar(cmdText, parameters, conn, CommandType.Text, ConnectionKey);
        if (logId == null)
        {
            throw new Exception($"Failed to Insert Log Transaction API {endPoint}||{apiMethod}||{message}");
        }
        conn.Dispose();
        var value = Convert.ToInt64(logId);
        return value;
    }

    public void UpdateLogTransactionApi(long logId, object responseBody)
    {
        if (logId < 1)
            throw new InvalidDataException($"{nameof(logId)} value is less than 1");

        ArgumentNullException.ThrowIfNull(responseBody);

        var conn = NawaDAO.CreateConnection(ConnectionKey);
        FieldParameter[] fieldParameter =
        [
            new FieldParameter("@timestamp_return", DateTime.Now),
            new FieldParameter("@logID", DbType.Int64, logId),
            new FieldParameter("@response_body", DbType.String, JsonConvert.SerializeObject(responseBody) )
        ];
        var parameters = ConvertParameters(fieldParameter);
        //var query = queryBuilder.GetQuery("OneLoan.HelperBLL.UpdateLogTransactionAPI");
        string cmdText = "update OL_LOG_TRANSACTION_API set TIMESTAMP_RETURN = @timestamp_return, RESPONSE_BODY = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@response_body,'\\n',''),'\\r',''),'\"<','<'),'>\"','>'),'\\\"','\"'), lastupdatedate = now() where PK_LOG_TRANSACTION_API_ID = @logID";
        NawaDAO.ExecuteNonQuery(cmdText, parameters, conn, CommandType.Text, ConnectionKey);
        conn.Dispose();
    }
    private static QueryParameter[] ConvertParameters(IEnumerable<FieldParameter>? fieldParameter)
    {
        fieldParameter ??= new List<FieldParameter>();
        return fieldParameter.Select(x => x.GetQueryParameter()).ToArray();
    }
    public T ConvertXMLtoClass<T>(string xml)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));

        // Membuat pembaca StringReader dari XML string
        using (StringReader stringReader = new StringReader(xml))
        {
            // Deserialisasi XML menjadi objek ClassA
            T obj = (T)serializer.Deserialize(stringReader);
            return obj;
        }

    }

}