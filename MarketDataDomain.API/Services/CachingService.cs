using MarketDataDomain.API.Constants;
using MarketDataDomain.API.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MarketDataDomain.API.Services
{
    public class CachingService(IMemoryCache cache) : ICachingService
    {
        private readonly IMemoryCache _cache = cache;

        public async Task<List<MarketDataDto>?> RetrieveMarketDataCache()
        {
            if (_cache.TryGetValue(CacheConstanct.MarketDataCacheKey, out List<MarketDataDto>? cachedMarketData))
                return cachedMarketData;
            else
                return null;
        }

        public async Task<List<StockSymbolDto>?> RetrieveStockSymbolsCache()
        {
            if (_cache.TryGetValue(CacheConstanct.StockSymbolsCacheKey, out List<StockSymbolDto>? cachedStockSymbols))
                return cachedStockSymbols;
            else
                return null;
        }

        public async Task<MarketStatusDto?> RetrieveMarketStatusCache()
        {
            if (_cache.TryGetValue(CacheConstanct.MarketStatusCacheKey, out MarketStatusDto? cachedMarketStatus))
                return cachedMarketStatus;
            else
                return null;
        }

        public void SetMarketDataCache(List<MarketDataDto> marketData)
        {
            _cache.Set(CacheConstanct.MarketDataCacheKey, marketData, TimeSpan.FromHours(24));
            Console.WriteLine("Caching market data for 5 minutes");
        }

        public void SetStockSymbolsCache(List<StockSymbolDto> stockSymbols)
        {
            _cache.Set(CacheConstanct.StockSymbolsCacheKey, stockSymbols, TimeSpan.FromHours(24));
            Console.WriteLine("Caching stock symbols for 24 hours");
        }

        public void SetMarketStatusCache(MarketStatusDto marketStatus)
        {
            _cache.Set(CacheConstanct.MarketStatusCacheKey, marketStatus, TimeSpan.FromMinutes(30));
            Console.WriteLine("Caching market status for 30 minutes");
        }
    }
}