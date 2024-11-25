using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Core.Utils.Messaging;
using Microsoft.Extensions.Options;
using Core.Config;
using Core.Utils.DB;
using Core.Utils.Security;
using Microsoft.Extensions.Logging;
using Bogus;
using Newtonsoft.Json;

namespace CoreIntegrationTest.Utils.Messaging;

public class MessagingProducerTest
{
    private const string Topic = "top";

    //preparation data
    [Fact]
    public async Task Produce_WithoutSuffixTest()
    {
        using var messagingProducer = PreparationObject();

        var message = GenerateMessage();

        await messagingProducer.Produce(Topic, message.Id, JsonConvert.SerializeObject(message));

        // wait kafka sending message
        await Task.Delay(2000);
            
        Assert.Equal(Topic,messagingProducer.GetTopic());
        Assert.True(messagingProducer.IsSuccess());
    }

    [Fact]
    public async Task Produce_WithSuffixTest()
    {
        const string suffix = "sit";
        using var messagingProducer = PreparationObject(suffix);

        var message = GenerateMessage();

        await messagingProducer.Produce(Topic, message.Id, JsonConvert.SerializeObject(message));

        // wait for kafka to send message
        await Task.Delay(2000);

        // Verify that the topic has been suffixed
        Assert.Equal($"{Topic}-{suffix}", messagingProducer.GetTopic());
        Assert.True(messagingProducer.IsSuccess());
    }

    private static TopModel GenerateMessage()
    {
        var faker = new Faker();
        var key = Guid.NewGuid().ToString();
        var message = new TopModel
        {
            Id = key,
            Message = faker.Address.FullAddress()
        };
        return message;
    }

    private static MessagingProducer PreparationObject(string suffix = "")
    {
        MessagingProducer messagingProducer = null;
        try
        {
            var logger = new Mock<ILogger<MessagingProducer>>();
            var producerLog = new Mock<IProducerLog>();
            var options = new Mock<IOptions<MessagingConfig>>();
            var vault = new Mock<IVault>();
            

            vault.Setup(m => m.RevealSecret(It.IsAny<object>())).Returns(
                new MessagingConfig
                {
                    BootstrapServers = "localhost:29092",
                    SaslPassword = "",
                    SaslUsername = "",
                    Authentication = false,
                    Provider = "kafka",
                    TopicSuffix = suffix,
                    ProjectId = "",
                    SessionTimeoutMs = 30
                });
            
            messagingProducer = new MessagingProducer(logger.Object, options.Object, producerLog.Object, vault.Object);
            return messagingProducer;
        }
        catch
        {
            messagingProducer?.Dispose();
            throw;
        }
    }
}