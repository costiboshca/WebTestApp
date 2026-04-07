using WebTestApp.Models;

namespace WebTestApp.Services;

public interface IChatService
{
    Task<string> ChatAsync(List<ChatMessage> history);
}
