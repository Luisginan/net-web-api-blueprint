using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Config;
using Core.Systems;
using Core.Utils.DB;
using Core.Utils.Messaging;
using Core.Utils.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CoreIntegrationTest.Utils.Messaging;

public class MessagingConsumerPubsubTest
{
    [Fact]
    public async Task ConsumerReceiveMessage()
    {
        var logger = new Mock<ILogger<MessagingConsumerPubsub>>();
        var options = new Mock<IOptions<MessagingConfig>>();
        var metrics = new Mock<IMetrics>();
        var vault = new Mock<IVault>();
        var consumerLog = new Mock<IConsumerLog>();
        vault.Setup(x => x.RevealSecret(It.IsAny<MessagingConfig>())).Returns(GetMessagingConfig());
        
        var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;
        var consumerPubsub = new MessagingConsumerPubsub(logger.Object,options.Object, metrics.Object, vault.Object, consumerLog.Object);
        
        var loggerProducer = new Mock<ILogger<MessagingProducerPubsub>>();
        var producerLog = new Mock<IProducerLog>();
        
        var producer = new MessagingProducerPubsub(loggerProducer.Object, options.Object, producerLog.Object, vault.Object);
        var topic = "top";
        consumerPubsub.Topic = topic;
        consumerPubsub.GroupId = "top-sub";
        
        var model = new TopModel
        {
            Id = "0001",
            Message = "halo"
        };
        var serializeObject = JsonConvert.SerializeObject(model);

        var resultMessage = "";
        var newKey = Guid.NewGuid().ToString();
        
        await producer.Produce(topic, newKey, serializeObject);
        
        consumerPubsub.OnReceiveMessage += async (_, message) =>
        {
            resultMessage = message;
            await consumerPubsub.StopListening();
        };
        
        await consumerPubsub.Listening(cancellationToken);
        
        Assert.Equal(serializeObject, resultMessage);
    }

    private static MessagingConfig GetMessagingConfig()
    {
        return new MessagingConfig
        {
            BootstrapServers = "",
            SaslPassword = "",
            SaslUsername = "",
            Authentication = false,
            Provider = "pubsub",
            TopicSuffix = "",
            ProjectId = "nds-oneloan-dev",
            SessionTimeoutMs = 30
        };
    }
}