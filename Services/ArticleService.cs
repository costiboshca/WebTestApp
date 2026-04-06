using Microsoft.EntityFrameworkCore;
using WebTestApp.Data;
using WebTestApp.Models;

namespace WebTestApp.Services;

public class ArticleService(AppDbContext db) : IArticleService
{
    public async Task<IReadOnlyList<Article>> GetAllAsync() =>
        await db.Articles.OrderBy(a => a.Code).ToListAsync();

    public async Task<Article?> GetByIdAsync(Guid id) =>
        await db.Articles.FindAsync(id);

    public async Task<Article> CreateAsync(ArticleRequest request)
    {
        var article = new Article
        {
            Code        = request.Code,
            Description = request.Description,
            ProductCode = request.ProductCode
        };
        db.Articles.Add(article);
        await db.SaveChangesAsync();
        return article;
    }

    public async Task<Article?> UpdateAsync(Guid id, ArticleRequest request)
    {
        var article = await db.Articles.FindAsync(id);
        if (article is null) return null;

        article.Code        = request.Code;
        article.Description = request.Description;
        article.ProductCode = request.ProductCode;
        await db.SaveChangesAsync();
        return article;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var article = await db.Articles.FindAsync(id);
        if (article is null) return false;
        db.Articles.Remove(article);
        await db.SaveChangesAsync();
        return true;
    }
}
