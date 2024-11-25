using System.Diagnostics.CodeAnalysis;
using Core.CExceptions;
using Core.Config;
using Core.Utils.Security;
using Microsoft.Extensions.Options;

namespace Core.Utils.Messaging;
[ExcludeFromCodeCoverage]
public class ConsumerTopicManager(IOptions<MessagingConfig> options, IVault vault) : IConsumerTopicManager
{
    private readonly MessagingConfig _messagingConfig = vault.RevealSecret(options.Value);
    public string GetTopicId(string topicName)
    {
        var topic = GetTopic(topicName);
        return topic.TopicId;
    }
    
    public string GetGroupId(string topicName)
    {
        var topic = GetTopic(topicName);
        return topic.GroupId;
    }

    public string GetDeadLetterTopicId(string topicName)
    {
        var topic = GetTopic(topicName);
        return topic.DeadLetterTopicId;
    }

    public string GetDeadLetterGroupId(string topicName)
    {
        var topic = GetTopic(topicName);
        return topic.DeadLetterGroupId;
    }

    public List<string> GetListTopic()
    {
        return _messagingConfig.Consumer.Select(x => x.TopicId).ToList();
    }


    private MessagingTopicConfig GetTopic(string name)
    {
        var topic = _messagingConfig.Consumer.Find(x => x.Name == name);
        if (topic == null)
            throw new TopicManagerException($"Topic {name} not found in configuration");
        return topic;
    }
}