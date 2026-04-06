using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebTestApp.Data;
using WebTestApp.Models;

namespace WebTestApp.Services;

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null) return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed) return null;

        var jwt    = config.GetSection("JwtSettings");
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiryMinutes"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Role,           user.Role)
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
