using System.Diagnostics.CodeAnalysis;
using Core.CExceptions;
using Core.Utils.Messaging;
using Newtonsoft.Json;

namespace Core.Base;

[ExcludeFromCodeCoverage]
public abstract class ConsumerBase2<T>(
    ILogger<ConsumerBase2<T>> logger,
    IMessagingConsumer messagingConsumer,
    IMessagingConsumerDlq deadLetterConsumer) : BackgroundService where T : class
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        messagingConsumer.OnReceiveMessage += async (key, message) =>
        {
            var m = JsonConvert.DeserializeObject<T>(message);
            if (m == null)
              throw new DeserializeConsumerMessagingException("Error deserializing message");
            
            await OnReceiveMessage(key, m);
        };
        messagingConsumer.GroupId = GetGroupId();
        messagingConsumer.Topic = GetTopic();
        
        logger.LogInformation("ConsumerBase2 StartAsync: {info}", "Starting...");
        logger.LogInformation("ConsumerBase2 StartAsync: {info}", $"Subscribed to topic=  {messagingConsumer.FullTopic}");
        logger.LogInformation("ConsumerBase2 StartAsync: {info}", $"GroupId=  {messagingConsumer.FullGroupId}");
        
        deadLetterConsumer.KeepMessageDeadLetter(true);
        deadLetterConsumer.OnReceiveMessage += async (key, message) =>
        {
            var m = JsonConvert.DeserializeObject<T>(message);
            if (m == null)
              throw new Exception("Error deserializing message");
            
            await OnDeadLetterReceiveMessage(key, m);
        };
        
        deadLetterConsumer.GroupId = GetDeadLetterGroupId();
        deadLetterConsumer.Topic = GetDeadLetterTopic();
        
        logger.LogInformation("ConsumerBase2 StartAsync: {info}", "Starting...");
        logger.LogInformation("ConsumerBase2 StartAsync: {info}", $"Deadletter to topic=  {deadLetterConsumer.FullTopic}");
        logger.LogInformation("ConsumerBase2 StartAsync: {info}", $"Deadletter GroupId=  {deadLetterConsumer.FullGroupId}");
        
       
        
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
       
        var taskMessaging = messagingConsumer.Listening(stoppingToken);
        var taskDeadLetter =  deadLetterConsumer.Listening(stoppingToken);
        await Task.WhenAll(taskMessaging, taskDeadLetter);
    }
    
    protected abstract string GetTopic();
    
    protected abstract string GetGroupId();
    
    protected abstract string GetDeadLetterTopic();
    
    protected abstract string GetDeadLetterGroupId();

    protected abstract Task OnReceiveMessage(string key, T message);
    
    protected abstract Task OnDeadLetterReceiveMessage(string key, T message);

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ConsumerBase StopAsync: {info}", "Stopping...");
        return Task.CompletedTask;
    }

    
}