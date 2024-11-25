using System.Diagnostics.CodeAnalysis;
using Blueprint.HealthCheck.Models;
using Core.Base;
using Core.Utils.DB;
using Core.Utils.Messaging;

namespace Blueprint.HealthCheck.MasConsumer;
[ExcludeFromCodeCoverage]
public class HealthCheckListener(ILogger<ConsumerBase<HealthCheckPayload>> logger, 
    IMessagingConsumer messagingConsumer,
    ILogger<ConsumerBase<HealthCheckPayload>> loggerBase,
    IServiceProvider serviceProvider,
    IConsumerTopicManager consumerTopicManager
    ) : ConsumerBase<HealthCheckPayload>(loggerBase, messagingConsumer)
{

    protected override string GetTopic()
    {

        return consumerTopicManager.GetTopicId("healthcheck");
    }

    protected override Task OnReceiveMessage(string key, HealthCheckPayload message)
    {
        try
        {
            var appId = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name?.ToLower();
            if (message.Audience == appId || message.Audience == "*")
            {
                using var scope = serviceProvider.CreateScope();
                CheckHealthDatabase(scope);
                CheckHealthCache(scope);
                CheckHealthLogDb(scope);
            }
            else
            {
                logger.LogWarning("HealthCheckListener receivedMessage info : {error}", "the message not for " + appId);
            }

        }
        catch (Exception e)
        {
            logger.LogError("HealthCheckListener receivedMessage error : {error}", e.Message);
        }
       
        return Task.CompletedTask;
    }

    private void CheckHealthLogDb(IServiceScope scope)
    {
        var logDb = scope.ServiceProvider.GetRequiredService<ILogDb>();
        try
        {
            logDb.ExecuteNonQuery("select 1",[]);
            logger.LogInformation("HealthCheckListener receivedMessage success info : {info}", "LogDb Connected");
        }
        catch (Exception e)
        {
            logger.LogError("HealthCheckListener receivedMessage error : {error}", $"Connection LogDb failed - {e.Message}");
        }
    }

    private void CheckHealthCache(IServiceScope scope)
    {
        var cache = scope.ServiceProvider.GetRequiredService<ICache>();
        cache.SetString("healthcheck", "success");
        var cacheValue = cache.GetString("healthcheck");
        if (cacheValue != "success")
            logger.LogError("HealthCheckListener receivedMessage error : {error}", "Connection cache failed");
        else
            logger.LogInformation("HealthCheckListener receivedMessage success info : {info}", "Cache Connected");
    }

    private void CheckHealthDatabase(IServiceScope scope)
    {
        try
        {
            var nawaDao = scope.ServiceProvider.GetRequiredService<INawaDaoRepository>();
            nawaDao.ExecuteNonQuery("select 1",[]);
            logger.LogInformation("HealthCheckListener receivedMessage success info : {info}", "Database connected");
        }
        catch (Exception e)
        {
            logger.LogError("HealthCheckListener receivedMessage error : {error}", $"Connection database failed - {e.Message}");
        }
        
    }

    protected override string GetGroupId()
    {
        return consumerTopicManager.GetGroupId("healthcheck");
    }
}