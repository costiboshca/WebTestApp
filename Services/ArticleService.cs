using System.Collections.Concurrent;
using WebTestApp.Models;

namespace WebTestApp.Services;

public class ArticleService : IArticleService
{
    private readonly ConcurrentDictionary<Guid, Article> _store = new();

    public IReadOnlyList<Article> GetAll() =>
        _store.Values.OrderBy(a => a.Code).ToList();

    public Article? GetById(Guid id) =>
        _store.TryGetValue(id, out var article) ? article : null;

    public Article Create(ArticleRequest request)
    {
        var article = new Article
        {
            Code        = request.Code,
            Description = request.Description,
            ProductCode = request.ProductCode
        };
        _store[article.Id] = article;
        return article;
    }

    public Article? Update(Guid id, ArticleRequest request)
    {
        if (!_store.TryGetValue(id, out var article))
            return null;

        article.Code        = request.Code;
        article.Description = request.Description;
        article.ProductCode = request.ProductCode;
        return article;
    }

    public bool Delete(Guid id) => _store.TryRemove(id, out _);
}
