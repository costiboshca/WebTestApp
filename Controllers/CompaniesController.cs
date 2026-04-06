using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebTestApp.Models;
using WebTestApp.Services;

namespace WebTestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController(ICompanyService companies, IArticleService articles) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await companies.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var company = await companies.GetByIdAsync(id);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var company = await companies.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var company = await companies.UpdateAsync(id, request);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await companies.DeleteAsync(id) ? NoContent() : NotFound();

    // ── Company → Articles sub-resource ──────────────────────────────────────

    [HttpGet("{id:guid}/articles")]
    public async Task<IActionResult> GetArticles(Guid id)
    {
        var result = await companies.GetArticlesAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/articles/{articleId:guid}")]
    public async Task<IActionResult> AddArticle(Guid id, Guid articleId)
    {
        if (await articles.GetByIdAsync(articleId) is null)
            return NotFound("Article not found.");

        return await companies.AddArticleAsync(id, articleId)
            ? NoContent()
            : NotFound("Company not found.");
    }

    [HttpDelete("{id:guid}/articles/{articleId:guid}")]
    public async Task<IActionResult> RemoveArticle(Guid id, Guid articleId)
    {
        return await companies.RemoveArticleAsync(id, articleId)
            ? NoContent()
            : NotFound();
    }
}
