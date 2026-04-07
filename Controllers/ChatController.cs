using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebTestApp.Models;
using WebTestApp.Services;

namespace WebTestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(IChatService chat) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest req)
    {
        if (req.Messages is null || req.Messages.Count == 0)
            return BadRequest("Messages list is required.");

        var reply = await chat.ChatAsync(req.Messages);
        return Ok(new ChatResponse(reply));
    }
}
