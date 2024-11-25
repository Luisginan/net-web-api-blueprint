using Blueprint.CustomerModule.Models;
using Core.Utils.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace Blueprint.CustomerModule.Controllers;
[Route("blueprint/[controller]")]
[ApiController]
public class MessagingController( IMessagingProducer producer, IProducerTopicManager producerTopicManager): ControllerBase
{
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CustomerPayload value)
    {
        var topic = producerTopicManager.GetTopicId("top");
        await producer.Produce(topic, Guid.NewGuid().ToString(), value);

        return Accepted();
    }
    
}