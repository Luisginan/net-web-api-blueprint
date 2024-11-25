using Blueprint.RecoveryMessage.Services;
using Core.Utils.DB;
using Core.Utils.Messaging;
using Moq;

namespace BlueprintTest.RecoveryMessageTest;

public class RecoveryMessageGetTest
{
    [Fact]
    public async void GetRecoveryMessages_WhenCalled_ReturnRecoveryMessages()
    {
        // Arrange
        var messagingProducer = new Mock<IMessagingProducer>();
        var consumerLog = new Mock<IConsumerLog>();
        var consumerTopicManager = new Mock<IConsumerTopicManager>();
        var recoveryMessageService = new RecoveryMessageService(messagingProducer.Object, consumerLog.Object, consumerTopicManager.Object);

        var listTopic = new List<string> { "topic1", "topic2" };
        consumerTopicManager.Setup(x => x.GetListTopic()).Returns(listTopic);
        
        consumerLog.Setup(x => x.GetListAsync(It.IsAny<List<string>>()))
            .ReturnsAsync([
                new Message<object>
                {
                    Key = "key1", Topic = "topic1", PayLoad = "payload1", Method = "method1", CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now, Status = "status1", Retry = 0, Error = "error1", GroupId = "groupId1",
                    AppId = "appId1"
                },
                new Message<object>
                {
                    Key = "key2", Topic = "topic2", PayLoad = "payload2", Method = "method2", CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now, Status = "status2", Retry = 0, Error = "error2", GroupId = "groupId2",
                    AppId = "appId2"
                }
            ]);


        // Act
        var result = await recoveryMessageService.GetRecoveryMessages();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        consumerTopicManager.Verify(x => x.GetListTopic(), Times.Once);
        consumerLog.Verify(x => x.GetListAsync(listTopic), Times.Once);
    }
    
    [Fact]
    public async Task GetRecoversMessages_WhenCalled_ReturnEmptyMessage()
    {
        // Arrange
        var messagingProducer = new Mock<IMessagingProducer>();
        var consumerLog = new Mock<IConsumerLog>();
        var consumerTopicManager = new Mock<IConsumerTopicManager>();
        var recoveryMessageService = new RecoveryMessageService(messagingProducer.Object, consumerLog.Object, consumerTopicManager.Object);

        var listTopic = new List<string> { "topic1", "topic2" };
        consumerTopicManager.Setup(x => x.GetListTopic()).Returns(listTopic);
        
        consumerLog.Setup(x => x.GetListAsync(It.IsAny<List<string>>()))
            .ReturnsAsync([]);

        // Act
        var result = await recoveryMessageService.GetRecoveryMessages();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        consumerTopicManager.Verify(x => x.GetListTopic(), Times.Once);
        consumerLog.Verify(x => x.GetListAsync(listTopic), Times.Once);
    }

    [Fact]
    public async Task GetRecoverMessage_WhenCalled_ListTopicEmpty()
    {
        // Arrange
        var messagingProducer = new Mock<IMessagingProducer>();
        var consumerLog = new Mock<IConsumerLog>();
        var consumerTopicManager = new Mock<IConsumerTopicManager>();
        var recoveryMessageService = new RecoveryMessageService(messagingProducer.Object, consumerLog.Object, consumerTopicManager.Object);

        var listTopic = new List<string> { };
        consumerTopicManager.Setup(x => x.GetListTopic()).Returns(listTopic);
        
        consumerLog.Setup(x => x.GetListAsync(It.IsAny<List<string>>()))
            .ReturnsAsync([]);

        // Act
        var result = await recoveryMessageService.GetRecoveryMessages();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        consumerTopicManager.Verify(x => x.GetListTopic(), Times.Once);
        consumerLog.Verify(x => x.GetListAsync(listTopic), Times.Once);
    }
}