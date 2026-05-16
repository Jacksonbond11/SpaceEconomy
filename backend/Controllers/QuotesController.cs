using SpaceEconomy.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SpaceEconomy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuotesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var quotes = await db.Quotes.ToListAsync();
        var map = quotes.ToDictionary(
            q => q.Ticker,
            q => new
            {
                q.Price, q.Change, q.ChangePct,
                q.High, q.Low, q.Open, q.Volume,
                q.MarketCap, q.UpdatedAt,
            });
        return Ok(map);
    }

    [HttpGet("{ticker}")]
    public async Task<IActionResult> Get(string ticker)
    {
        var quote = await db.Quotes.FindAsync(ticker.ToUpper());
        if (quote is null) return NotFound();
        return Ok(new
        {
            quote.Price, quote.Change, quote.ChangePct,
            quote.High, quote.Low, quote.Open, quote.Volume,
            quote.MarketCap, quote.UpdatedAt,
        });
    }
}
