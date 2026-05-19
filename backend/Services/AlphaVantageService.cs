using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace SpaceEconomy.Api.Services;

public class AlphaVantageService(HttpClient http, IConfiguration config, IMemoryCache cache, ILogger<AlphaVantageService> logger)
{
    private readonly string _apiKey = config["AlphaVantage:ApiKey"]
        ?? throw new InvalidOperationException("AlphaVantage:ApiKey not configured");

    private static readonly TimeSpan _ttl = TimeSpan.FromMinutes(20);
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    // Free tier: 1 req/sec burst, 25 req/day
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private static DateTime _lastCall = DateTime.MinValue;
    private static readonly TimeSpan _minInterval = TimeSpan.FromMilliseconds(1100);

    private async Task<string> ThrottledGetAsync(string url, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var wait = _minInterval - (DateTime.UtcNow - _lastCall);
            if (wait > TimeSpan.Zero) await Task.Delay(wait, ct);
            _lastCall = DateTime.UtcNow;
            return await http.GetStringAsync(url, ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    // AV returns error info in the body with HTTP 200; detect by key presence.
    private bool IsErrorBody(string body, string fn, string ticker)
    {
        if (body.Contains("\"Information\"") || body.Contains("\"Note\"") || body.Contains("\"Error Message\""))
        {
            logger.LogWarning("AlphaVantage {Function} error/rate-limit for {Ticker}: {Body}",
                fn, ticker, body[..Math.Min(300, body.Length)]);
            return true;
        }
        return false;
    }

    public async Task<AvIncomeReport?> GetIncomeStatementAsync(string ticker, CancellationToken ct = default)
    {
        var key = $"av:income:{ticker}";
        if (cache.TryGetValue(key, out AvIncomeReport? hit)) return hit;

        var url = $"https://www.alphavantage.co/query?function=INCOME_STATEMENT&symbol={ticker}&apikey={_apiKey}";
        var body = await ThrottledGetAsync(url, ct);

        AvIncomeReport? value = null;
        if (!IsErrorBody(body, "INCOME_STATEMENT", ticker))
        {
            try
            {
                var resp = JsonSerializer.Deserialize<IncomeStatementResponse>(body, _json);
                value = resp?.AnnualReports?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "AlphaVantage INCOME_STATEMENT deserialize failed for {Ticker}", ticker);
            }
        }

        cache.Set(key, value, _ttl);
        return value;
    }

    public async Task<AvOverview?> GetOverviewAsync(string ticker, CancellationToken ct = default)
    {
        var key = $"av:overview:{ticker}";
        if (cache.TryGetValue(key, out AvOverview? hit)) return hit;

        var url = $"https://www.alphavantage.co/query?function=OVERVIEW&symbol={ticker}&apikey={_apiKey}";
        var body = await ThrottledGetAsync(url, ct);

        AvOverview? value = null;
        if (!IsErrorBody(body, "OVERVIEW", ticker))
        {
            try
            {
                value = JsonSerializer.Deserialize<AvOverview>(body, _json);
                // AV returns {} for unknown tickers
                if (string.IsNullOrEmpty(value?.Symbol)) value = null;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "AlphaVantage OVERVIEW deserialize failed for {Ticker}", ticker);
            }
        }

        cache.Set(key, value, _ttl);
        return value;
    }

    // AV encodes all numbers as strings; "None" means absent.
    public static decimal? ParseDecimal(string? s) =>
        s is null or "None" or "-"
            ? null
            : decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : null;

    public static string? NoneToNull(string? s) =>
        string.IsNullOrEmpty(s) || s == "None" ? null : s;
}

// ── Alpha Vantage response shapes ────────────────────────────────────────────

record IncomeStatementResponse(
    [property: JsonPropertyName("annualReports")] List<AvIncomeReport>? AnnualReports);

public record AvIncomeReport(
    [property: JsonPropertyName("fiscalDateEnding")] string? FiscalDateEnding,
    [property: JsonPropertyName("totalRevenue")] string? TotalRevenue,
    [property: JsonPropertyName("costOfRevenue")] string? CostOfRevenue,
    [property: JsonPropertyName("grossProfit")] string? GrossProfit,
    [property: JsonPropertyName("operatingExpenses")] string? OperatingExpenses,
    [property: JsonPropertyName("netIncome")] string? NetIncome);

public record AvOverview(
    [property: JsonPropertyName("Symbol")] string? Symbol,
    [property: JsonPropertyName("Sector")] string? Sector,
    [property: JsonPropertyName("Industry")] string? Industry,
    [property: JsonPropertyName("Country")] string? Country,
    [property: JsonPropertyName("OfficialSite")] string? OfficialSite,
    [property: JsonPropertyName("FiscalYearEnd")] string? FiscalYearEnd,
    [property: JsonPropertyName("PERatio")] string? PeRatio,
    [property: JsonPropertyName("EPS")] string? Eps,
    [property: JsonPropertyName("Beta")] string? Beta,
    [property: JsonPropertyName("AnalystTargetPrice")] string? AnalystTargetPrice);
