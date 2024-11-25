using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Models;
using Core.Base;
using Core.Utils.Messaging;
using Newtonsoft.Json;

namespace Blueprint.CustomerModule.MsgConsumer;

[ExcludeFromCodeCoverage]
public class SyncCustomer(ILogger<SyncCustomer> logger,
    ILogger<ConsumerBase<CustomerPayload>> baseLogger,
    IConsumerTopicManager consumerTopicManager,
    IMessagingConsumer messagingConsumer) :
    ConsumerBase<CustomerPayload>(baseLogger, messagingConsumer)
{

    protected override string GetGroupId()
    {
        return consumerTopicManager.GetGroupId("top");
    }

    protected override string GetTopic()
    {
        return consumerTopicManager.GetTopicId("top");
    }


    protected override Task OnReceiveMessage(string key, CustomerPayload message)
    {
        return Task.Run(() =>
        {
            var messageString = JsonConvert.SerializeObject(message);
            logger.LogInformation("SyncCustomer OnReceiveMessage: {info}", "message:" + messageString);
          
            //throw new Exception("simulated exception");
            Task.Delay(5000).Wait();
                
        });
    }
}