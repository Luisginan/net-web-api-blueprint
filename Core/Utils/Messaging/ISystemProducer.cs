namespace Core.Utils.Messaging;

public interface ISystemProducer: IDisposable
{
    Task Produce(string topic, string key, string message, string orderingKey = "");
}