using WebTestApp.Models;

namespace WebTestApp.Services;

public interface ICompanyService
{
    IReadOnlyList<Company> GetAll();
    Company? GetById(Guid id);
    Company Create(CompanyRequest request);
    Company? Update(Guid id, CompanyRequest request);
    bool Delete(Guid id);
}
