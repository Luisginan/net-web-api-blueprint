using System.Diagnostics.CodeAnalysis;
using Core.CExceptions;
using Core.Utils.Messaging;
using Newtonsoft.Json;

namespace Core.Base;

[ExcludeFromCodeCoverage]
public abstract class ConsumerBase<T>(
    ILogger<ConsumerBase<T>> logger,
    IMessagingConsumer messagingConsumer) : BackgroundService where T : class
{

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        messagingConsumer.OnReceiveMessage += async (key, message) =>
        {
            var model = JsonConvert.DeserializeObject<T>(message);
            if (model == null)
              throw new DeserializeConsumerMessagingException(message);
            
            await OnReceiveMessage(key, model);
        };
        messagingConsumer.GroupId = GetGroupId();
        messagingConsumer.Topic = GetTopic();
        
        logger.LogInformation("ConsumerBase StartAsync: {info}", "Starting...");
        logger.LogInformation("ConsumerBase StartAsync: {info}", $"Subscribed to topic=  {messagingConsumer.FullTopic}");
        logger.LogInformation("ConsumerBase StartAsync: {info}", $"GroupId=  {messagingConsumer.FullGroupId}");
        
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
       
        await messagingConsumer.Listening(stoppingToken);
        
    }
    
    protected abstract string GetTopic();

    protected abstract Task OnReceiveMessage(string key, T message);
    
    protected abstract string GetGroupId();

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ConsumerBase StopAsync: {info}", "Stopping...");
        return Task.CompletedTask;
    }

    
}