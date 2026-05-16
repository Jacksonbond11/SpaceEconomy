using SpaceEconomy.Api.Data;
using SpaceEconomy.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SpaceEconomy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companies = await db.Companies
            .Include(c => c.Quote)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(companies.Select(ToDto));
    }

    [HttpGet("{ticker}")]
    public async Task<IActionResult> Get(string ticker)
    {
        var company = await db.Companies
            .Include(c => c.Quote)
            .FirstOrDefaultAsync(c => c.Ticker == ticker.ToUpper());

        if (company is null) return NotFound();

        var news = await db.NewsArticles
            .Where(n => n.Tickers.Any(t => t == ticker.ToUpper()))
            .OrderByDescending(n => n.PublishedUtc)
            .Take(8)
            .ToListAsync();

        return Ok(new
        {
            company = ToDto(company),
            news = news.Select(n => new
            {
                n.Id, n.Title, n.Description, n.ArticleUrl,
                n.ImageUrl, n.Author, n.Publisher, n.PublishedUtc,
            }),
        });
    }

    private static object ToDto(Company c) => new
    {
        c.Ticker, c.Name, c.Exchange,
        c.AltKm, c.Zone, c.Sectors,
        c.X, c.YOffset, c.Icon, c.Desc,
        c.Founded, c.Hq,
        quote = c.Quote is null ? null : new
        {
            c.Quote.Price, c.Quote.Change, c.Quote.ChangePct,
            c.Quote.High, c.Quote.Low, c.Quote.Open,
            c.Quote.Volume, c.Quote.MarketCap, c.Quote.UpdatedAt,
        },
    };
}
