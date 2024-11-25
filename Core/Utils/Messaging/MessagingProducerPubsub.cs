using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Core.Config;
using Core.Utils.DB;
using Core.Utils.Security;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Options;

namespace Core.Utils.Messaging;
[ExcludeFromCodeCoverage]
public class MessagingProducerPubsub(ILogger<MessagingProducerPubsub> logger, 
    IOptions<MessagingConfig> messagingConfig, 
    IProducerLog producerLog, 
    IVault vault) : IMessagingProducer, ISystemProducer
{
    private readonly MessagingConfig _messagingConfig = vault.RevealSecret(messagingConfig.Value);
    private readonly ActivitySource _activitySource = new("messaging.producer");
    private string _topic = "";
    private bool _isSuccess;

    void IDisposable.Dispose()
    {
        _activitySource.Dispose();
        GC.SuppressFinalize(this);
    }

    public  async Task Produce(string topic, string key, string message, string orderingKey = "")
    {
        _topic = CheckAndUpdateSuffix(topic);
        await PostWithRetry(_topic, key, message, orderingKey);
    }

    public async Task Produce<T>(string topic, string key, T message, string orderingKey = "")
    {
        var messageString = System.Text.Json.JsonSerializer.Serialize(message);
        await Produce(topic, key, messageString, orderingKey);
    }

    private async Task Post(string topic, string key, string message, string orderingKey = "")
    {
        var publisher = await PublisherServiceApiClient.CreateAsync();
        var topicName = new TopicName(_messagingConfig.ProjectId, topic);
        var data = System.Text.Encoding.UTF8.GetBytes(message);
        var appId = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        await publisher.PublishAsync(topicName, [
            new PubsubMessage
            {
                MessageId = key, 
                Data = ByteString.CopyFrom(data),
                Attributes =
                {
                    {"OriginalKey",key}, 
                    {"Sender", appId},
                    {"SendAt", DateTime.Now.ToString(CultureInfo.InvariantCulture) }
                }, 
                OrderingKey = orderingKey
            }]);
        logger.LogDebug("MessagingProducerPubsub Produce: {info}", $"Message sent to {topic} with message {message}");
    }
    
    private async Task PostWithRetry(string topic, string key, string message, string orderingKey = "", int maxRetries = 3)
    {
       
        var retries = 1;
        var isRetry = false;
        var delay = TimeSpan.FromSeconds(3);
        var loop = true;
        
        while (loop)
        {
            using var postWithRetry = _activitySource.StartActivity( ActivityKind.Producer);
            try
            {
                await Post(topic, key, message, orderingKey);
                await DeleteLog(key, isRetry);
                SetActivitySuccess(topic, key, postWithRetry, retries, isRetry);
                _isSuccess = true;
                break;
            }
            catch (Exception ex)
            {
                isRetry = true;
                logger.LogError("MessagingProducerPubsub Produce: {info}", $"Error= {ex?.Message} for message {message}");
                logger.LogInformation("MessagingProducerPubsub Produce: {info}", $"Retrying to send message to {topic}, message {message}");
                
                await UpdateLog(key, message, retries);
                
                retries++;
                loop = !IsMaxRetry(key, maxRetries, retries);
                
                await Task.Delay(delay);
                delay *= 2;
                
                SetActivityError(topic, key, postWithRetry, retries, isRetry, ex);
            }
        }
        
       
    }

    private async Task UpdateLog(string key, string message, int retries)
    {
        if (retries == 1)
            await InsertLog(key, message, retries);
        else
            await UpdateLog(key, retries);
    }

    private async Task DeleteLog(string key, bool isRetry)
    {
        if (isRetry)
            await producerLog.DeleteAsync(key);
    }

    private static void SetActivityError(string topic, string key, Activity? postWithRetry, int retries, bool isRetry,
        Exception? ex)
    {
        postWithRetry?.SetTag("retries", retries);
        postWithRetry?.SetTag("isRetry", isRetry);
        postWithRetry?.SetTag("topic", topic);
        postWithRetry?.SetTag("key", key);
        postWithRetry?.SetTag("exception", ex?.Message);
    }

    private static void SetActivitySuccess(string topic, string key, Activity? postWithRetry, int retries, bool isRetry)
    {
        postWithRetry?.SetTag("retries", retries);
        postWithRetry?.SetTag("isRetry", isRetry);
        postWithRetry?.SetTag("topic", topic);
        postWithRetry?.SetTag("key", key);
    }

    private bool IsMaxRetry(string key, int maxRetries, int retries)
    {
        if (retries <= maxRetries) return false;
        
        logger.LogError("MessagingProducerPubsub Produce: {info}"," Max Retries Reached");
        logger.LogInformation("MessagingProducerPubsub Produce: {info}","stop retrying and save to db: " + key);

        return true;
    }

    private async Task UpdateLog(string key, int retries)
    {
        var pe = await producerLog.GetAsync(key);
        if (pe != null)
        {
            pe.Retry = retries;
            await producerLog.UpdateAsync(key, pe);
        }
    }

    private async Task InsertLog(string key, string message, int retries)
    {
        var appId = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        var p = new Message<object>(topic: _topic, appId: appId, key: key, payLoad: message, retry: retries);
        await producerLog.InsertAsync(p);
    }

    private string CheckAndUpdateSuffix(string topic)
    {
        if (string.IsNullOrEmpty(_messagingConfig.TopicSuffix)) return topic;
        
        var suffixed = topic.Split("-").Last();
        if (!string.IsNullOrEmpty(suffixed))
        {
            if (!suffixed.Equals(_messagingConfig.TopicSuffix))
            {
                topic = $"{topic}-{_messagingConfig.TopicSuffix}";
            }
        }
        else
        {
            topic = $"{topic}-{_messagingConfig.TopicSuffix}";
        }

        return topic;
    }
    
    public string GetTopic()
    {
        return _topic;
    }

    public bool IsSuccess()
    {
        return _isSuccess;
    }
}