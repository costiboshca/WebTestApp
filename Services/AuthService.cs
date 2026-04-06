using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebTestApp.Models;

namespace WebTestApp.Services;

public class AuthService : IAuthService
{
    private static readonly Dictionary<string, string> Users = new()
    {
        { "admin", "password123" },
        { "user",  "letmein"    }
    };

    private readonly IConfiguration _config;

    public AuthService(IConfiguration config) => _config = config;

    public LoginResponse? Authenticate(LoginRequest request)
    {
        if (!Users.TryGetValue(request.Username, out var storedPassword)
            || storedPassword != request.Password)
            return null;

        var jwt    = _config.GetSection("JwtSettings");
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiryMinutes"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, request.Username),
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, request.Username == "admin" ? "Admin" : "User")
        };

        var token = new JwtSecurityToken(
            issuer:             jwt["Issuer"],
            audience:           jwt["Audience"],
            claims:             claims,
            expires:            expiry,
            signingCredentials: creds);

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }
}
