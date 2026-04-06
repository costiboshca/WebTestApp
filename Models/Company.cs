namespace WebTestApp.Models;

public class Company
{
    public Guid   Id          { get; set; } = Guid.NewGuid();
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address     { get; set; } = string.Empty;

    public ICollection<Article> Articles { get; set; } = [];
}

public record CompanyRequest(string Name, string Description, string Address);

// Flattened response ─ keeps the same JSON shape the frontend already uses
public record CompanyResponse(
    Guid               Id,
    string             Name,
    string             Description,
    string             Address,
    IEnumerable<Guid>  ArticleIds);
