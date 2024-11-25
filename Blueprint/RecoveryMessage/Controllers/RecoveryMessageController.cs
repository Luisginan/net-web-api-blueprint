using Blueprint.RecoveryMessage.Services;
using Core.Utils.DB;
using Microsoft.AspNetCore.Mvc;

namespace Blueprint.RecoveryMessage.Controllers;
[ApiController]
[Route("recovery-message")]
public class RecoveryMessageController(IRecoveryMessageService recoveryMessageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Message<string>>>> GetRecoveryMessages()
    {
        var result =  await recoveryMessageService.GetRecoveryMessages();
        return Ok(result);
    }

    [HttpPut("{key}")]
    public async Task<ActionResult> UpdateMessage(string key, [FromBody] string message)
    {
        await recoveryMessageService.UpdateMessage(key, message);
        return Ok();
    }

    [HttpPost("{key}/resend")]
    public ActionResult ResendMessage(string key)
    {
        recoveryMessageService.ResendMessage(key);
        return Ok();
    }
}