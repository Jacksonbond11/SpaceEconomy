using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace SpaceEconomy.Api.Services;

public class FmpService(HttpClient http, IConfiguration config, IMemoryCache cache)
{
    private readonly string _apiKey = config["FMP:ApiKey"]
        ?? throw new InvalidOperationException("FMP:ApiKey not configured");

    private static readonly TimeSpan _ttl = TimeSpan.FromMinutes(20);

    public async Task<FmpIncomeStatement?> GetIncomeStatementAsync(string ticker, CancellationToken ct = default)
    {
        var key = $"fmp:income:{ticker}";
        if (cache.TryGetValue(key, out FmpIncomeStatement? hit))
            return hit;

        var url = $"https://financialmodelingprep.com/api/v3/income-statement/{ticker}?limit=1&apikey={_apiKey}";
        var results = await http.GetFromJsonAsync<FmpIncomeStatement[]>(url, ct);
        var value = results?.FirstOrDefault();
        cache.Set(key, value, _ttl);
        return value;
    }

    public async Task<IReadOnlyList<FmpExecutive>> GetExecutivesAsync(string ticker, CancellationToken ct = default)
    {
        var key = $"fmp:exec:{ticker}";
        if (cache.TryGetValue(key, out IReadOnlyList<FmpExecutive>? hit))
            return hit ?? [];

        var url = $"https://financialmodelingprep.com/api/v3/key-executives/{ticker}?apikey={_apiKey}";
        var results = await http.GetFromJsonAsync<FmpExecutive[]>(url, ct);
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
    [property: JsonPropertyName("grossProfitRatio")] decimal? GrossProfitRatio,
    [property: JsonPropertyName("operatingExpenses")] decimal? OperatingExpenses,
    [property: JsonPropertyName("operatingIncome")] decimal? OperatingIncome,
    [property: JsonPropertyName("netIncome")] decimal? NetIncome,
    [property: JsonPropertyName("netIncomeRatio")] decimal? NetIncomeRatio);

public record FmpExecutive(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("title")] string? Title);
