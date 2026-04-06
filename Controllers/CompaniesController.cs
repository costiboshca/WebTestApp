using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebTestApp.Models;
using WebTestApp.Services;

namespace WebTestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companies;
    private readonly IArticleService _articles;

    public CompaniesController(ICompanyService companies, IArticleService articles)
    {
        _companies = companies;
        _articles  = articles;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_companies.GetAll());

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Company), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var company = _companies.GetById(id);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Company), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var company = _companies.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Company), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Update(Guid id, [FromBody] CompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var company = _companies.Update(id, request);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        _companies.Delete(id) ? NoContent() : NotFound();

    // ── Company → Articles sub-resource ──────────────────────────────────────

    [HttpGet("{id:guid}/articles")]
    [ProducesResponseType(typeof(IEnumerable<Article>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetArticles(Guid id)
    {
        var ids = _companies.GetArticleIds(id);
        if (ids is null) return NotFound();

        var articles = ids
            .Select(aid => _articles.GetById(aid))
            .Where(a => a is not null)
            .ToList();

        return Ok(articles);
    }

    [HttpPost("{id:guid}/articles/{articleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AddArticle(Guid id, Guid articleId)
    {
        if (_articles.GetById(articleId) is null)
            return NotFound("Article not found.");

        if (!_companies.AddArticle(id, articleId))
            return NotFound("Company not found.");

        return NoContent();
    }

    [HttpDelete("{id:guid}/articles/{articleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RemoveArticle(Guid id, Guid articleId)
    {
        if (_companies.GetById(id) is null) return NotFound("Company not found.");
        if (!_companies.RemoveArticle(id, articleId)) return NotFound("Article not linked.");
        return NoContent();
    }
}
