namespace Massive.Api.Models;

public class NewsArticle
{
    public string Id { get; set; } = null!;
    public string[] Tickers { get; set; } = [];
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ArticleUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public DateTime PublishedUtc { get; set; }
    public DateTime FetchedAt { get; set; }
}
