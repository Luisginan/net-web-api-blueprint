using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Confluent.Kafka;
using Core.Config;
using Core.Systems;
using Core.Utils.DB;
using Core.Utils.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Core.Utils.Messaging;
[ExcludeFromCodeCoverage]
public sealed class MessagingConsumerKafka(ILogger<MessagingConsumerKafka> logger,
    IOptions<MessagingConfig> messagingConfig, 
    IConsumerLog consumerLog,
    IMetrics metrics,
    IVault vault) : IMessagingConsumer, IMessagingConsumerDlq
{
    private IConsumer<string, string>? _consumer;
    private readonly MessagingConfig _messagingConfig = vault.RevealSecret(messagingConfig.Value);
    private readonly ActivitySource _activitySource = new("messaging.consumer");
    private bool _disposed;

    private ConsumerConfig GetConfig()
    {
        var bootstrapServers = _messagingConfig.BootstrapServers;
        ConsumerConfig config;
        if (_messagingConfig.Authentication)
        {
            config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = FullGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest, // Set to 'Latest' or 'None' if needed
                EnableAutoCommit = false,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = _messagingConfig.SaslUsername,
                SaslPassword = _messagingConfig.SaslPassword,
                SessionTimeoutMs = 45000,
                ClientId = Assembly.GetEntryAssembly()?.GetName().Name
            };
        }
        else
        {
            config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = FullGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest, // Set to 'Latest' or 'None' if needed
                EnableAutoCommit = false,
                SessionTimeoutMs = 45000,
                ClientId = Assembly.GetEntryAssembly()?.GetName().Name
            };
        }

        return config;
    }

    public async Task Listening(CancellationToken stoppingToken)
    {
        metrics.InitCounter(GetMetricNameCounter(),
            GetMetricNameCounter());
        metrics.InitHistogram(GetMetricNameHistogramSuccess(),
            GetMetricNameHistogramSuccess(),
            GetBucketHistogram());
        var loop = true;
        while (loop)
        {
            var lastUpTime = DateTime.UtcNow;
            var config = GetConfig();
            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe(GetFullTopic());
            logger.LogInformation("MessagingConsumerKafka Listening: {info}", $"Topic= {GetFullTopic()}");

            while (!stoppingToken.IsCancellationRequested)
            {
                await OnConsumeMessage(stoppingToken);

                if ((DateTime.UtcNow - lastUpTime).TotalSeconds <= 60) continue;
                    
                _consumer.Close();
                logger.LogWarning("MessagingConsumerKafka Listening: {info}:","service is closing and reopening ...");
                break;
            }
                
            if (!stoppingToken.IsCancellationRequested) continue;
                
            logger.LogWarning("MessagingConsumerKafka Listening: {info}","service is stopping ...");
            _consumer.Close();
            loop = false;
        }
    }

    public event Func<string, string, Task>? OnReceiveMessage;
    public string GroupId { get; set; } = "";
    public string Topic { get; set; } = "";
    public string FullTopic => GetFullTopic();
    public string FullGroupId => GetFullGroupId();
    
    private bool _keepDeadLetter = false;
    public void KeepMessageDeadLetter(bool keep)
    {
       _keepDeadLetter = keep;
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

    private string GetFullTopic()
    {
        return string.IsNullOrEmpty(_messagingConfig.TopicSuffix)
            ? Topic
            : $"{Topic}-{_messagingConfig.TopicSuffix}";
    }

    private async Task OnConsumeMessage(CancellationToken stoppingToken)
    {
        if (_consumer == null)
        {
            logger.LogWarning("MessagingConsumerKafka OnConsumeMessage: {info}", "Consumer is null");
            return;
        }
            
        ConsumeResult<string, string> consumeResult = new();
        try
        {
            consumeResult = _consumer.Consume(stoppingToken);
            await ConsumeMessage(consumeResult);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("MessagingConsumerKafka OnConsumeMessage: {info}", "Service is stopping ...");
        }
        catch (Exception ex)
        {
            try
            {
                var consumerLogExisting = await consumerLog.GetAsync(consumeResult.Message.Key);
                if (consumerLogExisting != null)
                {
                    consumerLogExisting.Error = ex.Message;
                    await consumerLog.UpdateAsync(consumeResult.Message.Key, consumerLogExisting);
                }

                logger.LogError("MessagingConsumerKafka OnConsumeMessage: {info}", "Error= " + ex.Message);
            }
            catch (Exception exception)
            {
                logger.LogError("MessagingConsumerKafka ErrorInCatch : {info}", "Error= " + exception.Message);
            }
        }
    }

    private async Task ConsumeMessage(ConsumeResult<string, string> consumeResult)
    {
        var sw = new Stopwatch();
        var appName = Assembly.GetEntryAssembly()?.GetName().Name;
        using var startAsyncActivity = _activitySource.StartActivity(ActivityKind.Consumer);
        try
        {
            if (ValidateProperty()) return;

            var consumerLogExisting = await consumerLog.GetAsync(consumeResult.Message.Key);
            if (consumerLogExisting != null)
            {
                if (StopRetryFailedMessage(consumerLogExisting)) 
                    return;

                consumerLogExisting.Retry++;
                await consumerLog.UpdateAsync(consumeResult.Message.Key, consumerLogExisting);

                await StartConsumeMessage(consumeResult, sw, startAsyncActivity);
            }
            else
            {
                await InsertLog(consumeResult, appName);
                await StartConsumeMessage(consumeResult, sw, startAsyncActivity);
            }
        }
        catch (Exception e)
        {
            try
            {
                await HandleError(consumeResult, e, sw, startAsyncActivity);
            }
            catch (Exception exception)
            {
                logger.LogError("MessagingConsumerKafka ErrorInCatch : {info}", "Error= " + exception.Message);
            }
        }
    }

    private async Task InsertLog(ConsumeResult<string, string> consumeResult, string? appName)
    {
        var c = GenerateMessageLog(consumeResult, appName);
        await consumerLog.InsertAsync(c);
    }

    private async Task HandleError(ConsumeResult<string, string> consumeResult, Exception e, Stopwatch sw, Activity? startAsyncActivity)
    {
        var consumerLogExisting = await consumerLog.GetAsync(consumeResult.Message.Key);
        if (consumerLogExisting != null)
        {
            consumerLogExisting.Error = e.Message;
            await consumerLog.UpdateAsync(consumeResult.Message.Key, consumerLogExisting);
        }

        sw.Stop();
        logger.LogError(
            "MessagingConsumerKafka Message consumed failed: {info}",$" id: {consumeResult.Message.Key} duration: {sw.Elapsed.TotalSeconds} seconds Error: {e.Message}");
        SetErrorActivity(consumeResult, startAsyncActivity, e);
    }

    private void SetErrorActivity(ConsumeResult<string, string> consumeResult, Activity? startAsyncActivity, Exception e)
    {
        startAsyncActivity?.SetTag("topic", GetFullTopic());
        startAsyncActivity?.SetTag("messageId", consumeResult.Message.Key);
        startAsyncActivity?.SetTag("groupId", FullGroupId);
        startAsyncActivity?.SetTag("exception", e.Message);
        startAsyncActivity?.SetTag("stackTrace", e.StackTrace);
        startAsyncActivity?.SetTag("exceptionType", e.GetType().ToString());
        startAsyncActivity?.SetTag("exceptionSource", e.Source);
        startAsyncActivity?.SetTag("exceptionTargetSite", e.TargetSite?.ToString());
        startAsyncActivity?.SetTag("exceptionData", e.Data.ToString());
        startAsyncActivity?.SetStatus(ActivityStatusCode.Error,
            $"Error when processing id {GetFullTopic().Replace("-", "")} message {e.Message}");
    }

    private async Task StartConsumeMessage(ConsumeResult<string, string> consumeResult, Stopwatch sw, Activity? startAsyncActivity)
    {
        sw.Start();
        metrics.SetCounter(GetMetricNameCounter());
        
        if (OnReceiveMessage!= null)
            await OnReceiveMessage(consumeResult.Message.Key, consumeResult.Message.Value);
        
        sw.Stop();
        _consumer?.Commit(consumeResult);
        await consumerLog.DeleteAsync(consumeResult.Message.Key);
        logger.LogInformation(
            "MessagingConsumerKafka Message consumed successfully: {info}",$" id: {consumeResult.Message.Key} duration: {sw.Elapsed.TotalSeconds} seconds");
        metrics.SetHistogram(GetMetricNameHistogramSuccess(), sw.Elapsed.TotalSeconds);

        startAsyncActivity?.SetTag("topic", GetFullTopic());
        startAsyncActivity?.SetTag("messageId", consumeResult.Message.Key);
        startAsyncActivity?.SetTag("groupId", FullGroupId);
    }
    
    private static double[] GetBucketHistogram()
    {
        return [1.0, 3.0, 5.0];
    }

    private string GetMetricNameCounter()
    {
        return "consume_topic_" + GetFullTopic().Replace("-", "");
    }

    private string GetMetricNameHistogramSuccess()
    {
        return "duration_consume_topic_" + GetFullTopic().Replace("-", "");
    }

    private Message<object> GenerateMessageLog(ConsumeResult<string, string> consumeResult, string? appName)
    {
        var c = new Message<object>
        {
            AppId = appName,
            GroupId = FullGroupId,
            Topic = consumeResult.Topic,
            PayLoad = consumeResult.Message.Value,
            Key = consumeResult.Message.Key,
            Retry = 0
        };
        return c;
    }

    private bool StopRetryFailedMessage(Message<object> consumerLogExisting)
    {
        if (consumerLogExisting.Retry <= 3) 
            return false;
        logger.LogError(
            "Retry limit reached, message will be stop and logging in database");
        _consumer?.Commit();
        return true;

    }

    private bool ValidateProperty()
    {
        if (Topic.IsNullOrEmpty())
        {
            logger.LogError("MessagingConsumerKafka Topic is empty. service will be stop.");
            return true;
        }
            
        if (GroupId.IsNullOrEmpty())
        {
            logger.LogError("MessagingConsumerKafka GroupId is empty. service will be stop.");
            return true;
        }
            
        if (OnReceiveMessage == null)
        {
            logger.LogError("MessagingConsumerKafka OnReceiveMessage is null. service will be stop.");
            return true;
        }

        return false;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _consumer?.Dispose();
            }

            // Dispose unmanaged resources (if any)

            _consumer = null;
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~MessagingConsumerKafka()
    {
        Dispose(false);
    }
}