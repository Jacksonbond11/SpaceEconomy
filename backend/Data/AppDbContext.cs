using SpaceEconomy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace SpaceEconomy.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<NewsArticle> NewsArticles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(e =>
        {
            e.HasKey(c => c.Ticker);
            e.Property(c => c.Sectors).HasColumnType("text[]");
            e.HasOne(c => c.Quote)
             .WithOne(q => q.Company)
             .HasForeignKey<Quote>(q => q.Ticker);
        });

        modelBuilder.Entity<Quote>(e =>
        {
            e.HasKey(q => q.Ticker);
        });

        modelBuilder.Entity<NewsArticle>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Tickers).HasColumnType("text[]");
            e.HasIndex(n => n.PublishedUtc);
        });
    }
}
