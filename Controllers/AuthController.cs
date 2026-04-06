using Microsoft.AspNetCore.Mvc;
using WebTestApp.Models;
using WebTestApp.Services;

namespace WebTestApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await auth.AuthenticateAsync(request);
        return result is null ? Unauthorized("Invalid credentials.") : Ok(result);
    }
}
