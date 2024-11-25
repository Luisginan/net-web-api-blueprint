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

public class MessagingConsumerTest
{
    [Fact]
    public async Task ConsumerReceiveMessage()
    {
        var logger = new Mock<ILogger<MessagingConsumerKafka>>();
        var options = new Mock<IOptions<MessagingConfig>>();
        var metrics = new Mock<IMetrics>();
        var consumerLog = new Mock<IConsumerLog>();
        var vault = new Mock<IVault>();
        
        vault.Setup(x => x.RevealSecret(It.IsAny<MessagingConfig>())).Returns(GetMessagingConfig());
        
        var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;
        var consumerKafka = new MessagingConsumerKafka(logger.Object,options.Object, consumerLog.Object, metrics.Object, vault.Object);
        
        var loggerProducer = new Mock<ILogger<MessagingProducer>>();
        var producerLog = new Mock<IProducerLog>();
        
        var producer = new MessagingProducer(loggerProducer.Object, options.Object, producerLog.Object,  vault.Object);
        var consumerKafkaTopic = "top";
        consumerKafka.Topic = consumerKafkaTopic;
        consumerKafka.GroupId = "top-sub";

        var model = new TopModel
        {
            Id = "0001",
            Message = "halo"
        };
        var serializeObject = JsonConvert.SerializeObject(model);

        var resultMessage = "";
        var newKey = Guid.NewGuid().ToString();
        
        await producer.Produce(consumerKafkaTopic, newKey, serializeObject);
        
        consumerKafka.OnReceiveMessage += (_, message) =>
        {
            resultMessage = message;
            cancellationSource.Cancel();
            return Task.CompletedTask;
        };
        
        await consumerKafka.Listening(cancellationToken);
        
        Assert.Equal(serializeObject, resultMessage);
    }

    private static MessagingConfig GetMessagingConfig()
    {
        return new MessagingConfig
        {
            BootstrapServers = "localhost:29092",
            SaslPassword = "",
            SaslUsername = "",
            Authentication = false,
            Provider = "kafka",
            TopicSuffix = "",
            ProjectId = "",
            SessionTimeoutMs = 30
        };
    }
}