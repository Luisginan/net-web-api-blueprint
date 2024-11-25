using Blueprint.CustomerModule.Controllers;
using Blueprint.CustomerModule.Models;
using Core.Utils.Messaging;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BlueprintTest.MessagingControllerTest;

public class MessagingControllerPostTest
{
    [Fact]
    public async Task Post_Success()
    {
        var mockMessagingProducer = new Mock<IMessagingProducer>();
        var mockTopicManager = new Mock<IProducerTopicManager>();

        var request = new CustomerPayload
        {
            Id = "001",
            Message = "Helo"
        };
        
        mockTopicManager.Setup(x => x.GetTopicId(It.IsAny<string>()))
            .Returns("top");

        var controller = new MessagingController( mockMessagingProducer.Object, mockTopicManager.Object);

        var result = await controller.Post(request);

        var okResult = Assert.IsType<AcceptedResult>(result);
        
        Assert.Equal(202, okResult.StatusCode);
        mockTopicManager.Verify(x => x.GetTopicId("top"), Times.Once);
        mockMessagingProducer.Verify(x => x.Produce("top", It.IsAny<string>(), request, It.IsAny<string>()), Times.Once);
        
    }
}