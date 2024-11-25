using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Core.Config;
using Core.Systems;
using Core.Utils.DB;
using Core.Utils.Security;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;

namespace Core.Utils.Messaging;

[ExcludeFromCodeCoverage]
public class MessagingConsumerPubsub(
    ILogger<MessagingConsumerPubsub> logger,
    IOptions<MessagingConfig> messagingConfig,
    IMetrics metrics,
    IVault vault,
    IConsumerLog consumerLog
)
    : IMessagingConsumer, IMessagingConsumerDlq
{
    private readonly ActivitySource _activitySource = new("messaging.consumer");
    private readonly MessagingConfig _messagingConfig = vault.RevealSecret(messagingConfig.Value);
    private bool _isListening;
    private SemaphoreSlim? _semaphore;

    public void Dispose()
    {
    }

    public async Task Listening(CancellationToken stoppingToken)
    {
        _semaphore = new SemaphoreSlim(_messagingConfig.InitialCount, _messagingConfig.MaxCount);
        _isListening = true;
        logger.LogInformation("MessagingConsumerPubsub Listening: {info}", $"Semaphore initial count: {_messagingConfig.InitialCount}, max count: {_messagingConfig.MaxCount}");
        logger.LogInformation("MessagingConsumerPubsub Listening: {info}", $"Semaphore current count: {_semaphore.CurrentCount}");  
        await PullMessages(stoppingToken);
    }

    public async Task StopListening()
    {
        if (_subscriber == null) return;
        if (!_isListening) return;

        _isListening = false;
        await _subscriber.StopAsync(CancellationToken.None);
        await _subscriber.DisposeAsync();
    }

    private SubscriberClient? _subscriber;

    public event Func<string, string, Task>? OnReceiveMessage;
    public string GroupId { get; set; } = "";
    public string Topic { get; set; } = "";

    public string FullTopic => GetFullTopic();
    public string FullGroupId => GetFullGroupId();
    
    private bool _keepMessageDeadLetter;
    public void KeepMessageDeadLetter(bool keep)
    {
        _keepMessageDeadLetter = keep;
    }

    private async Task PullMessages(CancellationToken stoppingToken)
    {
        using var pullActivity = _activitySource.StartActivity(ActivityKind.Consumer);
        InitMetric();
        try
        {
            
            if (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("MessagingConsumerPubsub OnPullMessages: {info}",
                    "Cancellation requested, stopping message processing.");
                return;
            }

            var subscriptionName = SubscriptionName.FromProjectSubscription(_messagingConfig.ProjectId, FullGroupId);

            _subscriber = await SubscriberClient.CreateAsync(subscriptionName);

            
            var startTask = _subscriber.StartAsync(async (message, cancellationToken) =>
                await ConsumeMessage(message, cancellationToken));

            await startTask;
        }
        catch (Exception e)
        {
            logger.LogError("MessagingConsumerPubsub OnPullMessages: {info}", $"Error= {e.Message}");
            SetActivityOnError(pullActivity, e);
        }
       
    }

    private void InitMetric()
    {
        try
        {
            metrics.InitCounter(GetMetricNameCounter(),
                GetMetricNameCounter());
            metrics.InitHistogram(GetMetricNameHistogramSuccess(),
                GetMetricNameHistogramSuccess(),
                GetBucketHistogram());
            metrics.InitHistogram(GetMetricNameHistogramFailed(),
                GetMetricNameHistogramFailed(),
                GetBucketHistogram());
        }
        catch (Exception ex)
        {
            logger.LogError($"MessagingConsumerPubsub Metric Initiation - error : {ex.Message}");          
        }
    }

    private static double[] GetBucketHistogram()
    {
        return [1.0, 3.0, 5.0];
    }

    private string GetMetricNameHistogramFailed()
    {
        return "duration_consume_topic_failed_" + FullTopic.Replace("-", "");
    }

    private string GetMetricNameHistogramSuccess()
    {
        return "duration_consume_topic_success_" + FullTopic.Replace("-", "");
    }

    private string GetMetricNameCounter()
    {
        return "consume_topic_" + FullTopic.Replace("-", "");
    }

    private void SetActivityOnError(Activity? pullActivity, Exception e)
    {
        pullActivity?.SetTag("topic", FullTopic);
        pullActivity?.SetTag("groupId", FullGroupId);
        pullActivity?.SetTag("exception", e.Message);
        pullActivity?.SetTag("stackTrace", e.StackTrace);
        pullActivity?.SetTag("exceptionType", e.GetType().ToString());
        pullActivity?.SetTag("exceptionSource", e.Source);
        pullActivity?.SetTag("exceptionTargetSite", e.TargetSite?.ToString());
        pullActivity?.SetTag("exceptionData", e.Data.ToString());
        pullActivity?.SetStatus(ActivityStatusCode.Error, "Error when pulling messages: " + e.Message);
    }

    private int threadWaitingCount = 0;
    private int thredAdditionalCount = 0;
    private async Task<SubscriberClient.Reply> ConsumeMessage(
        PubsubMessage message, CancellationToken cancellationToken)
    {
        var originalKey = GetOriginalKey(message);
        using var startAsyncActivity = _activitySource.StartActivity();
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("MessagingConsumerPubsub ConsumeMessage: {info}",
                "Cancellation requested, stopping message processing.");
            return await Task.FromResult(SubscriberClient.Reply.Nack);
        }
        RecheckMetric();
        var sw = new Stopwatch();
        try
        {
            Interlocked.Increment(ref threadWaitingCount);
            if (_semaphore != null)
            {
                if (_semaphore.CurrentCount == 0)
                {
                    logger.LogWarning("MessagingConsumerPubsub ConsumeMessage: {info}",
                        $"Semaphore is full, waiting for available slot for {originalKey}. Waiting thread: {threadWaitingCount}");
                    
                    if (threadWaitingCount > _messagingConfig.MaxCount)
                    {
                        if(_messagingConfig.InitialCount< _messagingConfig.MaxCount)
                            if ((thredAdditionalCount + _messagingConfig.InitialCount) < _messagingConfig.MaxCount)
                            {
                                _semaphore.Release();
                                Interlocked.Increment(ref thredAdditionalCount);
                                logger.LogWarning("MessagingConsumerPubsub ConsumeMessage: {info}",
                                    $"waiting thread is greater than max count, release one additional slot for {originalKey}. Additional slot count: {thredAdditionalCount}");
                            }
                    }
                }
                
                await _semaphore.WaitAsync(cancellationToken);
                Interlocked.Decrement(ref threadWaitingCount);
                logger.LogDebug("MessagingConsumerPubsub ConsumeMessage: {info}",
                    $"Semaphore acquired for {originalKey}. Available slot: {_semaphore.CurrentCount}");
            }
            
            var text = message.Data.ToStringUtf8();
            logger.LogDebug("MessagingConsumerPubsub ConsumeMessage Start: {info}",
                $"Topic= {GetFullTopic()}, Message= {text}, {ShowAttributeMessage(message)}");

            sw.Start();
            metrics.SetCounter(GetMetricNameCounter());

            if (OnReceiveMessage != null)
                await OnReceiveMessage(originalKey, text);

            sw.Stop();
            logger.LogInformation("MessagingConsumerPubsub ConsumeMessage Success : {info}",
                $"Key {originalKey}, Topic= {GetFullTopic()}, Message= {text} Duration: {sw.ElapsedMilliseconds} ms");
            metrics.SetHistogram(GetMetricNameHistogramSuccess(), sw.Elapsed.TotalSeconds);

            startAsyncActivity?.SetTag("topic", FullTopic);
            startAsyncActivity?.SetTag("messageId", originalKey);
            startAsyncActivity?.SetTag("groupId", FullGroupId);
            
            if (_keepMessageDeadLetter) 
                await KeepDeadLetterLog(message);
            else 
                await DeleteLog(message);
           
            return await Task.FromResult(SubscriberClient.Reply.Ack);
        }
        catch (Exception e)
        {
            sw.Stop();
            logger.LogError(
                "MessagingConsumerPubsub ConsumeMessage Failed: {info}",
                $"Key= {originalKey}, Topic= {GetFullTopic()}, Message=  {message.Data.ToStringUtf8()} Duration: {sw.ElapsedMilliseconds} ms Error: {e.Message}");
            metrics.SetHistogram(GetMetricNameHistogramFailed(), sw.Elapsed.TotalSeconds);
            SetActivityOnError(startAsyncActivity, e);
            
            await InsertLog(message, originalKey, e);

            return await Task.FromResult(SubscriberClient.Reply.Nack);
        }finally
        {
            _semaphore?.Release();
            logger.LogInformation("MessagingConsumerPubsub ConsumeMessage: {info}", $"Semaphore release for  {originalKey}. Available slot: {_semaphore?.CurrentCount}");
        }
    }

    private void RecheckMetric()
    {
        if (!metrics.IsMetricHistogramExist(GetMetricNameHistogramSuccess()))
        {
            logger.LogInformation($"Metric histogram missing, try to re-init.");
            metrics.InitHistogram(GetMetricNameHistogramSuccess(),
                     GetMetricNameHistogramSuccess(),GetBucketHistogram());
            metrics.InitHistogram(GetMetricNameHistogramFailed(),
                     GetMetricNameHistogramFailed(),GetBucketHistogram());
        }

        if (!metrics.IsMetricCounterExist(GetMetricNameCounter()))
        {
            logger.LogInformation($"Metric counter missing, try to re-init.");
            metrics.InitCounter(GetMetricNameCounter(),
              GetMetricNameCounter());
        }
    }

    private async Task DeleteLog(PubsubMessage message)
    {
        var originalKey = GetOriginalKey(message);
        try
        {
            await consumerLog.DeleteAsync(originalKey);
        }
        catch (Exception e)
        {
            logger.LogError("MessagingConsumerPubsub DeleteLog Failed: {info}", $"Key= {originalKey} Error= {e.Message}");
        }
    }

    private async Task InsertLog(PubsubMessage message, string originalKey, Exception e)
    {
        try
        {
            var logData = await consumerLog.GetAsync(originalKey);
            if (logData == null)
            {
                await consumerLog.InsertAsync(new Message<object>
                {
                    Key = originalKey,
                    PayLoad = message.Data.ToStringUtf8(),
                    Method = "consumer",
                    Error = e.Message + e.StackTrace,
                    GroupId = GetFullGroupId(),
                    Topic = GetFullTopic(),
                    Retry = 0,
                    AppId = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = "RETRY"
                });
            } else {
                
                logData.Retry++;
                logData.Error = e.Message + e.StackTrace;
                logData.UpdatedAt = DateTime.Now;
                logData.Status = "RETRY";
                await consumerLog.UpdateAsync(originalKey, logData);
            }
        }
        catch (Exception exception)
        {
            logger.LogError("MessagingConsumerPubsub InsertLog Failed: {info}", $"Key= {originalKey} Error= {exception.Message}");
        }
        
    }

    private  string GetOriginalKey(PubsubMessage message)
    {
        try
        { 
            if (!message.Attributes.ContainsKey("OriginalKey"))
            {
                return message.MessageId;
            }
            var originalKey = message.Attributes["OriginalKey"] ?? message.MessageId;
            return originalKey;
        }
        catch (Exception e)
        {
            logger.LogError("MessagingConsumerPubsub GetOriginalKey: {info}", $"Error= {e.Message}");
            return message.MessageId;
        }
    }

    private string ShowAttributeMessage(PubsubMessage message)
    {
        if (message.Attributes != null)
        {
            var attributeString = new StringBuilder();
            foreach (var attribute in message.Attributes)
            {
                attributeString.Append($"{attribute.Key} = {attribute.Value}, ");
            }
            return $"Attributes: {attributeString}";
        }
        return "";
    }

    private async Task KeepDeadLetterLog(PubsubMessage message)
    {
        var originalKey = GetOriginalKey(message);
        try
        {
            var appId = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
            
            var logData = await consumerLog.GetAsync(originalKey);
            if (logData != null)
            {
                logData.Status = "DEAD";
                logData.UpdatedAt = DateTime.Now;
                await consumerLog.UpdateAsync(originalKey, logData);
                return;
            } 
            
            await consumerLog.InsertAsync(new Message<object>
            {
                Key = originalKey,
                PayLoad = message.Data.ToStringUtf8(),
                Method = "consumer",
                Error = "",
                GroupId = GetFullGroupId(),
                Topic = GetFullTopic(),
                Retry = 0,
                AppId = appId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = "DEAD"
            });
            
            logger.LogDebug("MessagingConsumerPubsub KeepDeadLetterLog: {info}", $"Key= {originalKey} Flagged as DEAD");
        }
        catch (Exception exception)
        {
            logger.LogError("MessagingConsumerPubsub KeepDeadLetterLog: {info}", $"Key= {originalKey} Error= {exception.Message}");
        }
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