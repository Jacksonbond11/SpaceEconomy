using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SpaceEconomy.Api.Services;

public class PolygonService(HttpClient http, IConfiguration config)
{
    private readonly string _apiKey = config["Polygon:ApiKey"]
        ?? throw new InvalidOperationException("Polygon:ApiKey not configured");

    // Free tier: 5 calls/minute → 1 call every 13s with buffer
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private static DateTime _lastCall = DateTime.MinValue;
    private static readonly TimeSpan _minInterval = TimeSpan.FromSeconds(13);

    // Polygon uses mixed-case field names (e.g. "T" ticker vs "t" timestamp)
    // so we need case-sensitive deserialization
    private static readonly System.Text.Json.JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = false,
    };

    private async Task<T?> CallAsync<T>(string url, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var wait = _minInterval - (DateTime.UtcNow - _lastCall);
            if (wait > TimeSpan.Zero) await Task.Delay(wait, ct);
            _lastCall = DateTime.UtcNow;
            return await http.GetFromJsonAsync<T>(url, _jsonOpts, ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<PolygonPrevClose?> GetPreviousCloseAsync(
        string ticker, CancellationToken ct = default)
    {
        var url = $"https://api.polygon.io/v2/aggs/ticker/{ticker}/prev?adjusted=true&apiKey={_apiKey}";
        var response = await CallAsync<PrevCloseResponse>(url, ct);
        return response?.Results?.FirstOrDefault();
    }

    public async Task<IReadOnlyList<PolygonNewsResult>> GetNewsAsync(
        string? ticker = null, int limit = 10, CancellationToken ct = default)
    {
        var url = ticker is not null
            ? $"https://api.polygon.io/v2/reference/news?ticker={ticker}&limit={limit}&order=desc&apiKey={_apiKey}"
            : $"https://api.polygon.io/v2/reference/news?limit={limit}&order=desc&apiKey={_apiKey}";
        var response = await CallAsync<NewsResponse>(url, ct);
        return response?.Results ?? [];
    }

    public async Task<PolygonTickerDetails?> GetTickerDetailsAsync(
        string ticker, CancellationToken ct = default)
    {
        var url = $"https://api.polygon.io/v3/reference/tickers/{ticker}?apiKey={_apiKey}";
        var response = await CallAsync<TickerDetailsResponse>(url, ct);
        return response?.Results;
    }
}

// ── Polygon response shapes ──────────────────────────────────────────────────

record PrevCloseResponse(
    [property: JsonPropertyName("results")] List<PolygonPrevClose>? Results);

public record PolygonPrevClose(
    [property: JsonPropertyName("T")] string Ticker,
    [property: JsonPropertyName("o")] decimal? Open,
    [property: JsonPropertyName("h")] decimal? High,
    [property: JsonPropertyName("l")] decimal? Low,
    [property: JsonPropertyName("c")] decimal? Close,
    [property: JsonPropertyName("v")] decimal? Volume,
    [property: JsonPropertyName("vw")] decimal? Vwap);

record NewsResponse(
    [property: JsonPropertyName("results")] List<PolygonNewsResult>? Results);

public record PolygonNewsResult(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("article_url")] string? ArticleUrl,
    [property: JsonPropertyName("image_url")] string? ImageUrl,
    [property: JsonPropertyName("author")] string? Author,
    [property: JsonPropertyName("published_utc")] DateTime PublishedUtc,
    [property: JsonPropertyName("tickers")] List<string>? Tickers,
    [property: JsonPropertyName("publisher")] PolygonPublisher? Publisher);

public record PolygonPublisher(
    [property: JsonPropertyName("name")] string Name);

record TickerDetailsResponse(
    [property: JsonPropertyName("results")] PolygonTickerDetails? Results);

public record PolygonTickerDetails(
    [property: JsonPropertyName("ticker")] string Ticker,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("market_cap")] decimal? MarketCap,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("homepage_url")] string? HomepageUrl,
    [property: JsonPropertyName("address")] PolygonAddress? Address,
    [property: JsonPropertyName("list_date")] string? ListDate);

public record PolygonAddress(
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("state")] string? State);
