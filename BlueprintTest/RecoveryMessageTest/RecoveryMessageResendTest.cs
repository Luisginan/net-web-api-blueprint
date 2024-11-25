using Blueprint.RecoveryMessage.Services;
using Core.Utils.DB;
using Core.Utils.Messaging;
using Moq;

namespace BlueprintTest.RecoveryMessageTest;

public class RecoveryMessageResendTest
{
    [Fact]
    public async void ResendRecovery_WhenCalled_Success()
    {
        // Arrange
        var messagingProducer = new Mock<IMessagingProducer>();
        var consumerLog = new Mock<IConsumerLog>();
        var consumerTopicManager = new Mock<IConsumerTopicManager>();
        var recoveryMessageService = new RecoveryMessageService(messagingProducer.Object, consumerLog.Object, consumerTopicManager.Object);

        var key = "key1";
        var message = "message1";
        consumerLog.Setup(x => x.GetAsync(key))
            .ReturnsAsync(new Message<object>
            {
                Key = key, Topic = "topic1", PayLoad = message, Method = "method1", CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now, Status = "status1", Retry = 0, Error = "error1", GroupId = "groupId1",
                AppId = "appId1"
            });

        // Act
        recoveryMessageService.ResendMessage(key);

        // Assert
        consumerLog.Verify(x => x.GetAsync(key), Times.Once);
        messagingProducer.Verify(x => x.Produce("topic1", key, message, It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public void ResendRecovery_failed_MessageNotFound()
    {
        // Arrange
        var messagingProducer = new Mock<IMessagingProducer>();
        var consumerLog = new Mock<IConsumerLog>();
        var consumerTopicManager = new Mock<IConsumerTopicManager>();
        var recoveryMessageService = new RecoveryMessageService(messagingProducer.Object, consumerLog.Object, consumerTopicManager.Object);

        var key = "key1";
        consumerLog.Setup(x => x.GetAsync(key))
            .ReturnsAsync((Message<object>) null);

        // Act
        var ex = Assert.Throws<Exception>(() =>  recoveryMessageService.ResendMessage(key));

        // Assert
        Assert.Equal("Message not found", ex.Message);
        consumerLog.Verify(x => x.GetAsync(key), Times.Once);
        messagingProducer.Verify(x => x.Produce(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ResendRecovery_failed_payloadIsNull()
    {
        // Arrange
        var messagingProducer = new Mock<IMessagingProducer>();
        var consumerLog = new Mock<IConsumerLog>();
        var consumerTopicManager = new Mock<IConsumerTopicManager>();
        var recoveryMessageService = new RecoveryMessageService(messagingProducer.Object, consumerLog.Object, consumerTopicManager.Object);

        var key = "key1";
        consumerLog.Setup(x => x.GetAsync(key))
            .ReturnsAsync(new Message<object>
            {
                Key = key, Topic = "topic1", PayLoad = null, Method = "method1", CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now, Status = "status1", Retry = 0, Error = "error1", GroupId = "groupId1",
                AppId = "appId1"
            });

        // Act
        var ex = Assert.Throws<Exception>(() =>  recoveryMessageService.ResendMessage(key));

        // Assert
        Assert.Equal("Payload is null", ex.Message);
        consumerLog.Verify(x => x.GetAsync(key), Times.Once);
        messagingProducer.Verify(x => x.Produce(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public void ResendRecovery_Failed_MessageEmpty()
    {
        // Arrange
        var messagingProducer = new Mock<IMessagingProducer>();
        var consumerLog = new Mock<IConsumerLog>();
        var consumerTopicManager = new Mock<IConsumerTopicManager>();
        var recoveryMessageService = new RecoveryMessageService(messagingProducer.Object, consumerLog.Object, consumerTopicManager.Object);

        var key = "key1";
        consumerLog.Setup(x => x.GetAsync(key))
            .ReturnsAsync(new Message<object>
            {
                Key = key, Topic = "topic1", PayLoad = "", Method = "method1", CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now, Status = "status1", Retry = 0, Error = "error1", GroupId = "groupId1",
                AppId = "appId1"
            });

        // Act
        var ex = Assert.Throws<Exception>(() =>  recoveryMessageService.ResendMessage(key));

        // Assert
        Assert.Equal("Message is empty", ex.Message);
        consumerLog.Verify(x => x.GetAsync(key), Times.Once);
        messagingProducer.Verify(x => x.Produce(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}