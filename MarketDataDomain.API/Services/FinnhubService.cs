using System.Text.Json.Serialization;
using AutoMapper;
using Hangfire;
using MarketDataDomain.API.Constants;
using MarketDataDomain.API.Models;
using MarketDataDomain.API.Records;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace MarketDataDomain.API.Services
{
    public class FinnhubService(HttpClient httpClient, IConfiguration configuration, IMapper mapper, IMemoryCache cache) : IFinnhubService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly string _apiToken = configuration["APITokens:Finnhub"] ?? throw new ArgumentNullException("APITokens:Finnhub");
        private readonly string _finnhubBaseUrl = APIConstants.FinnhubBaseUrl;
        private readonly IMapper _mapper = mapper;
        private readonly IMemoryCache _cache = cache;

        public async Task<List<StockSymbolDto>?> GetStockSymbolsAsync()
        {
            if (_cache.TryGetValue(CacheConstanct.StockSymbolsCacheKey, out List<StockSymbolDto>? cachedStockSymbols))
            {
                Console.WriteLine("Retrieving stock symbols from cache");
                return cachedStockSymbols;
            }

            var apiUrl = $"{_finnhubBaseUrl}{APIConstants.FinnhubStockSymbolsEndpoint}";
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Finnhub-Token", _apiToken);
            
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                return null; 

            var jsonData = await response.Content.ReadAsStringAsync();

            var stockSymbols = JsonConvert.DeserializeObject<List<StockSymbolDto>>(jsonData);

            var wellKnownSymbolsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Text", "Top100Symbols.txt");
            var wellKnownSymbols = await File.ReadAllLinesAsync(wellKnownSymbolsFilePath);
            var wellKnownSymbolsSet = new HashSet<string>(wellKnownSymbols);

            stockSymbols = stockSymbols!.Where(symbol => wellKnownSymbols.Contains(symbol.Symbol)).ToList();

            _cache.Set(CacheConstanct.StockSymbolsCacheKey, stockSymbols, TimeSpan.FromHours(24));
            Console.WriteLine("Caching stock symbols for 24 hours");

            return stockSymbols;
        }

        public async Task<QuoteDto?> GetStockQuoteAsync(string stockSymbol)
        {
            var apiUrl = $"{_finnhubBaseUrl}{APIConstants.FinnhubQuoteEndpoint}";
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Finnhub-Token", _apiToken);

            var response = await GetResponseWithRetriesAsync(apiUrl + stockSymbol, GlobalConstants.DelayDueToApiRequestLimit, GlobalConstants.DefaultRetries);

            if (!response.IsSuccessStatusCode)
                return null;

            var jsonData = await response.Content.ReadAsStringAsync();

            var quote = JsonConvert.DeserializeObject<QuoteDto>(jsonData);

            return quote;
        }

        public async Task<List<MarketDataDto>> GetMarketDataAsync()
        {
            Console.WriteLine($"Starting GetMarketDataAsync()");

            if (_cache.TryGetValue(CacheConstanct.MarketStatusCacheKey, out MarketStatusDto? cachedMarketStatus))
            {
                if (!cachedMarketStatus!.IsOpen)
                {
                    Console.WriteLine($"Stopping GetMarketDataAsync() due to market closed");
                    return [];
                }
            }
            else
            {
                Console.WriteLine("Starting GetMarketStatusAsync");
                await GetMarketStatusAsync();
            }

            var stockSymbols = await GetStockSymbolsAsync();
            var marketData = new List<MarketDataDto>();

            foreach (var symbol in stockSymbols!)
            {
                var quote = await GetStockQuoteAsync(symbol.Symbol);

                await Task.Delay(GlobalConstants.DelayBetweenRequestsInMiliseconds);

                if (quote != null)
                    marketData.Add(_mapper.Map<MarketDataDto>(new FinnhubGroup() { StockSymbols = symbol, Quotes = quote }));
                else
                    Console.WriteLine($"Failed to retrieve quote for symbol: {symbol.Symbol}");
    
            }

            _cache.Set(CacheConstanct.MarketDataCacheKey, marketData, TimeSpan.FromHours(24));
            Console.WriteLine("Caching market data for 24 hours");

            return marketData;
        }

        public async Task<List<MarketDataDto>?> RetrieveMarketDataCache()
        {
            if (_cache.TryGetValue(CacheConstanct.MarketDataCacheKey, out List<MarketDataDto>? cachedMarketData))
                return cachedMarketData;
            else
                return [];
        }

        private async Task<HttpResponseMessage> GetResponseWithRetriesAsync(string requestUrl, int delay, int retries)
        {
            HttpResponseMessage response;
            int retryCount = 0;

            Console.WriteLine($"Starting GetResponseWithRetriesAsync(requestUrl = {requestUrl}, delay = {delay}, retries = {retries})");

            do
            {
                response = await _httpClient.GetAsync(requestUrl);

                //Console.WriteLine($"Sending request to {requestUrl}");

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    retryCount++;
                    Console.WriteLine($"Rate limit exceeded (429). Retrying in {delay} seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                }
                else
                {
                    break; // Exit the loop if not 429
                }

            } while (retryCount < retries);

            return response;
        }

        public async Task<MarketStatusDto> GetMarketStatusAsync()
        {
            var apiUrl = $"{_finnhubBaseUrl}{APIConstants.FinnhubMarketStatusEndpoint}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Finnhub-Token", _apiToken);

            var response = await GetResponseWithRetriesAsync(apiUrl, 10, 10);

            if (!response.IsSuccessStatusCode)
                return new MarketStatusDto(){ IsOpen = false, Exchange = "US", Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Timezone = "America/New_York" };

            var jsonData = await response.Content.ReadAsStringAsync();

            var marketStatus = JsonConvert.DeserializeObject<MarketStatusDto>(jsonData);

            _cache.Set(CacheConstanct.MarketStatusCacheKey, marketStatus, TimeSpan.FromHours(24));

            if(marketStatus!.IsOpen)
                Console.WriteLine($"{DateTime.Now} - Market is open");
            else
                Console.WriteLine($"{DateTime.Now} - Market is closed");

            return marketStatus;
        }
    }
}