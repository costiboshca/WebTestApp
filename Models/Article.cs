namespace WebTestApp.Models;

public class Article
{
    public Guid   Id          { get; set; } = Guid.NewGuid();
    public string Code        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
}

public record ArticleRequest(string Code, string Description, string ProductCode);
