using Core.Utils.DB;
using Core.Utils.Messaging;
using Newtonsoft.Json;

namespace Blueprint.RecoveryMessage.Services;

public class RecoveryMessageService(IMessagingProducer messagingProducer, IConsumerLog consumerLog, IConsumerTopicManager consumerTopicManager) : IRecoveryMessageService
{
    public async Task<List<Message<string>>> GetRecoveryMessages()
    {
        var listTopic = consumerTopicManager.GetListTopic();
        var messages = await consumerLog.GetListAsync(listTopic);
        return messages.Select(m => new Message<string>
        {
            Key = m.Key,
            Topic = m.Topic,
            PayLoad = m.PayLoad?.ToString(),
            Method = m.Method,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt,
            Status = m.Status,
            Retry = m.Retry,
            Error = m.Error,
            GroupId = m.GroupId,
            AppId = m.AppId
        }).ToList();
    }

    public async Task UpdateMessage(string key, string message)
    {
        var log = await consumerLog.GetAsync(key);
        if (log == null)
        {
            throw new Exception("Message not found");
        }
        
        await consumerLog.UpdateAsync(key, new Message<object>
        {
            Topic = log.Topic,
            GroupId = log.GroupId,
            AppId = log.AppId,
            Key = log.Key,
            PayLoad = message,
            Retry = log.Retry,
            Error = log.Error,
            CreatedAt = log.CreatedAt,
            UpdatedAt = log.UpdatedAt,
            Status = log.Status
        });
    }

    public void ResendMessage(string key)
    {
        var log = consumerLog.GetAsync(key).Result;
        if (log == null)
        {
            throw new Exception("Message not found");
        }

        if (log.PayLoad == null)
            throw new Exception("Payload is null");

        var message = log.PayLoad.ToString();

        if (String.IsNullOrEmpty(message))
            throw new Exception("Message is empty");
  
        messagingProducer.Produce(log.Topic, log.Key, message);
    }
}