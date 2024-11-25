using Blueprint.RecoveryMessage.Controllers;
using Blueprint.RecoveryMessage.Services;
using Core.Utils.DB;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BlueprintTest.RecoveryMessageControllerTest;

public class RecoveryMessageControllerTest
{
    [Fact]
    public async void GetRecoveryMessage_Success()
    {
        var recoveryMessageService = new Mock<IRecoveryMessageService>();
        var recoveryController = new RecoveryMessageController(recoveryMessageService.Object);

        recoveryMessageService.Setup(x => x.GetRecoveryMessages())
            .ReturnsAsync([
                new Message<string>()
                {
                    Key = "key1", Topic = "topic1", PayLoad = "payload1", Method = "method1", CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now, Status = "status1", Retry = 0, Error = "error1", GroupId = "groupId1",
                    AppId = "appId1"
                },

                new Message<string>()
                {
                    Key = "key2", Topic = "topic2", PayLoad = "payload2", Method = "method2", CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now, Status = "status2", Retry = 0, Error = "error2", GroupId = "groupId2",
                    AppId = "appId2"
                }
            ]);

        var result = await recoveryController.GetRecoveryMessages();
        
        Assert.NotNull(result);
        var response = Assert.IsType<ActionResult<List<Message<string>>>>(result);
        var okObjectResult = response.Result as OkObjectResult;
        var listMessage = Assert.IsType<List<Message<string>>>(okObjectResult?.Value);
        Assert.Equal(2, listMessage.Count);
        recoveryMessageService.Verify(x => x.GetRecoveryMessages(), Times.Once);
    }
    
    [Fact]
    public async void GetRecoveryMessage_Empty()
    {
        var recoveryMessageService = new Mock<IRecoveryMessageService>();
        var recoveryController = new RecoveryMessageController(recoveryMessageService.Object);

        recoveryMessageService.Setup(x => x.GetRecoveryMessages())
            .ReturnsAsync(new List<Message<string>>());

        var result = await recoveryController.GetRecoveryMessages();
        
        Assert.NotNull(result);
        var response = Assert.IsType<ActionResult<List<Message<string>>>>(result);
        var okObjectResult = response.Result as OkObjectResult;
        var listMessage = Assert.IsType<List<Message<string>>>(okObjectResult?.Value);
        Assert.Empty(listMessage);
        recoveryMessageService.Verify(x => x.GetRecoveryMessages(), Times.Once);
    }
    
    [Fact]
    public async void UpdateRecoveryMessage_Success()
    {
        var recoveryMessageService = new Mock<IRecoveryMessageService>();
        var recoveryController = new RecoveryMessageController(recoveryMessageService.Object);

        var key = "key1";
        var message = "message1";
        recoveryMessageService.Setup(x => x.UpdateMessage(key, message));

        var result = await recoveryController.UpdateMessage(key, message);
        
        Assert.NotNull(result);
        var response = Assert.IsAssignableFrom<ActionResult>(result);
        var okObjectResult = response as OkResult;
        Assert.NotNull(okObjectResult);
        recoveryMessageService.Verify(x => x.UpdateMessage(key, message), Times.Once);
    }
    
    [Fact]
    public async void ResendRecoveryMessage_Success()
    {
        var recoveryMessageService = new Mock<IRecoveryMessageService>();
        var recoveryController = new RecoveryMessageController(recoveryMessageService.Object);

        var key = "key1";
        recoveryMessageService.Setup(x => x.ResendMessage(key));

        var result = recoveryController.ResendMessage(key);
        
        Assert.NotNull(result);
        var response = Assert.IsAssignableFrom<ActionResult>(result);
        var okObjectResult = response as OkResult;
        Assert.NotNull(okObjectResult);
        recoveryMessageService.Verify(x => x.ResendMessage(key), Times.Once);
    }
    
}