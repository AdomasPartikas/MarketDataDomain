using Hangfire;
using Hangfire.MemoryStorage;
using MarketDataDomain.API.Profile;
using MarketDataDomain.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(typeof(MarketDataProfile).Assembly);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MarketDataDomain.API", Version = "v1" });
});

builder.Services.AddHttpClient<IFinnhubService, FinnhubService>();

builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});

builder.Services.AddHangfireServer();


var app = builder.Build();

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
    service => service.GetMarketDataAsync(false),
    "*/3 * * * *");

app.UseRouting();
app.UseHttpsRedirection();
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
