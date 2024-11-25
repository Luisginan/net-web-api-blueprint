using System.Diagnostics.CodeAnalysis;
using Core.Utils.Messaging;

namespace Core.Base;

[ExcludeFromCodeCoverage]
public abstract class ProducerBase(ILogger<ProducerBase> logger, IMessagingProducer producer) : BackgroundService
{

    public override Task StartAsync(CancellationToken cancellationToken)
    {
            logger.LogInformation("ProducerBase Info: {info}", "Service Starting...");
            logger.LogInformation("ProducerBase Info: {info}", $"Destination Topic= {GetTopic()}");
            return base.StartAsync(cancellationToken);
        }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
            await Task.Yield();
 
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var items = GetData();
                    foreach (var item in items){
                        var message = await OnExecuteRequest(item);
                        await producer.Produce(GetTopic(), message.Key, message.Value);
                        logger.LogInformation("ProducerBase SendingMessage: {info}", $"Message sent to {GetTopic()} with key {message.Key}");
                    }

                    await Task.Delay(Delay(), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
    }

    protected abstract List<object> GetData();

    protected abstract Task<Message> OnExecuteRequest(object item);


    protected abstract string GetTopic();

    protected abstract int Delay();
        
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ProducerBase StopAsync: {info}", "Service Stopped.");
        return Task.CompletedTask;
    }
}