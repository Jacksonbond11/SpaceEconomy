using Massive.Api.Data;
using Massive.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHttpClient<PolygonService>();
builder.Services.AddHostedService<MarketDataRefreshService>();

builder.Services.AddControllers();
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p
        .WithOrigins(
            builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await Seeder.SeedAsync(db);
}

app.UseCors();
app.MapControllers();
app.Run();
