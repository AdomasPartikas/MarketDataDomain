using Hangfire;
using MarketDataDomain.API.Models;

namespace MarketDataDomain.API.Services
{
    [DisableConcurrentExecution(timeoutInSeconds: 10)]
    [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public interface IFinnhubService
    {
        Task<List<StockSymbolDto>?> GetStockSymbolsAsync();
        Task<QuoteDto?> GetStockQuoteAsync(string stockSymbols);
        Task<List<MarketDataDto>> GetMarketDataAsync();
        Task<MarketStatusDto> GetMarketStatusAsync();
        Task<List<MarketDataDto>?> RetrieveMarketDataCache();
    }
}