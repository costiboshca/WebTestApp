using WebTestApp.Models;

namespace WebTestApp.Services;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanyResponse>> GetAllAsync();
    Task<CompanyResponse?>              GetByIdAsync(Guid id);
    Task<CompanyResponse>               CreateAsync(CompanyRequest request);
    Task<CompanyResponse?>              UpdateAsync(Guid id, CompanyRequest request);
    Task<bool>                          DeleteAsync(Guid id);

    Task<IReadOnlyList<Article>?>       GetArticlesAsync(Guid companyId);
    Task<bool>                          AddArticleAsync(Guid companyId, Guid articleId);
    Task<bool>                          RemoveArticleAsync(Guid companyId, Guid articleId);
}
