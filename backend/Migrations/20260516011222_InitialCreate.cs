using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Massive.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Exchange = table.Column<string>(type: "text", nullable: false),
                    AltKm = table.Column<int>(type: "integer", nullable: false),
                    Zone = table.Column<string>(type: "text", nullable: false),
                    Sectors = table.Column<string[]>(type: "text[]", nullable: false),
                    X = table.Column<double>(type: "double precision", nullable: false),
                    YOffset = table.Column<double>(type: "double precision", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Founded = table.Column<int>(type: "integer", nullable: true),
                    Hq = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Ticker);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Tickers = table.Column<string[]>(type: "text[]", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ArticleUrl = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Author = table.Column<string>(type: "text", nullable: true),
                    Publisher = table.Column<string>(type: "text", nullable: true),
                    PublishedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: true),
                    Change = table.Column<decimal>(type: "numeric", nullable: true),
                    ChangePct = table.Column<decimal>(type: "numeric", nullable: true),
                    High = table.Column<decimal>(type: "numeric", nullable: true),
                    Low = table.Column<decimal>(type: "numeric", nullable: true),
                    Open = table.Column<decimal>(type: "numeric", nullable: true),
                    Volume = table.Column<long>(type: "bigint", nullable: true),
                    MarketCap = table.Column<decimal>(type: "numeric", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Ticker);
                    table.ForeignKey(
                        name: "FK_Quotes_Companies_Ticker",
                        column: x => x.Ticker,
                        principalTable: "Companies",
                        principalColumn: "Ticker",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_PublishedUtc",
                table: "NewsArticles",
                column: "PublishedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsArticles");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
