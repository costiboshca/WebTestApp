using Microsoft.AspNetCore.Mvc;
using WebTestApp.Models;
using WebTestApp.Services;

namespace WebTestApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = _auth.Authenticate(request);
        return result is null ? Unauthorized("Invalid credentials.") : Ok(result);
    }
}
