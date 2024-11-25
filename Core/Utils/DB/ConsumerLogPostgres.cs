using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class ConsumerLogPostgres(ILogDb logDb, ILogger<ConsumerLogPostgres> logger) : IConsumerLog
{
    public async Task InsertAsync(Message<object> message)
    {
        var task = new Task(() => {
            try
            {
                var payLoad = JsonConvert.SerializeObject(message.PayLoad);
                var logData = new MessagingLog
                {
                    Topic = message.Topic,
                    GroupId = message.GroupId,
                    AppId = message.AppId,
                    Key = message.Key,
                    PayLoad = payLoad,
                    Retry = message.Retry,
                    Error = message.Error ?? "",
                    Method = "consumer",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = message.Status
                };
                logDb.ExecuteNonQuery("insert into messaging_log (topic, group_id, app_id, key, payload, retry, error, method, created_at, updated_at, status) values (@topic, @group_id, @app_id, @key, @payload, @retry,@error, @method, @created_at, @updated_at, @status)", new FieldParameter[]
                {
                    new("@topic", logData.Topic),
                    new("@group_id", logData.GroupId),
                    new("@app_id", logData.AppId ?? ""),
                    new("@key", logData.Key),
                    new("@payload", logData.PayLoad),
                    new("@retry", logData.Retry),
                    new("@error", logData.Error),
                    new("@method", logData.Method),
                    new ("@created_at", logData.CreatedAt),
                    new ("@updated_at", logData.UpdatedAt),
                    new("@status", logData.Status)
                });
               logger.LogDebug("ConsumerLogPostgres InsertAsync: {info}", "Key= " + logData.Key);
            }
            catch (Exception e)
            {
              logger.LogError("ConsumerLogPostgres InsertAsync: {info}", "Error= " + e.Message);
            }
        }); 
        task.Start();
        await task;
    }

    public async Task UpdateAsync(string key, Message<object> message)
    {
       var task = new Task(() => {
                try
                {
                    var logData = new MessagingLog
                    {
                        Topic = message.Topic,
                        GroupId = message.GroupId,
                        AppId = message.AppId,
                        Key = message.Key,
                        PayLoad = message.PayLoad?.ToString() ?? string.Empty,
                        Retry = message.Retry,
                        Error = message.Error ?? "",
                        Method = "consumer",
                        CreatedAt = message.CreatedAt,
                        UpdatedAt = DateTime.Now,
                        Status = message.Status
                    };
                    logDb.ExecuteNonQuery("update messaging_log set topic = @topic, group_id = @group_id, app_id = @app_id, payload = @payload, retry = @retry, error = @error, method = @method, updated_at = @updated_at, status = @status where key = @key", new FieldParameter[]
                    {
                        new("@topic", logData.Topic),
                        new("@group_id", logData.GroupId),
                        new("@app_id", logData.AppId ?? ""),
                        new("@key", logData.Key),
                        new("@payload", logData.PayLoad),
                        new("@retry", logData.Retry),
                        new("@error", logData.Error),
                        new("@method", logData.Method),
                        new("@updated_at", logData.UpdatedAt),
                        new("@status", logData.Status)
                    });
                    logger.LogDebug("ConsumerLogPostgres UpdateAsync: {info}", "Key= " + logData.Key);
                }
                catch (Exception e)
                {
                    logger.LogError("ConsumerLogPostgres UpdateAsync: {info}", "Error= " + e.Message);
                }
       }); 
       task.Start();
       await task;
    }

    public async Task<Message<object>?> GetAsync(string key)
    {
        var task = new Task<Message<object>?>(() =>
        {
            try
            {
                var logData = logDb.ExecuteRow<MessagingLog>("select * from messaging_log where key = @key",
                [
                    new FieldParameter("@key", key)
                ]);
                
               
                if (logData == null)
                    return null;
                
                var data = new Message<object>
                {
                    Topic = logData.Topic,
                    GroupId = logData.GroupId,
                    AppId = logData.AppId,
                    Key = logData.Key,
                    PayLoad = logData.PayLoad,
                    Retry = logData.Retry,
                    Error = logData.Error,
                    CreatedAt = logData.CreatedAt,
                    UpdatedAt = logData.UpdatedAt,
                    Status = logData.Status
                };
                
                logger.LogDebug("ConsumerLogPostgres GetAsync: {info}", "Key= " + key);
                return data;
            }
            catch (Exception e)
            {
                logger.LogError("ConsumerLogPostgres GetAsync: {info}", "Error= " + e.Message);
                return null;
            }
        });
        
        task.Start();
        await task;
        return task.Result;
    }

    public async Task<List<Message<object>>> GetListAsync(List<string> listTopic)
    {
        var task = new Task<List<Message<object>>>(() =>
        {
            try
            {
                var stringFilter = string.Join(",", listTopic.Select(x => "'" + x + "'"));
                var logData =
                    logDb.ExecuteTable<MessagingLog>(
                        $"select * from messaging_log where topic in ({stringFilter}) and status = 'DEAD'", []);
                var data = new List<Message<object>>();
                foreach (var log in logData)
                {
                   
                    var message = new Message<object>
                    {
                        Topic = log.Topic,
                        GroupId = log.GroupId,
                        AppId = log.AppId,
                        Key = log.Key,
                        PayLoad = log.PayLoad,
                        Retry = log.Retry,
                        Error = log.Error,
                        CreatedAt = log.CreatedAt,
                        UpdatedAt = log.UpdatedAt,
                        Status = log.Status,
                        Method = log.Method
                    };
                    data.Add(message);
                }
                logger.LogDebug("ConsumerLogPostgres GetAsync: {info}", "Count= " + data.Count);
                return data;
            }
            catch (Exception e)
            {
                logger.LogError("ConsumerLogPostgres GetAsync: {info}", "Error= " + e.Message);
                return new List<Message<object>>();
            }
        });
        
        task.Start();
        await task;
        return task.Result;
    }

    public async Task DeleteAsync(string key)
    {
       var task = new Task(() => {
            try
            {
                logDb.ExecuteNonQuery("delete from messaging_log where key = @key", new FieldParameter[]
                {
                    new("@key", key)
                });
                logger.LogDebug("ConsumerLogPostgres DeleteAsync: {info}", "Key= " + key);
            }
            catch (Exception e)
            {
                logger.LogError("ConsumerLogPostgres DeleteAsync: {info}", "Error= " + e.Message);
            }
       });
       
       task.Start();
       await task;
    }
}