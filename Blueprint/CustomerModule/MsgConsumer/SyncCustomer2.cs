using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Models;
using Core.Base;
using Core.Utils.Messaging;
using Newtonsoft.Json;

namespace Blueprint.CustomerModule.MsgConsumer;

[ExcludeFromCodeCoverage]
public class SyncCustomer2(ILogger<SyncCustomer2> logger,
    ILogger<ConsumerBase2<CustomerPayload>> baseLogger,
    IMessagingConsumer messagingConsumer,
    IConsumerTopicManager consumerTopicManager,
    IMessagingConsumerDlq deadLetterConsumer
    
) :
    ConsumerBase2<CustomerPayload>(baseLogger, messagingConsumer, deadLetterConsumer)
{

    protected override string GetTopic()
    {
        return consumerTopicManager.GetTopicId("top");
    }
    protected override string GetGroupId()
    {
        return consumerTopicManager.GetGroupId("top");
    }

    protected override string GetDeadLetterTopic()
    {
        return consumerTopicManager.GetDeadLetterTopicId("top");
    }

    protected override string GetDeadLetterGroupId()
    {
        return consumerTopicManager.GetDeadLetterGroupId("top");
    }

    protected override Task OnReceiveMessage(string key, CustomerPayload message)
    {
        return Task.Run(() =>
        {
            var messageString = JsonConvert.SerializeObject(message);
            logger.LogInformation("SyncCustomer OnReceiveMessage: {info}", "message:" + messageString);
            //throw new Exception("simulated exception 2");
                
        });
    }
    
    protected override async Task OnDeadLetterReceiveMessage(string key, CustomerPayload message)
    {
        await Task.Run(() =>
        {
            var messageString = JsonConvert.SerializeObject(message);
            logger.LogInformation("SyncCustomer OnDeadLetterReceiveMessage: {info}", "message:" + messageString);
        });
    }
}