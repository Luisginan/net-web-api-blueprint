namespace Core.Utils.Messaging;

public interface IProducerTopicManager
{
    string GetTopicId(string topicName);
    string GetSenderId();
}