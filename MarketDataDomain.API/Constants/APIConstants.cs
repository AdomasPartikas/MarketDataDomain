namespace MarketDataDomain.API.Constants
{
    public static class APIConstants
    {
        public const string FinnhubStockSymbolsEndpoint = "/stock/symbol?exchange=US";
        public const string FinnhubQuoteEndpoint = "/quote?symbol=";
        public const string FinnhubMarketStatusEndpoint = "/stock/market-status?exchange=US";
    }
}