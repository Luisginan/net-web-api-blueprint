using Blueprint.RecoveryMessage.Services;
using Core.Utils.DB;
using Core.Utils.Messaging;
using Moq;

namespace BlueprintTest.RecoveryMessageTest;

public class RecoveryMessageUpdateTest
{
    [Fact]
    public async void UpdateRecovery_WhenCalled_Success()
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
                Key = key, Topic = "topic1", PayLoad = "payload1", Method = "method1", CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now, Status = "status1", Retry = 0, Error = "error1", GroupId = "groupId1",
                AppId = "appId1"
            });

        // Act
        await recoveryMessageService.UpdateMessage(key, message);

        // Assert
        consumerLog.Verify(x => x.GetAsync(key), Times.Once);
        consumerLog.Verify(x => x.UpdateAsync(key, It.IsAny<Message<object>>()), Times.Once);
    }
    
    [Fact]
    public async void UpdateRecovery_failed_MessageNotFound()
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
        var ex = await Assert.ThrowsAsync<Exception>(() => recoveryMessageService.UpdateMessage(key, "message1"));

        // Assert
        Assert.Equal("Message not found", ex.Message);
        consumerLog.Verify(x => x.GetAsync(key), Times.Once);
        consumerLog.Verify(x => x.UpdateAsync(key, It.IsAny<Message<object>>()), Times.Never);
    }
}