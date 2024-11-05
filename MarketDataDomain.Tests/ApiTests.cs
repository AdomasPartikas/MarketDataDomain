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
using Newtonsoft.Json;

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
        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Text")))
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Text"));
        }
        if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Text", "Top100Symbols.txt")))
        {
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Text", "Top100Symbols.txt"), "AAPL\nMSFT\nGOOGL"); // Add some default symbols for testing
        }
    }
    
    private void SetupMockResponses()
    {
        
            // Mock response for stock quote
            // _wireMockServer
            //     .Given(Request.Create()
            //         .WithPath("/quote")
            //         .WithParam("symbol", "AAPL")
            //         .UsingGet())
            //     .RespondWith(Response.Create()
            //         .WithStatusCode(200)
            //         .WithBodyAsJson(new QuoteDto 
            //     { 
            //         CurrentPrice = 150.00m, 
            //         HighPrice = 155.00m, 
            //         LowPrice = 145.00m 
            //     }));

            // _cachingServiceMock.Setup(x => x.RetrieveMarketDataCache())
            //     .ReturnsAsync(new List<MarketDataDto>());
    }

    [Fact]
    public async Task Test_GetStockSymbolsAsync_SuccessWithWellKnownSymbols()
    {
        _wireMockServer
                .Given(Request.Create()
                    .WithPath("/stock/symbol")
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

        // Act
        var symbols = await _finnhubService.GetStockSymbolsAsync();

        // Assert
        Assert.NotNull(symbols);
        Assert.True(symbols.Any(s => s.Symbol == "AAPL"));
        Assert.True(symbols.Any(s => s.Symbol == "GOOGL"));
        Assert.Equal(2, symbols.Count); // Assuming only AAPL and GOOGL are in Top100Symbols.txt
    }

    [Fact]
    public async Task Test_GetStockSymbolsAsync_UsesCacheIfAvailable()
    {
        // Arrange: Set up cache with data
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
                    },
            });

        // Act
        var symbols = await _finnhubService.GetStockSymbolsAsync();

        // Assert: Verify cache usage
        Assert.NotNull(symbols);
        Assert.Equal(2, symbols.Count);
        _cachingServiceMock.Verify(x => x.RetrieveStockSymbolsCache(), Times.Once);
    }

    [Fact]
    public async Task Test_GetStockSymbolsAsync_ApiReturnsError()
    {
        // Arrange: Mock API error response
        _wireMockServer
            .Given(Request.Create().WithPath("/stock/symbol").WithParam("exchange", "US").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

        // Act
        var symbols = await _finnhubService.GetStockSymbolsAsync();

        // Assert: Should return null on API error
        Assert.Null(symbols);
    }

    [Fact]
    public async Task Test_GetStockSymbolsAsync_EmptyApiResponse()
    {
        // Arrange: Mock empty API response
        _wireMockServer
            .Given(Request.Create().WithPath("/stock/symbol").WithParam("exchange", "US").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new List<StockSymbolDto>()));

        // Act
        var symbols = await _finnhubService.GetStockSymbolsAsync();

        // Assert: Should return empty list if no symbols are returned
        Assert.NotNull(symbols);
        Assert.Empty(symbols);
    }

    [Fact]
    public async Task Test_GetStockSymbolsAsync_WellKnownSymbolsFileNotFound()
    {
        // Arrange: Mock API response and remove or corrupt the well-known symbols file
        _wireMockServer
            .Given(Request.Create().WithPath("/stock/symbol").WithParam("exchange", "US").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new List<StockSymbolDto>
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
                    }
            }));

        var invalidFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Text", "Top100Symbols.txt");
        if (File.Exists(invalidFilePath))
            File.Delete(invalidFilePath);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await _finnhubService.GetStockSymbolsAsync());
    }

    [Fact]
    public async Task Test_GetStockSymbolsAsync_ApiResponseWithoutWellKnownSymbols()
    {
        // Arrange: Mock API response with symbols not in well-known symbols set
        _wireMockServer
            .Given(Request.Create().WithPath("/stock/symbol").WithParam("exchange", "US").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new List<StockSymbolDto>
            {
                new StockSymbolDto { Symbol = "XYZ", Description = "Unknown Corp", Currency = "USD", DisplaySymbol = "XYZ", Figi = "BBG000B9XRY4", Mic = "XNAS", Type = "Common Stock" }
            }));

        // Act
        var symbols = await _finnhubService.GetStockSymbolsAsync();

        // Assert: No well-known symbols should return an empty list
        Assert.NotNull(symbols);
        Assert.Empty(symbols);
    }

    [Fact]
    public async Task Test_GetStockSymbolsAsync_CacheInvalidation()
    {
        // Arrange: Set cache to be empty and mock API response
        _cachingServiceMock.Setup(x => x.RetrieveStockSymbolsCache()).ReturnsAsync(new List<StockSymbolDto>());
        _wireMockServer
            .Given(Request.Create().WithPath("/stock/symbol").WithParam("exchange", "US").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new List<StockSymbolDto>
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

        // Act
        var symbols = await _finnhubService.GetStockSymbolsAsync();

        // Assert: Verify API is called if cache is empty
        Assert.NotNull(symbols);
        Assert.Equal(2, symbols.Count);
        _cachingServiceMock.Verify(x => x.RetrieveStockSymbolsCache(), Times.Once);
        _cachingServiceMock.Verify(x => x.SetStockSymbolsCache(It.IsAny<List<StockSymbolDto>>()), Times.Once);
    }

    [Fact]
    public async Task Test_GetStockQuoteAsync_RetryMechanism()
    {
        // Arrange - simulate initial 429 response followed by a success response
        _wireMockServer
            .Given(Request.Create().WithPath("/quote").WithParam("symbol", "AAPL").UsingGet())
            .InScenario("Retry Scenario")
            .WillSetStateTo("Rate Limited")
            .RespondWith(Response.Create().WithStatusCode(429)); // Too Many Requests
        _wireMockServer
            .Given(Request.Create().WithPath("/quote").WithParam("symbol", "AAPL").UsingGet())
            .InScenario("Retry Scenario")
            .WhenStateIs("Rate Limited")
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new QuoteDto
            {
                CurrentPrice = 150.00m,
                HighPrice = 155.00m,
                LowPrice = 145.00m
            }));

        // Act
        var quote = await _finnhubService.GetStockQuoteAsync("AAPL");

        // Assert
        Assert.NotNull(quote);
        Assert.Equal(150.00m, quote.CurrentPrice);
    }

    [Fact]
    public async Task Test_GetStockQuoteAsync_InvalidStockSymbol()
    {
        // Arrange
        var invalidSymbol = "INVALID";
        _wireMockServer
            .Given(Request.Create().WithPath("/quote").WithParam("symbol", invalidSymbol).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404)); // Not Found

        // Act
        var quote = await _finnhubService.GetStockQuoteAsync(invalidSymbol);

        // Assert
        Assert.Null(quote); // Expect null for an invalid stock symbol
    }

    [Fact]
    public async Task Test_GetStockQuoteAsync_InvalidJsonResponse()
    {
        // Arrange
        _wireMockServer
            .Given(Request.Create().WithPath("/quote").WithParam("symbol", "AAPL").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("Invalid JSON format"));

        // Act
        var exception = await Assert.ThrowsAsync<JsonReaderException>(
        async () => await _finnhubService.GetStockQuoteAsync("AAPL"));

        // Optionally, you can check the message or other properties of the exception
        Assert.Contains("Error parsing", exception.Message); // Expect null since JSON is not valid
    }

    [Fact]
    public async Task Test_GetStockQuoteAsync_500InternalServerError()
    {
        // Arrange
        _wireMockServer
            .Given(Request.Create().WithPath("/quote").WithParam("symbol", "AAPL").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500)); // Internal Server Error

        // Act
        var quote = await _finnhubService.GetStockQuoteAsync("AAPL");

        // Assert
        Assert.Null(quote); // Expect null due to server error
    }

    [Fact]
    public async Task Test_GetStockQuoteAsync_SuccessfulResponseWithDifferentData()
    {
        // Arrange
        _wireMockServer
            .Given(Request.Create().WithPath("/quote").WithParam("symbol", "AAPL").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new QuoteDto
            {
                CurrentPrice = 160.00m,
                HighPrice = 165.00m,
                LowPrice = 155.00m
            }));

        // Act
        var quote = await _finnhubService.GetStockQuoteAsync("AAPL");

        // Assert
        Assert.NotNull(quote);
        Assert.Equal(160.00m, quote.CurrentPrice); // Check that the current price is as expected
        Assert.Equal(165.00m, quote.HighPrice);    // Additional checks for high price
        Assert.Equal(155.00m, quote.LowPrice);      // Additional checks for low price
    }

    [Fact]
    public async Task Test_GetStockQuoteAsync_MultipleConsecutiveRequests()
    {
        // Arrange
        _wireMockServer
            .Given(Request.Create().WithPath("/quote").WithParam("symbol", "AAPL").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new QuoteDto
            {
                CurrentPrice = 150.00m,
                HighPrice = 155.00m,
                LowPrice = 145.00m
            }));

        // Act
        var quote1 = await _finnhubService.GetStockQuoteAsync("AAPL");
        var quote2 = await _finnhubService.GetStockQuoteAsync("AAPL");

        // Assert
        Assert.NotNull(quote1);
        Assert.NotNull(quote2);
        Assert.Equal(150.00m, quote1.CurrentPrice);
        Assert.Equal(150.00m, quote2.CurrentPrice); // Ensure consecutive calls yield the same result
    }

