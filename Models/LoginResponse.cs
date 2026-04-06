namespace WebTestApp.Models;

public record LoginResponse(string Token, DateTime ExpiresAt);
