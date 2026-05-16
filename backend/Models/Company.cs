namespace SpaceEconomy.Api.Models;

public class Company
{
    public string Ticker { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Exchange { get; set; } = null!;
    public int AltKm { get; set; }
    public string Zone { get; set; } = null!;
    public string[] Sectors { get; set; } = [];
    public double X { get; set; }
    public double? YOffset { get; set; }
    public string Icon { get; set; } = null!;
    public string Desc { get; set; } = null!;
    public int? Founded { get; set; }
    public string? Hq { get; set; }

    public Quote? Quote { get; set; }
}
