using WebTestApp.Models;

namespace WebTestApp.Services;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request);
}
