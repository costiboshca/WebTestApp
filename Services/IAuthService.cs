using WebTestApp.Models;

namespace WebTestApp.Services;

public interface IAuthService
{
    LoginResponse? Authenticate(LoginRequest request);
}
