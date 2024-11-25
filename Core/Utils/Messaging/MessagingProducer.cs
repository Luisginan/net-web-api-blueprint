using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Confluent.Kafka;
using Core.Config;
using Core.Utils.DB;
using Core.Utils.Security;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Core.Utils.Messaging;

[ExcludeFromCodeCoverage]
public sealed class MessagingProducer(
    ILogger<MessagingProducer> logger,
    IOptions<MessagingConfig> options,
    IProducerLog producerLog,
    IVault vault)
    : IMessagingProducer, ISystemProducer
{
    private readonly MessagingConfig _messagingConfig = vault.RevealSecret(options.Value);
    private IProducer<string, string>? _producer;
    private readonly ActivitySource _activitySource = new("messaging.producer");
    private string _topic = "";
    private bool _isSuccess;
    private bool _disposed;

    public async Task Produce<T>(string topic, string key, T message, string orderingKey = "")
    {
        var messageString = JsonConvert.SerializeObject(message);
        await Produce(topic, key, messageString);
    }
    public Task Produce(string topic, string key, string message, string orderingKey = "")
    {
        using var produceActivity = _activitySource.StartActivity(ActivityKind.Producer);
        _topic = CheckAndUpdateSuffix(topic);    
        ProducerConfig config;
        if (_messagingConfig.Authentication)
        {
            config = new ProducerConfig
            {
                BootstrapServers = _messagingConfig.BootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = _messagingConfig.SaslUsername,
                SaslPassword = _messagingConfig.SaslPassword,
                //EnableIdempotence = true,
                ClientId = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name
            };
        } else {
            config = new ProducerConfig
            {
                BootstrapServers = _messagingConfig.BootstrapServers,
                //EnableIdempotence = true,
                ClientId = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name
            };
        }
           
        _producer = new ProducerBuilder<string, string>(config).Build();
            
        var appId = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        var p = new Message<object>
        {
            AppId = appId,
            Key = key,
            PayLoad = message,
            Topic = _topic,
            Retry = -1,
            Method = "producer"
        };
        producerLog.InsertAsync(p);
        _producer.Produce(_topic, new Message<string, string> { Key = key, Value = message }, r=>
        {
            if (r.Error.Code != ErrorCode.NoError)
            {
                logger.LogError("MessagingProducer Produce: {info}", "Error= " + r.Error.Reason);
                _isSuccess = false;
            }
            else
            {
                logger.LogDebug("MessagingProducer Produce: {info}", "Topic= " + r.TopicPartitionOffset);
                producerLog.DeleteAsync(key);
                _isSuccess = true;
            }
        });
            
        produceActivity?.SetTag("topic", _topic);
        produceActivity?.SetTag("key", key);
            
        return Task.CompletedTask;
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

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _producer?.Dispose();
            }

            // Dispose unmanaged resources (if any)

            _producer = null;
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~MessagingProducer()
    {
        Dispose(false);
    }
}