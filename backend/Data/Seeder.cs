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

    private static readonly Company[] InitialCompanies =
    [
        new() { Ticker = "RKLB",   Name = "Rocket Lab USA",         Exchange = "NASDAQ", AltKm = 0,      Zone = "launch", Sectors = ["Launch", "Sat ops"], X = 0.30, Icon = "rocket",       Desc = "End-to-end launch and space systems company. Operates the small-lift Electron and is developing the medium-lift Neutron rocket. Also builds satellites and components through its Space Systems arm.", Founded = 2006, Hq = "Long Beach, CA" },
        new() { Ticker = "RDW",    Name = "Redwire",                 Exchange = "NYSE",   AltKm = 250,    Zone = "vleo",   Sectors = ["Sat ops"],            X = 0.46, Icon = "platform",     Desc = "Space infrastructure provider. Builds VLEO platforms, in-space manufacturing payloads, deployable solar arrays, and avionics. Recently expanded into autonomous spacecraft via the Edge Autonomy acquisition.", Founded = 2020, Hq = "Jacksonville, FL" },
        new() { Ticker = "BKSY",   Name = "BlackSky Technology",     Exchange = "NYSE",   AltKm = 450,    Zone = "leo",    Sectors = ["Imagery", "Defense"], X = 0.08, Icon = "imager",       Desc = "Real-time geospatial intelligence operator. Runs a constellation of high-revisit Gen-2 and Gen-3 imaging satellites for defense and commercial customers, paired with an AI analytics platform.", Founded = 2014, Hq = "Herndon, VA" },
        new() { Ticker = "PL",     Name = "Planet Labs",             Exchange = "NYSE",   AltKm = 500,    Zone = "leo",    Sectors = ["Imagery"],            X = 0.22, Icon = "imager",       Desc = "Operates the world's largest fleet of Earth observation satellites — Doves, SuperDoves, SkySats, and the upcoming Pelican / Tanager hyperspectral line — delivering daily global imagery as a subscription.", Founded = 2010, Hq = "San Francisco, CA" },
        new() { Ticker = "SPIR",   Name = "Spire Global",            Exchange = "NYSE",   AltKm = 520,    Zone = "leo",    Sectors = ["Sat ops", "Imagery"], X = 0.38, Icon = "cubesat",      Desc = "Space-as-a-service operator running ~100 LEMUR cubesats. Sells weather, maritime, aviation, and RF intelligence data, plus hosted-payload space services for third parties.", Founded = 2012, Hq = "Vienna, VA" },
        new() { Ticker = "MAXR",   Name = "Maxar Intelligence",      Exchange = "—",      AltKm = 617,    Zone = "leo",    Sectors = ["Imagery", "Defense"], X = 0.52, Icon = "imager",       Desc = "High-resolution Earth imagery and geospatial intelligence — WorldView Legion constellation. Taken private by Advent in 2023; included as a historical participant.", Founded = 1969, Hq = "Westminster, CO" },
        new() { Ticker = "ASTS",   Name = "AST SpaceMobile",         Exchange = "NASDAQ", AltKm = 700,    Zone = "leo",    Sectors = ["Comms"],              X = 0.66, Icon = "sat-big",      Desc = "Building a space-based cellular broadband network designed to connect ordinary smartphones directly to satellites — large phased-array BlueBird satellites in LEO, partnered with major terrestrial mobile carriers.", Founded = 2017, Hq = "Midland, TX" },
        new() { Ticker = "IRDM",   Name = "Iridium Communications",  Exchange = "NASDAQ", AltKm = 780,    Zone = "leo",    Sectors = ["Comms"],              X = 0.79, Icon = "constellation", Desc = "Operates a 66-satellite cross-linked LEO constellation providing global L-band voice and data services. Customers span maritime, aviation, IoT, defense, and emergency response.", Founded = 2001, Hq = "McLean, VA" },
        new() { Ticker = "GSAT",   Name = "Globalstar",              Exchange = "NYSE",   AltKm = 1414,   Zone = "leo",    Sectors = ["Comms"],              X = 0.90, Icon = "constellation", Desc = "LEO satellite constellation providing mobile voice and data services worldwide. Powers Apple's iPhone Emergency SOS via satellite and Apple Watch satellite connectivity. Pending acquisition by Amazon to support Project Kuiper infrastructure.", Founded = 1991, Hq = "Covington, LA" },
        new() { Ticker = "VSAT",   Name = "Viasat",                  Exchange = "NASDAQ", AltKm = 35786,  Zone = "deep",   Sectors = ["Comms", "Defense"],   X = 0.18, Icon = "geo-sat",      Desc = "Global satellite communications operator. Runs Ka-band ViaSat-3 and Inmarsat (acquired 2023) GEO fleets serving in-flight connectivity, government, maritime, and fixed broadband.", Founded = 1986, Hq = "Carlsbad, CA" },
        new() { Ticker = "TSAT",   Name = "Telesat",                 Exchange = "NASDAQ", AltKm = 35786,  Zone = "deep",   Sectors = ["Comms"],              X = 0.38, Icon = "geo-sat",      Desc = "Canadian GEO satellite operator with 13 geostationary satellites serving broadcast, enterprise, and government markets. Developing Telesat Lightspeed, a 198-satellite LEO broadband constellation.", Founded = 1969, Hq = "Ottawa, Canada" },
        new() { Ticker = "LUNR",   Name = "Intuitive Machines",      Exchange = "NASDAQ", AltKm = 384400, Zone = "deep",   Sectors = ["Sat ops"],            X = 0.55, Icon = "lander",       Desc = "Lunar and cislunar services provider. Operates the Nova-C lunar lander (first US lander on the Moon since Apollo) and is building data relay infrastructure between Earth and the lunar surface under NASA's CLPS and Near Space Network programs.", Founded = 2013, Hq = "Houston, TX" },
        new() { Ticker = "KRMN",   Name = "Karman Holdings",         Exchange = "NYSE",   AltKm = 0,      Zone = "launch", Sectors = ["Launch", "Defense"],  X = 0.14, Icon = "platform",     Desc = "Mission-critical manufacturing for space and defense. Produces thermal protection systems, composite structures, and high-performance components for launch vehicles, satellites, and hypersonic systems.", Founded = 2022, Hq = "Gardena, CA" },
        new() { Ticker = "SPACEX", Name = "SpaceX",                  Exchange = "—",      AltKm = 0,      Zone = "launch", Sectors = ["Launch", "Comms"],    X = 0.62, Icon = "rocket",       Desc = "World's most prolific launch provider operating Falcon 9, Falcon Heavy, and Starship. Also runs Starlink, the largest LEO broadband constellation with 6,000+ satellites. Privately held; IPO pending.", Founded = 2002, Hq = "Hawthorne, CA" },
    ];
}