[Fact]
public async Task Test_GetMarketStatusAsync_Open()
{
    // Arrange: Mock the response for a market open status
    _wireMockServer
        .Given(Request.Create().WithPath("/stock/market-status").UsingGet())
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(new MarketStatusDto
            {
                Exchange = "NYSE",
                IsOpen = true,
                Timestamp = 123456,
                Timezone = "EST"
            }));

    // Act: Call the method
    var status = await _finnhubService.GetMarketStatusAsync();

    // Assert: Verify that the response is not null and that the market is open
    Assert.NotNull(status);
    Assert.True(status.IsOpen);
}

[Fact]
public async Task Test_GetMarketStatusAsync_Closed()
{
    // Arrange: Mock the response for a market closed status
    _wireMockServer
        .Given(Request.Create().WithPath("/stock/market-status").UsingGet())
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(new MarketStatusDto
            {
                Exchange = "NYSE",
                IsOpen = false,
                Timestamp = 123456,
                Timezone = "EST"
            }));

    // Act: Call the method
    var status = await _finnhubService.GetMarketStatusAsync();

    // Assert: Verify that the response is not null and that the market is closed
    Assert.NotNull(status);
    Assert.False(status.IsOpen);
}

[Fact]
public async Task Test_GetMarketStatusAsync_RetryOnRateLimit()
{
    // Arrange: Simulate rate limiting followed by a successful response
    _wireMockServer
        .Given(Request.Create().WithPath("/stock/market-status").UsingGet())
        .InScenario("Retry Scenario")
        .WillSetStateTo("Rate Limited")
        .RespondWith(Response.Create().WithStatusCode(429)); // Too Many Requests

    _wireMockServer
        .Given(Request.Create().WithPath("/stock/market-status").UsingGet())
        .InScenario("Retry Scenario")
        .WhenStateIs("Rate Limited")
        .RespondWith(Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(new MarketStatusDto
            {
                Exchange = "NYSE",
                IsOpen = true,
                Timestamp = 123456,
                Timezone = "EST"
            }));

    // Act: Call the method
    var status = await _finnhubService.GetMarketStatusAsync();

    // Assert: Verify that the response is not null and that the market is open
    Assert.NotNull(status);
    Assert.True(status.IsOpen);
}

[Fact]
public async Task Test_GetMarketStatusAsync_UnsuccessfulResponse()
{
    // Arrange: Mock an unsuccessful response
    _wireMockServer
        .Given(Request.Create().WithPath("/stock/market-status").UsingGet())
        .RespondWith(Response.Create().WithStatusCode(500)); // Internal Server Error

    // Act: Call the method
    var status = await _finnhubService.GetMarketStatusAsync();

    // Assert: Verify that the response is null due to the unsuccessful status code
    Assert.Null(status);
}


    public void Dispose()
    {
        _wireMockServer.Stop();
        _wireMockServer.Dispose();
    }
}
