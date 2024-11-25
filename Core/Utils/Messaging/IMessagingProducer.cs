namespace Core.Utils.Messaging;

public interface IMessagingProducer: IDisposable
{
    Task Produce(string topic, string key, string message, string orderingKey = "");
    Task Produce<T>(string topic, string key, T message, string orderingKey = "");

}