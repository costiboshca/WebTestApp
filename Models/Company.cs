namespace WebTestApp.Models;

public class Company
{
    public Guid      Id          { get; set; } = Guid.NewGuid();
    public string    Name        { get; set; } = string.Empty;
    public string    Description { get; set; } = string.Empty;
    public string    Address     { get; set; } = string.Empty;
    public HashSet<Guid> ArticleIds { get; set; } = [];
}

public record CompanyRequest(string Name, string Description, string Address);
