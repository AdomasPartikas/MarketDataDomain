using Hangfire;
using Hangfire.MemoryStorage;
using MarketDataDomain.API.Profile;
using MarketDataDomain.API.Services;
using MarketDataDomain.API.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(typeof(MarketDataProfile).Assembly);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MarketDataDomain.API", Version = "v1" });
});

builder.Services.AddHttpClient<IFinnhubService, FinnhubService>();
builder.Services.AddSingleton<ICachingService, CachingService>();

builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});

builder.Services.AddHangfireServer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var finnhubService = scope.ServiceProvider.GetRequiredService<IFinnhubService>();

    ProgressReporter.StartAwaitingNotifier();

    Console.WriteLine("Initializing cache");

    Console.WriteLine("Fetching market status from Finnhub API");
    await finnhubService.GetMarketStatusAsync();
    Console.WriteLine("Fetching available stocks from Finnhub API");
    await finnhubService.GetStockSymbolsAsync();
    Console.WriteLine("Fetching market data from Finnhub API\nThis might take a while...");
    await finnhubService.GetMarketDataAsync();

    await ProgressReporter.StopAwaitingNotifier();

    Console.WriteLine("Cache initialized. Other domains can now access the cache.");
}

app.UseHangfireDashboard();

#pragma warning disable CS0618 // Type or member is obsolete
app.UseHangfireServer();
#pragma warning restore CS0618 // Type or member is obsolete

// Schedule recurring jobs
RecurringJob.AddOrUpdate<IFinnhubService>(
    "refresh-stock-symbols",
    service => service.GetStockSymbolsAsync(),
    "0 0 * * *");

RecurringJob.AddOrUpdate<IFinnhubService>(
    "refresh-market-data",
    service => service.GetMarketDataAsync(),
    "*/1 * * * *");

RecurringJob.AddOrUpdate<IFinnhubService>(
    "refresh-market-status",
    service => service.GetMarketStatusAsync(),
    "*/5 * * * *");

app.UseRouting();
app.UseHttpsRedirection();
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
