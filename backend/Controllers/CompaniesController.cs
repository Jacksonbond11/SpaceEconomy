using SpaceEconomy.Api.Data;
using SpaceEconomy.Api.Models;
using SpaceEconomy.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SpaceEconomy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController(AppDbContext db, AlphaVantageService av) : ControllerBase
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

        AvIncomeReport? income = null;
        AvOverview? overview = null;

        if (company.Exchange != "—")
        {
            try { income = await av.GetIncomeStatementAsync(upper); }
            catch (Exception ex) when (ex is not OperationCanceledException) { }

            try { overview = await av.GetOverviewAsync(upper); }
            catch (Exception ex) when (ex is not OperationCanceledException) { }
        }

        var revenue = AlphaVantageService.ParseDecimal(income?.TotalRevenue);
        var grossProfit = AlphaVantageService.ParseDecimal(income?.GrossProfit);
        var netIncome = AlphaVantageService.ParseDecimal(income?.NetIncome);

        return Ok(new
        {
            company = ToDto(company),
            news = news.Select(n => new
            {
                n.Id, n.Title, n.Description, n.ArticleUrl,
                n.ImageUrl, n.Author, n.Publisher, n.PublishedUtc,
            }),
            financials = income is null ? null : new
            {
                Date = income.FiscalDateEnding,
                Revenue = revenue,
                CostOfRevenue = AlphaVantageService.ParseDecimal(income.CostOfRevenue),
                GrossProfitRatio = revenue > 0 ? grossProfit / revenue : (decimal?)null,
                OperatingExpenses = AlphaVantageService.ParseDecimal(income.OperatingExpenses),
                NetIncome = netIncome,
                NetIncomeRatio = revenue > 0 ? netIncome / revenue : (decimal?)null,
            },
            companyOverview = overview is null ? null : new
            {
                Sector = AlphaVantageService.NoneToNull(overview.Sector),
                Industry = AlphaVantageService.NoneToNull(overview.Industry),
                Country = AlphaVantageService.NoneToNull(overview.Country),
                OfficialSite = AlphaVantageService.NoneToNull(overview.OfficialSite),
                FiscalYearEnd = AlphaVantageService.NoneToNull(overview.FiscalYearEnd),
                PeRatio = AlphaVantageService.ParseDecimal(overview.PeRatio),
                Eps = AlphaVantageService.ParseDecimal(overview.Eps),
                Beta = AlphaVantageService.ParseDecimal(overview.Beta),
                AnalystTargetPrice = AlphaVantageService.ParseDecimal(overview.AnalystTargetPrice),
            },
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
