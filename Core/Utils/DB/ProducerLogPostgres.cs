using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using SystemException = System.SystemException;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class ProducerLogPostgres (ILogDb nawaDaoRepository,ILogger<ProducerLogPostgres> logger): IProducerLog
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
                    Method = "producer",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = message.Status
                };
                nawaDaoRepository.ExecuteNonQuery("insert into messaging_log (topic, group_id, app_id, key, payload, retry, error, method, created_at, updated_at) values (@topic, @group_id, @app_id, @key, @payload, @retry,@error, @method, @created_at, @updated_at)", new FieldParameter[]
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
                    new ("@status", logData.Status)
                });
                logger.LogDebug("ProducerLogPostgres InsertAsync: {info}", "Key= " + logData.Key);
            }
            catch (Exception e)
            {
              logger.LogError("ProducerLogPostgres InsertAsync {info}", "Error= " + e.Message);
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
                    Method = "producer",
                    CreatedAt = message.CreatedAt,
                    UpdatedAt = DateTime.Now,
                    Status = message.Status
                    
                };
                nawaDaoRepository.ExecuteNonQuery("update messaging_log set topic = @topic, group_id = @group_id, app_id = @app_id, payload = @payload, retry = @retry, error = @error, method = @method, updated_at = @updated_at where key = @key", new FieldParameter[]
                {
                    new("@topic", logData.Topic),
                    new("@group_id", logData.GroupId),
                    new("@app_id", logData.AppId ?? ""),
                    new("@key", logData.Key),
                    new("@payload", logData.PayLoad),
                    new("@retry", logData.Retry),
                    new("@error", logData.Error),
                    new("@method", logData.Method),
                    new ("@updated_at", logData.UpdatedAt),
                    new ("@status", logData.Status)
                });
                logger.LogDebug("ProducerLogPostgres UpdateAsync: {info}", "Key= " + logData.Key);
            }
            catch (Exception e)
            {
                logger.LogError("ProducerLogPostgres UpdateAsync: {info}", "Error= " + e.Message);
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
                var logData = nawaDaoRepository.ExecuteRow<MessagingLog>("select * from messaging_log where key = @key",
                [
                    new FieldParameter("@key", key)
                ]);


                if (logData == null)
                    return null;

                var payLoad = JsonConvert.DeserializeObject<object>(logData.PayLoad);
                var data = new Message<object>
                {
                    Topic = logData.Topic,
                    GroupId = logData.GroupId,
                    AppId = logData.AppId,
                    Key = logData.Key,
                    PayLoad = payLoad,
                    Retry = logData.Retry,
                    Error = logData.Error,
                    CreatedAt = logData.CreatedAt,
                    UpdatedAt = logData.UpdatedAt,
                    Status = logData.Status
                };

                logger.LogDebug("ProducerLogPostgres GetAsync: {info}", "Key= " + key);
                return data;
            }
            catch (SystemException se)
            {
                logger.LogError("ProducerLogPostgres GetAsync: {info}", "Error= " + se.Message);
                return null;
            }
            catch (Exception e)
            {
                logger.LogError("ProducerLogPostgres GetAsync {info}", "Error= " + e.Message);
                return null;
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
                nawaDaoRepository.ExecuteNonQuery("delete from messaging_log where key = @key", new FieldParameter[]
                {
                    new("@key", key)
                });
                logger.LogDebug("ProducerLogPostgres DeleteAsync: {info}", "Key= " + key);
            }
            catch (Exception e)
            {
                logger.LogError("ProducerLogPostgres DeleteAsync {info}", "Error= " + e.Message);
            }
         });
       
         task.Start();
         await task;
    }
}