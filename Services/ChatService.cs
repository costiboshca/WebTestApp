using System.Text;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.EntityFrameworkCore;
using WebTestApp.Data;
using WebTestApp.Models;

namespace WebTestApp.Services;

public class ChatService(AppDbContext db, IConfiguration config) : IChatService
{
    private const string SystemPrompt = """
        You are a helpful AI assistant for WebTestApp, a business management application.
        You have access to the application's database which contains Companies and Articles.

        Companies have: Name, Description, Address, and linked Articles.
        Articles have: Code (unique identifier), Description, and ProductCode, and can be linked to multiple Companies.

        Use the available tools to query the database and answer the user's questions accurately.
        Be concise and helpful. When listing items, format them clearly.
        """;

    private List<ToolUnion> BuildTools()
    {
        return
        [
            new Tool
            {
                Name = "list_companies",
                Description = "Lists all companies in the database with their basic info and article count.",
                InputSchema = new()
                {
                    Properties = new Dictionary<string, JsonElement>(),
                    Required = [],
                },
            },
            new Tool
            {
                Name = "get_company",
                Description = "Gets details for a specific company including all its linked articles.",
                InputSchema = new()
                {
                    Properties = new Dictionary<string, JsonElement>
                    {
                        ["id"] = JsonSerializer.SerializeToElement(new
                        {
                            type = "string",
                            description = "The company GUID id"
                        }),
                    },
                    Required = ["id"],
                },
            },
            new Tool
            {
                Name = "list_articles",
                Description = "Lists all articles in the database.",
                InputSchema = new()
                {
                    Properties = new Dictionary<string, JsonElement>(),
                    Required = [],
                },
            },
            new Tool
            {
                Name = "get_article",
                Description = "Gets details for a specific article including all companies it is linked to.",
                InputSchema = new()
                {
                    Properties = new Dictionary<string, JsonElement>
                    {
                        ["id"] = JsonSerializer.SerializeToElement(new
                        {
                            type = "string",
                            description = "The article GUID id"
                        }),
                    },
                    Required = ["id"],
                },
            },
        ];
    }

    private async Task<string> ExecuteToolAsync(string name, IReadOnlyDictionary<string, JsonElement> input)
    {
        switch (name)
        {
            case "list_companies":
            {
                var companies = await db.Companies
                    .Include(c => c.Articles)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                if (companies.Count == 0) return "No companies found in the database.";

                var sb = new StringBuilder();
                sb.AppendLine($"Found {companies.Count} company(ies):");
                foreach (var c in companies)
                    sb.AppendLine($"- [{c.Id}] {c.Name} | {c.Description} | {c.Address} | Articles: {c.Articles.Count}");
                return sb.ToString();
            }

            case "get_company":
            {
                if (!input.TryGetValue("id", out var idEl) || !Guid.TryParse(idEl.GetString(), out var id))
                    return "Error: invalid or missing 'id' parameter.";

                var company = await db.Companies
                    .Include(c => c.Articles)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (company is null) return $"Company with id '{id}' not found.";

                var sb = new StringBuilder();
                sb.AppendLine($"Company: {company.Name}");
                sb.AppendLine($"  Id: {company.Id}");
                sb.AppendLine($"  Description: {company.Description}");
                sb.AppendLine($"  Address: {company.Address}");
                sb.AppendLine($"  Articles ({company.Articles.Count}):");
                foreach (var a in company.Articles.OrderBy(a => a.Code))
                    sb.AppendLine($"    - [{a.Id}] {a.Code} | {a.Description} | {a.ProductCode}");
                return sb.ToString();
            }

            case "list_articles":
            {
                var articles = await db.Articles.OrderBy(a => a.Code).ToListAsync();

                if (articles.Count == 0) return "No articles found in the database.";

                var sb = new StringBuilder();
                sb.AppendLine($"Found {articles.Count} article(s):");
                foreach (var a in articles)
                    sb.AppendLine($"- [{a.Id}] {a.Code} | {a.Description} | ProductCode: {a.ProductCode}");
                return sb.ToString();
            }

            case "get_article":
            {
                if (!input.TryGetValue("id", out var idEl) || !Guid.TryParse(idEl.GetString(), out var id))
                    return "Error: invalid or missing 'id' parameter.";

                var article = await db.Articles
                    .Include(a => a.Companies)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (article is null) return $"Article with id '{id}' not found.";

                var sb = new StringBuilder();
                sb.AppendLine($"Article: {article.Code}");
                sb.AppendLine($"  Id: {article.Id}");
                sb.AppendLine($"  Description: {article.Description}");
                sb.AppendLine($"  ProductCode: {article.ProductCode}");
                sb.AppendLine($"  Linked Companies ({article.Companies.Count}):");
                foreach (var c in article.Companies.OrderBy(c => c.Name))
                    sb.AppendLine($"    - [{c.Id}] {c.Name}");
                return sb.ToString();
            }

            default:
                return $"Unknown tool: {name}";
        }
    }

