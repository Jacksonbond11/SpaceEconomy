using SpaceEconomy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace SpaceEconomy.Api.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var existing = await db.Companies.Select(c => c.Ticker).ToHashSetAsync();
        var toAdd = InitialCompanies.Where(c => !existing.Contains(c.Ticker)).ToList();
        if (toAdd.Count == 0) return;
        db.Companies.AddRange(toAdd);
        await db.SaveChangesAsync();
    }

    // Name/Desc/Hq for public companies are populated from Polygon on startup.
    // Private companies (Exchange = "—") keep their hardcoded descriptions.
    private static readonly Company[] InitialCompanies =
    [
        new() { Ticker = "RKLB",   Name = "Rocket Lab USA",        Exchange = "NASDAQ", AltKm = 0,      Zone = "launch", Sectors = ["Launch", "Sat ops"],  X = 0.30, Icon = "rocket",        Founded = 2006 },
        new() { Ticker = "RDW",    Name = "Redwire",                Exchange = "NYSE",   AltKm = 250,    Zone = "vleo",   Sectors = ["Sat ops"],             X = 0.46, Icon = "platform",      Founded = 2020 },
        new() { Ticker = "BKSY",   Name = "BlackSky Technology",    Exchange = "NYSE",   AltKm = 450,    Zone = "leo",    Sectors = ["Imagery", "Defense"],  X = 0.08, Icon = "imager",        Founded = 2014 },
        new() { Ticker = "PL",     Name = "Planet Labs",            Exchange = "NYSE",   AltKm = 500,    Zone = "leo",    Sectors = ["Imagery"],             X = 0.22, Icon = "imager",        Founded = 2010 },
        new() { Ticker = "SPIR",   Name = "Spire Global",           Exchange = "NYSE",   AltKm = 520,    Zone = "leo",    Sectors = ["Sat ops", "Imagery"],  X = 0.38, Icon = "cubesat",       Founded = 2012 },
        new() { Ticker = "ASTS",   Name = "AST SpaceMobile",        Exchange = "NASDAQ", AltKm = 700,    Zone = "leo",    Sectors = ["Comms"],               X = 0.66, Icon = "sat-big",       Founded = 2017 },
        new() { Ticker = "IRDM",   Name = "Iridium Communications", Exchange = "NASDAQ", AltKm = 780,    Zone = "leo",    Sectors = ["Comms"],               X = 0.79, Icon = "constellation", Founded = 2001 },
        new() { Ticker = "GSAT",   Name = "Globalstar",             Exchange = "NYSE",   AltKm = 1414,   Zone = "leo",    Sectors = ["Comms"],               X = 0.90, Icon = "constellation", Founded = 1991 },
        new() { Ticker = "VSAT",   Name = "Viasat",                 Exchange = "NASDAQ", AltKm = 35786,  Zone = "deep",   Sectors = ["Comms", "Defense"],    X = 0.18, Icon = "geo-sat",       Founded = 1986 },
        new() { Ticker = "TSAT",   Name = "Telesat",                Exchange = "NASDAQ", AltKm = 35786,  Zone = "deep",   Sectors = ["Comms"],               X = 0.38, Icon = "geo-sat",       Founded = 1969 },
        new() { Ticker = "LUNR",   Name = "Intuitive Machines",     Exchange = "NASDAQ", AltKm = 384400, Zone = "deep",   Sectors = ["Sat ops"],             X = 0.55, Icon = "lander",        Founded = 2013 },
        new() { Ticker = "KRMN",   Name = "Karman Holdings",        Exchange = "NYSE",   AltKm = 0,      Zone = "launch", Sectors = ["Launch", "Defense"],   X = 0.14, Icon = "platform",      Founded = 2022 },
        new() { Ticker = "FLY",    Name = "Firefly Aerospace",      Exchange = "NASDAQ", AltKm = 0,      Zone = "launch", Sectors = ["Launch"],              X = 0.45, Icon = "rocket",        Founded = 2017 },

        // Private — Polygon won't have details, so descriptions are hardcoded
        new() { Ticker = "MAXR",   Name = "Maxar Intelligence",     Exchange = "—",      AltKm = 617,    Zone = "leo",    Sectors = ["Imagery", "Defense"],  X = 0.52, Icon = "imager",        Founded = 1969, Desc = "High-resolution Earth imagery and geospatial intelligence — WorldView Legion constellation. Taken private by Advent in 2023.", Hq = "Westminster, CO" },
        new() { Ticker = "SPACEX", Name = "SpaceX",                 Exchange = "—",      AltKm = 0,      Zone = "launch", Sectors = ["Launch", "Comms"],     X = 0.62, Icon = "rocket",        Founded = 2002, Desc = "World's most prolific launch provider operating Falcon 9, Falcon Heavy, and Starship. Also runs Starlink, the largest LEO broadband constellation with 6,000+ satellites. Privately held; IPO pending.", Hq = "Hawthorne, CA" },
    ];
}
