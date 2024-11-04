using MarketDataDomain.API.Services;
using MarketDataDomain.API.Models;
using MarketDataDomain.API.Profile;
using Moq;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Xunit;
using Xunit.Abstractions;

namespace MarketDataDomain.Tests;
public class ApiTests : IDisposable
{
    private readonly WireMockServer _wireMockServer;
    private readonly IFinnhubService _finnhubService;
    private readonly IMapper _mapper;
    private readonly Mock<ICachingService> _cachingServiceMock;
    private readonly ITestOutputHelper _output;

    public ApiTests(ITestOutputHelper output)
    {
        _output = output;
        _wireMockServer = WireMockServer.Start();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MarketDataProfile>();
        });
        _mapper = config.CreateMapper();
        _cachingServiceMock = new Mock<ICachingService>();

        var httpClient = new HttpClient { BaseAddress = new Uri(_wireMockServer.Url) };
        _output.WriteLine($"HttpClient BaseAddress: {httpClient.BaseAddress}");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "APITokens:Finnhub", "mock-token" },
            { "APIClients:FinnhubBaseUrl", _wireMockServer.Url } 
        }).Build();
        _output.WriteLine($"Finnhub Token: {configuration["APITokens:Finnhub"]}");
        _finnhubService = new FinnhubService(httpClient, configuration, _mapper, _cachingServiceMock.Object);
        SetupMockResponses();
    }

    private async Task TestHttpClientResponseAsync()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(_wireMockServer.Url) };
        var response = await httpClient.GetAsync("/stock/market-status?exchange=US");
        var responseBody = await response.Content.ReadAsStringAsync();
        
        _output.WriteLine($"Response Status: {response.StatusCode}, Body: {responseBody}");
    }
    
    private void SetupMockResponses()
    {
        _wireMockServer
                .Given(Request.Create()
                    .WithPath("/stock/symbols")
                    .WithParam("exchange", "US")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new List<StockSymbolDto>
                    {
                        new StockSymbolDto 
                    { 
                        Symbol = "AAPL", 
                        Description = "Apple Inc.", 
                        Currency = "USD", 
                        DisplaySymbol = "AAPL", 
                        Figi = "BBG000B9XRY4", 
                        Mic = "XNAS", 
                        Type = "Common Stock" 
                    },
                    new StockSymbolDto 
                    { 
                        Symbol = "GOOGL", 
                        Description = "Alphabet Inc.", 
                        Currency = "USD", 
                        DisplaySymbol = "GOOGL", 
                        Figi = "BBG009S3NB30", 
                        Mic = "XNAS", 
                        Type = "Common Stock" 
                    }
                    }));

            // Mock response for stock quote
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/quote")
                    .WithParam("symbol", "AAPL")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new QuoteDto 
                { 
                    CurrentPrice = 150.00m, 
                    HighPrice = 155.00m, 
                    LowPrice = 145.00m 
                }));

            // Mock response for market status
            _wireMockServer
                .Given(Request.Create()
                    .WithPath("/stock/market-status")
                    .WithParam("exchange", "US")
                    .UsingGet()
                    .WithHeader("X-Finnhub-Token", "mock-token"))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new MarketStatusDto 
                { 
                    Exchange = "NYSE", 
                    IsOpen = true, 
                    Timestamp = 123456,
                    Timezone = "EST"
                }));

            // Set up the caching service mock responses
            _cachingServiceMock.Setup(x => x.RetrieveStockSymbolsCache())
                .ReturnsAsync(new List<StockSymbolDto>
                {
                    new StockSymbolDto 
                    { 
                        Symbol = "AAPL", 
                        Description = "Apple Inc.", 
                        Currency = "USD", 
                        DisplaySymbol = "AAPL", 
                        Figi = "BBG000B9XRY4", 
                        Mic = "XNAS", 
                        Type = "Common Stock" 
                    },
                    new StockSymbolDto 
                    { 
                        Symbol = "GOOGL", 
                        Description = "Alphabet Inc.", 
                        Currency = "USD", 
                        DisplaySymbol = "GOOGL", 
                        Figi = "BBG009S3NB30", 
                        Mic = "XNAS", 
                        Type = "Common Stock" 
                    }
                });

            _cachingServiceMock.Setup(x => x.RetrieveMarketStatusCache())
                .ReturnsAsync(new MarketStatusDto 
            { 
                Exchange = "NYSE", 
                IsOpen = true, 
                Timezone = "EST"
            });

            _cachingServiceMock.Setup(x => x.RetrieveMarketDataCache())
                .ReturnsAsync(new List<MarketDataDto>());
    }

    [Fact]
    public async Task Test_GetStockSymbolsAsync()
    {
        var symbols = await _finnhubService.GetStockSymbolsAsync();
        Assert.NotNull(symbols);
        Assert.Equal(2, symbols.Count);
        Assert.Contains(symbols, s => s.Symbol == "AAPL");
        Assert.Contains(symbols, s => s.Symbol == "GOOGL");
        _output.WriteLine("gootest");
    }
    
    [Fact]
    public async Task Test_GetStockQuoteAsync()
    {
        var quote = await _finnhubService.GetStockQuoteAsync("AAPL");
        Assert.NotNull(quote);
        Assert.Equal(150.00m, quote.CurrentPrice);
    }

    [Fact]
    public async Task Test_GetMarketStatusAsync()
    {
        await TestHttpClientResponseAsync();
        var status = await _finnhubService.GetMarketStatusAsync();
        Assert.NotNull(status);
        Assert.True(status.IsOpen);
    }


    public void Dispose()
    {
        _wireMockServer.Stop();
        _wireMockServer.Dispose();
    }
}
