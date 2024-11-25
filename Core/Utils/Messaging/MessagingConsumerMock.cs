using System.Diagnostics.CodeAnalysis;
using Bogus;
using Core.Config;
using Core.Utils.Security;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Core.Utils.Messaging;
[ExcludeFromCodeCoverage]
public class MessagingConsumerMock(
    ILogger<MessagingConsumerMock> logger,
    IOptions<MessagingConfig> options,
    IConsumerTopicManager consumerTopicManager,
    IVault vault) : IMessagingConsumer, IMessagingConsumerDlq
{
    private MessagingConfig _messagingConfig = vault.RevealSecret(options.Value); 
    Timer? _timer;
    public void Dispose()
    {
        logger.LogInformation("MessagingConsumerMock Dispose");
    }

    public Task Listening(CancellationToken stoppingToken)
    {
        logger.LogWarning("MessagingConsumerMock Listening");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        var faker = new Faker();
        if (FullTopic == consumerTopicManager.GetTopicId("top"))
        {
            var message = new
            {
                Id = Guid.NewGuid().ToString(),
                Message = faker.Lorem.Sentence()
            };
            var jsonString = JsonConvert.SerializeObject(message);
            OnReceiveMessage?.Invoke(Guid.NewGuid().ToString(), jsonString);
        }
        else if (FullTopic == consumerTopicManager.GetTopicId("healthcheck"))
        {
            var message = new 
            {
                audience = "*",
                input = "",
                filter = ""
            };
            var jsonString = JsonConvert.SerializeObject(message);
            OnReceiveMessage?.Invoke(Guid.NewGuid().ToString(), jsonString);
        }
        else if (FullTopic == consumerTopicManager.GetDeadLetterTopicId("top"))
        {
            var message = new
            {
                Id = Guid.NewGuid().ToString(),
                Message = faker.Lorem.Sentence()
            };
            var jsonString = JsonConvert.SerializeObject(message);
            OnReceiveMessage?.Invoke(Guid.NewGuid().ToString(), jsonString);
        }
        
    }

    public event Func<string, string, Task>? OnReceiveMessage;
    public string GroupId { get; set; }
    public string Topic { get; set; }
    public string FullTopic => GetFullTopic();
    public string FullGroupId => GetFullGroupId();
    public void KeepMessageDeadLetter(bool keep)
    {
        logger.LogInformation("MessagingConsumerMock KeepMessageDeadLetter");
    }

    private string GetFullTopic()
    {
        return string.IsNullOrEmpty(_messagingConfig.TopicSuffix) ? Topic : $"{Topic}-{_messagingConfig.TopicSuffix}";
    }

    private string GetFullGroupId()
    {
        if (string.IsNullOrEmpty(_messagingConfig.TopicSuffix))
        {
            return GroupId;
        }
        else
        {
            var realGroupId = GroupId;
            var result = realGroupId.Substring(0, realGroupId.Length - 3);

            return $"{result}{_messagingConfig.TopicSuffix}-sub";
        }
    }
}