using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.Messaging;
[ExcludeFromCodeCoverage]
public class MessagingProducerMock(ILogger<MessagingProducerMock> logger) : IMessagingProducer, ISystemProducer
{
    public void Dispose()
    {
       logger.LogWarning("MessagingProducerMock Dispose: using Mock");
       GC.SuppressFinalize(this);
    }

    public Task Produce(string topic, string key, string message, string orderingKey = "")
    {
        logger.LogWarning("MessagingProducerMock Produce: using Mock");
        return Task.CompletedTask;
    }

    public async Task Produce<T>(string topic, string key, T message, string orderingKey = "")
    {
        logger.LogWarning("MessagingProducerMock Produce<T>: using Mock");
        await Task.CompletedTask;
    }
}