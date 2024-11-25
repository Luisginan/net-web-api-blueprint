namespace Core.Utils.Messaging;

public interface IConsumerTopicManager
{
    string GetTopicId(string topicName);
    string GetGroupId(string topicName);
    
    string GetDeadLetterTopicId(string topicName);
    string GetDeadLetterGroupId(string topicName);
    List<string> GetListTopic();
}