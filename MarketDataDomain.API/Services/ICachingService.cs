using Hangfire;
using MarketDataDomain.API.Models;

namespace MarketDataDomain.API.Services
{
    public interface ICachingService
    {
        Task<List<MarketDataDto>?> RetrieveMarketDataCache();
        Task<List<StockSymbolDto>?> RetrieveStockSymbolsCache();
        Task<MarketStatusDto?> RetrieveMarketStatusCache();
        void SetMarketDataCache(List<MarketDataDto> marketData);
        void SetStockSymbolsCache(List<StockSymbolDto> stockSymbols);
        void SetMarketStatusCache(MarketStatusDto marketStatus);
    }
}