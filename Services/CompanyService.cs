using Microsoft.EntityFrameworkCore;
using WebTestApp.Data;
using WebTestApp.Models;

namespace WebTestApp.Services;

public class CompanyService(AppDbContext db) : ICompanyService
{
    public async Task<IReadOnlyList<CompanyResponse>> GetAllAsync() =>
        await db.Companies
            .Include(c => c.Articles)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyResponse(
                c.Id, c.Name, c.Description, c.Address,
                c.Articles.Select(a => a.Id)))
            .ToListAsync();

    public async Task<CompanyResponse?> GetByIdAsync(Guid id)
    {
        var c = await db.Companies.Include(c => c.Articles).FirstOrDefaultAsync(c => c.Id == id);
        return c is null ? null
            : new CompanyResponse(c.Id, c.Name, c.Description, c.Address, c.Articles.Select(a => a.Id));
    }

    public async Task<CompanyResponse> CreateAsync(CompanyRequest request)
    {
        var company = new Company
        {
            Name        = request.Name,
            Description = request.Description,
            Address     = request.Address
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        return new CompanyResponse(company.Id, company.Name, company.Description, company.Address, []);
    }

    public async Task<CompanyResponse?> UpdateAsync(Guid id, CompanyRequest request)
    {
        var company = await db.Companies.Include(c => c.Articles).FirstOrDefaultAsync(c => c.Id == id);
        if (company is null) return null;

        company.Name        = request.Name;
        company.Description = request.Description;
        company.Address     = request.Address;
        await db.SaveChangesAsync();

        return new CompanyResponse(company.Id, company.Name, company.Description, company.Address,
            company.Articles.Select(a => a.Id));
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var company = await db.Companies.FindAsync(id);
        if (company is null) return false;
        db.Companies.Remove(company);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<Article>?> GetArticlesAsync(Guid companyId)
    {
        var company = await db.Companies.Include(c => c.Articles).FirstOrDefaultAsync(c => c.Id == companyId);
        return company is null ? null : company.Articles.OrderBy(a => a.Code).ToList();
    }

    public async Task<bool> AddArticleAsync(Guid companyId, Guid articleId)
    {
        var company = await db.Companies.Include(c => c.Articles).FirstOrDefaultAsync(c => c.Id == companyId);
        if (company is null) return false;

        var article = await db.Articles.FindAsync(articleId);
        if (article is null) return false;

        if (company.Articles.All(a => a.Id != articleId))
        {
            company.Articles.Add(article);
            await db.SaveChangesAsync();
        }
        return true;
    }

    public async Task<bool> RemoveArticleAsync(Guid companyId, Guid articleId)
    {
        var company = await db.Companies.Include(c => c.Articles).FirstOrDefaultAsync(c => c.Id == companyId);
        if (company is null) return false;

        var article = company.Articles.FirstOrDefault(a => a.Id == articleId);
        if (article is null) return false;

        company.Articles.Remove(article);
        await db.SaveChangesAsync();
        return true;
    }
}
