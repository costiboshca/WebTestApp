using WebTestApp.Models;

namespace WebTestApp.Services;

public interface ICompanyService
{
    IReadOnlyList<Company> GetAll();
    Company? GetById(Guid id);
    Company Create(CompanyRequest request);
    Company? Update(Guid id, CompanyRequest request);
    bool Delete(Guid id);

    bool AddArticle(Guid companyId, Guid articleId);
    bool RemoveArticle(Guid companyId, Guid articleId);
    IReadOnlySet<Guid>? GetArticleIds(Guid companyId);
}
