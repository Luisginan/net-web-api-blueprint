namespace Core.Utils.Messaging;

public interface IMessagingConsumer :IDisposable
{
    Task Listening(CancellationToken stoppingToken );
    event Func<string, string, Task>? OnReceiveMessage;
    string GroupId { get; set; }
    string Topic { get; set; }
    string FullTopic { get; }
    string FullGroupId { get; }
}