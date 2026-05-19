using SpaceEconomy.Api.Data;
using SpaceEconomy.Api.Models;
using SpaceEconomy.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SpaceEconomy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController(AppDbContext db, FmpService fmp) : ControllerBase
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
        var upper = ticker.ToUpper();

        var company = await db.Companies
            .Include(c => c.Quote)
            .FirstOrDefaultAsync(c => c.Ticker == upper);

        if (company is null) return NotFound();

        var news = await db.NewsArticles
            .Where(n => n.Tickers.Any(t => t == upper))
            .OrderByDescending(n => n.PublishedUtc)
            .Take(8)
            .ToListAsync();

        FmpIncomeStatement? financials = null;
        IReadOnlyList<FmpExecutive> executives = [];

        if (company.Exchange != "—")
        {
            try { financials = await fmp.GetIncomeStatementAsync(upper); }
            catch (Exception ex) when (ex is not OperationCanceledException) { }

            try { executives = await fmp.GetExecutivesAsync(upper); }
            catch (Exception ex) when (ex is not OperationCanceledException) { }
        }

        return Ok(new
        {
            company = ToDto(company),
            news = news.Select(n => new
            {
                n.Id, n.Title, n.Description, n.ArticleUrl,
                n.ImageUrl, n.Author, n.Publisher, n.PublishedUtc,
            }),
            financials = financials is null ? null : new
            {
                financials.Date,
                financials.Revenue,
                financials.CostOfRevenue,
                GrossProfitRatio = financials.Revenue > 0
                    ? financials.GrossProfit / financials.Revenue
                    : (decimal?)null,
                financials.OperatingExpenses,
                financials.OperatingIncome,
                financials.NetIncome,
                NetIncomeRatio = financials.Revenue > 0
                    ? financials.NetIncome / financials.Revenue
                    : (decimal?)null,
            },
            executives = executives
                .Where(e => e.Name is not null && e.Title is not null)
                .Select(e => new { e.Name, e.Title }),
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
