using System.Collections.Concurrent;
using WebTestApp.Models;

namespace WebTestApp.Services;

public class CompanyService : ICompanyService
{
    private readonly ConcurrentDictionary<Guid, Company> _store = new();

    public IReadOnlyList<Company> GetAll() =>
        _store.Values.OrderBy(c => c.Name).ToList();

    public Company? GetById(Guid id) =>
        _store.TryGetValue(id, out var company) ? company : null;

    public Company Create(CompanyRequest request)
    {
        var company = new Company
        {
            Name        = request.Name,
            Description = request.Description,
            Address     = request.Address
        };
        _store[company.Id] = company;
        return company;
    }

    public Company? Update(Guid id, CompanyRequest request)
    {
        if (!_store.TryGetValue(id, out var company))
            return null;

        company.Name        = request.Name;
        company.Description = request.Description;
        company.Address     = request.Address;
        return company;
    }

    public bool Delete(Guid id) => _store.TryRemove(id, out _);

    private readonly object _articleLock = new();

    public bool AddArticle(Guid companyId, Guid articleId)
    {
        if (!_store.TryGetValue(companyId, out var company)) return false;
        lock (_articleLock) { return company.ArticleIds.Add(articleId); }
    }

    public bool RemoveArticle(Guid companyId, Guid articleId)
    {
        if (!_store.TryGetValue(companyId, out var company)) return false;
        lock (_articleLock) { return company.ArticleIds.Remove(articleId); }
    }

    public IReadOnlySet<Guid>? GetArticleIds(Guid companyId) =>
        _store.TryGetValue(companyId, out var company) ? company.ArticleIds : null;
}
