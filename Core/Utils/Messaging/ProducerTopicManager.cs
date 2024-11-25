using System.Diagnostics.CodeAnalysis;
using Core.CExceptions;
using Core.Config;
using Core.Utils.Security;
using Microsoft.Extensions.Options;

namespace Core.Utils.Messaging;
[ExcludeFromCodeCoverage]
public class ProducerTopicManager(IOptions<MessagingConfig> options, IVault vault) : IProducerTopicManager
{
    private readonly MessagingConfig _messagingConfig = vault.RevealSecret(options.Value);
    public string GetTopicId(string name)
    {
        var topic = GetTopic(name);
        return topic.TopicId;
    }

    public string GetSenderId()
    {
        return _messagingConfig.SenderId;
    }

    private MessagingTopicConfig GetTopic(string name)
    {
        var topic = _messagingConfig.Producer.Find(x => x.Name == name);
        if (topic == null)
            throw new TopicManagerException($"Topic {name} not found in configuration");
        return topic;
    }
    
}