    public async Task<string> ChatAsync(List<ChatMessage> history)
    {
        var apiKey = config["Claude:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "CHANGE_ME")
            return "The Claude API key is not configured. Please set Claude:ApiKey in appsettings.Development.json.";

        var client = new AnthropicClient { ApiKey = apiKey };
        var tools  = BuildTools();

        // Build the messages list from conversation history
        var messages = history.Select(m => new MessageParam
        {
            Role    = m.Role == "assistant" ? Role.Assistant : Role.User,
            Content = m.Content,
        }).ToList();

        // Agentic loop — runs until Claude returns end_turn
        while (true)
        {
            var response = await client.Messages.Create(new MessageCreateParams
            {
                Model     = Model.ClaudeOpus4_6,
                MaxTokens = 8000,
                Thinking  = new ThinkingConfigAdaptive(),
                System    = SystemPrompt,
                Tools     = tools,
                Messages  = messages,
            });

            if (response.StopReason == StopReason.EndTurn)
            {
                // Return the final text response
                return response.Content
                    .Select(b => b.Value)
                    .OfType<TextBlock>()
                    .FirstOrDefault()?.Text ?? string.Empty;
            }

            if (response.StopReason == StopReason.ToolUse)
            {
                // Reconstruct the assistant turn
                List<ContentBlockParam> assistantContent = [];
                List<ContentBlockParam> toolResults      = [];

                foreach (var block in response.Content)
                {
                    if (block.TryPickText(out TextBlock? text))
                    {
                        assistantContent.Add(new TextBlockParam { Text = text.Text });
                    }
                    else if (block.TryPickThinking(out ThinkingBlock? thinking))
                    {
                        assistantContent.Add(new ThinkingBlockParam
                        {
                            Thinking  = thinking.Thinking,
                            Signature = thinking.Signature,
                        });
                    }
                    else if (block.TryPickRedactedThinking(out RedactedThinkingBlock? redacted))
                    {
                        assistantContent.Add(new RedactedThinkingBlockParam { Data = redacted.Data });
                    }
                    else if (block.TryPickToolUse(out ToolUseBlock? toolUse))
                    {
                        assistantContent.Add(new ToolUseBlockParam
                        {
                            ID    = toolUse.ID,
                            Name  = toolUse.Name,
                            Input = toolUse.Input,
                        });

                        var result = await ExecuteToolAsync(toolUse.Name, toolUse.Input);
                        toolResults.Add(new ToolResultBlockParam
                        {
                            ToolUseID = toolUse.ID,
                            Content   = result,
                        });
                    }
                }

                messages.Add(new MessageParam { Role = Role.Assistant, Content = assistantContent });
                messages.Add(new MessageParam { Role = Role.User,      Content = toolResults });
                continue;
            }

            // Unexpected stop reason — return whatever text we have
            return response.Content
                .Select(b => b.Value)
                .OfType<TextBlock>()
                .FirstOrDefault()?.Text ?? "Sorry, I encountered an unexpected issue.";
        }
    }
}
