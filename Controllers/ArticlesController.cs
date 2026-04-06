using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebTestApp.Models;
using WebTestApp.Services;

namespace WebTestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articles;

    public ArticlesController(IArticleService articles) => _articles = articles;

    [HttpGet]
    public IActionResult GetAll() => Ok(_articles.GetAll());

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Article), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var article = _articles.GetById(id);
        return article is null ? NotFound() : Ok(article);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Article), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] ArticleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Code is required.");

        var article = _articles.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = article.Id }, article);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Article), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Update(Guid id, [FromBody] ArticleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Code is required.");

        var article = _articles.Update(id, request);
        return article is null ? NotFound() : Ok(article);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id) =>
        _articles.Delete(id) ? NoContent() : NotFound();
}
