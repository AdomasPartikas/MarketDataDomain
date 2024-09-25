using MarketDataDomain.API.Models;

namespace MarketDataDomain.API.Records
{
    public class FinnhubGroup
    {
        public required QuoteDto Quotes { get; set; }
        public required StockSymbolDto StockSymbols { get; set; }
    }
}