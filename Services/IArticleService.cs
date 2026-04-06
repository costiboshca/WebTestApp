using WebTestApp.Models;

namespace WebTestApp.Services;

public interface IArticleService
{
    Task<IReadOnlyList<Article>> GetAllAsync();
    Task<Article?>               GetByIdAsync(Guid id);
    Task<Article>                CreateAsync(ArticleRequest request);
    Task<Article?>               UpdateAsync(Guid id, ArticleRequest request);
    Task<bool>                   DeleteAsync(Guid id);
}
