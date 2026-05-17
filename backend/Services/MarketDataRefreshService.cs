using SpaceEconomy.Api.Data;
using SpaceEconomy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace SpaceEconomy.Api.Services;

public class MarketDataRefreshService(
    IServiceScopeFactory scopeFactory,
    ILogger<MarketDataRefreshService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), ct);

        await RefreshCompanyDetailsAsync(ct);
        await RefreshQuotesAsync(ct);
        await RefreshNewsAsync(ct);

        int cycle = 0;
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (await timer.WaitForNextTickAsync(ct))
        {
            await RefreshQuotesAsync(ct);
            cycle++;
            if (cycle % 12 == 0)
                await RefreshNewsAsync(ct);
        }
    }

    private async Task RefreshCompanyDetailsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var polygon = scope.ServiceProvider.GetRequiredService<PolygonService>();

        var companies = await db.Companies
            .Where(c => c.Exchange != "—")
            .ToListAsync(ct);

        foreach (var company in companies)
        {
            try
            {
                var details = await polygon.GetTickerDetailsAsync(company.Ticker, ct);
                if (details is null) continue;

                if (!string.IsNullOrWhiteSpace(details.Name))
                    company.Name = details.Name;
                if (!string.IsNullOrWhiteSpace(details.Description))
                    company.Desc = details.Description;
                if (details.Address is { City: not null, State: not null })
                    company.Hq = $"{details.Address.City}, {details.Address.State}";

                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Details fetch failed for {Ticker}", company.Ticker);
            }
        }

        logger.LogInformation("Company details refreshed for {Count} tickers", companies.Count);
    }

    private async Task RefreshQuotesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var polygon = scope.ServiceProvider.GetRequiredService<PolygonService>();

        var tickers = await db.Companies
            .Where(c => c.Exchange != "—")
            .Select(c => c.Ticker)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        int refreshed = 0;

        foreach (var ticker in tickers)
        {
            try
            {
                var bar = await polygon.GetPreviousCloseAsync(ticker, ct);
                if (bar is null) continue;

                var existing = await db.Quotes.FindAsync([ticker], ct);
                if (existing is null)
                {
                    db.Quotes.Add(new Quote
                    {
                        Ticker = ticker,
                        Price = bar.Close,
                        High = bar.High,
                        Low = bar.Low,
                        Open = bar.Open,
                        Volume = (long?)bar.Volume,
                        UpdatedAt = now,
                    });
                }
                else
                {
                    existing.Price = bar.Close;
                    existing.High = bar.High;
                    existing.Low = bar.Low;
                    existing.Open = bar.Open;
                    existing.Volume = (long?)bar.Volume;
                    existing.UpdatedAt = now;
                }

                await db.SaveChangesAsync(ct);
                refreshed++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Quote fetch failed for {Ticker}", ticker);
            }
        }

        logger.LogInformation("Quotes refreshed for {Count}/{Total} tickers", refreshed, tickers.Count);
    }

    private async Task RefreshNewsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var polygon = scope.ServiceProvider.GetRequiredService<PolygonService>();

            var tickers = await db.Companies
                .Where(c => c.Exchange != "—")
                .Select(c => c.Ticker)
                .ToListAsync(ct);

            var now = DateTime.UtcNow;
            foreach (var ticker in tickers)
            {
                try
                {
                    var articles = await polygon.GetNewsAsync(ticker, limit: 5, ct);
                    foreach (var article in articles)
                    {
                        if (await db.NewsArticles.AnyAsync(n => n.Id == article.Id, ct))
                            continue;

                        db.NewsArticles.Add(new NewsArticle
                        {
                            Id = article.Id,
                            Tickers = (article.Tickers ?? []).ToArray(),
                            Title = article.Title,
                            Description = article.Description,
                            ArticleUrl = article.ArticleUrl,
                            ImageUrl = article.ImageUrl,
                            Author = article.Author,
                            Publisher = article.Publisher?.Name,
                            PublishedUtc = article.PublishedUtc,
                            FetchedAt = now,
                        });
                    }
                    await db.SaveChangesAsync(ct);
                    await Task.Delay(150, ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "News fetch failed for {Ticker}", ticker);
                }
            }

            logger.LogInformation("News refreshed for {Count} tickers", tickers.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "News refresh failed");
        }
    }
}
