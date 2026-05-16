namespace SpaceEconomy.Api.Models;

public class Quote
{
    public string Ticker { get; set; } = null!;
    public decimal? Price { get; set; }
    public decimal? Change { get; set; }
    public decimal? ChangePct { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? Open { get; set; }
    public long? Volume { get; set; }
    public decimal? MarketCap { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Company Company { get; set; } = null!;
}
