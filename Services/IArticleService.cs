using WebTestApp.Models;

namespace WebTestApp.Services;

public interface IArticleService
{
    IReadOnlyList<Article> GetAll();
    Article? GetById(Guid id);
    Article Create(ArticleRequest request);
    Article? Update(Guid id, ArticleRequest request);
    bool Delete(Guid id);
}
