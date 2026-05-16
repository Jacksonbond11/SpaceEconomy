using Massive.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Massive.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? ticker,
        [FromQuery] int limit = 20)
    {
        limit = Math.Clamp(limit, 1, 50);

        var query = db.NewsArticles.AsQueryable();

        if (!string.IsNullOrEmpty(ticker))
            query = query.Where(n => n.Tickers.Any(t => t == ticker.ToUpper()));

        var articles = await query
            .OrderByDescending(n => n.PublishedUtc)
            .Take(limit)
            .Select(n => new
            {
                n.Id, n.Title, n.Description,
                n.ArticleUrl, n.ImageUrl,
                n.Author, n.Publisher,
                n.Tickers, n.PublishedUtc,
            })
            .ToListAsync();

        return Ok(articles);
    }
}
