using MarketDataDomain.API.Models;

namespace MarketDataDomain.API.Services
{
    public interface IFinnhubService
    {
        Task<List<StockSymbolDto>?> GetStockSymbolsAsync();
        Task<QuoteDto?> GetStockQuoteAsync(string stockSymbols);
        Task<List<MarketDataDto>> GetMarketDataAsync(bool useCache = true);
    }
}