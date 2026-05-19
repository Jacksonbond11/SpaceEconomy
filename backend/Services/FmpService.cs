using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace SpaceEconomy.Api.Services;

public class FmpService(HttpClient http, IConfiguration config, IMemoryCache cache, ILogger<FmpService> logger)
{
    private readonly string _apiKey = config["FMP:ApiKey"]
        ?? throw new InvalidOperationException("FMP:ApiKey not configured");

    private static readonly TimeSpan _ttl = TimeSpan.FromMinutes(20);
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    // Returns null and logs a warning if FMP returns an error body instead of a JSON array.
    private async Task<T[]?> FetchArrayAsync<T>(string url, string endpoint, string ticker, CancellationToken ct)
    {
        var body = await http.GetStringAsync(url, ct);
        if (!body.TrimStart().StartsWith('['))
        {
            logger.LogWarning("FMP {Endpoint} non-array response for {Ticker}: {Body}",
                endpoint, ticker, body[..Math.Min(300, body.Length)]);
            return null;
        }
        try
        {
            return JsonSerializer.Deserialize<T[]>(body, _json);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "FMP {Endpoint} deserialize failed for {Ticker}", endpoint, ticker);
            return null;
        }
    }

    public async Task<FmpIncomeStatement?> GetIncomeStatementAsync(string ticker, CancellationToken ct = default)
    {
        var key = $"fmp:income:{ticker}";
        if (cache.TryGetValue(key, out FmpIncomeStatement? hit))
            return hit;

        var url = $"https://financialmodelingprep.com/stable/income-statement?symbol={ticker}&limit=1&apikey={_apiKey}";
        var results = await FetchArrayAsync<FmpIncomeStatement>(url, "income-statement", ticker, ct);
        var value = results?.FirstOrDefault();
        cache.Set(key, value, _ttl);
        return value;
    }

    public async Task<IReadOnlyList<FmpExecutive>> GetExecutivesAsync(string ticker, CancellationToken ct = default)
    {
        var key = $"fmp:exec:{ticker}";
        if (cache.TryGetValue(key, out IReadOnlyList<FmpExecutive>? hit))
            return hit ?? [];

        var url = $"https://financialmodelingprep.com/stable/key-executives?symbol={ticker}&apikey={_apiKey}";
        var results = await FetchArrayAsync<FmpExecutive>(url, "key-executives", ticker, ct);
        IReadOnlyList<FmpExecutive> value = results ?? [];
        cache.Set(key, value, _ttl);
        return value;
    }
}

// ── FMP response shapes ──────────────────────────────────────────────────────

public record FmpIncomeStatement(
    [property: JsonPropertyName("date")] string? Date,
    [property: JsonPropertyName("revenue")] decimal? Revenue,
    [property: JsonPropertyName("costOfRevenue")] decimal? CostOfRevenue,
    [property: JsonPropertyName("grossProfit")] decimal? GrossProfit,
    [property: JsonPropertyName("operatingExpenses")] decimal? OperatingExpenses,
    [property: JsonPropertyName("operatingIncome")] decimal? OperatingIncome,
    [property: JsonPropertyName("netIncome")] decimal? NetIncome);

public record FmpExecutive(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("title")] string? Title);
