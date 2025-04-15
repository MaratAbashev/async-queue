using Microsoft.AspNetCore.Mvc;

namespace Broker.Controllers;
[Route("/broker")]
public class ProducerController: Controller
{
    [HttpPost("/produce")]
    public IActionResult Produce([FromBody] RequestMessageDto requestMessage)
    {
        
        return Ok(Guid.NewGuid());
    }

    [HttpGet("/get")]
    public IActionResult Get()
    {
        return Ok();
    }
}

public record RequestMessageDto(string Content, DateTime CreatedAt);
public record ResponseMessageDto(string Content, DateTime CreatedAt);