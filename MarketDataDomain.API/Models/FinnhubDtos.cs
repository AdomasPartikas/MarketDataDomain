using Newtonsoft.Json;

namespace MarketDataDomain.API.Models
{
    /// <summary>
    /// Represents a stock symbol.
    /// </summary>
    public class StockSymbolDto
    {
        /// <summary>
        /// Gets or sets the currency of the stock.
        /// </summary>
        public required string Currency { get; set; }

        /// <summary>
        /// Gets or sets the description of the stock.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the display symbol of the stock.
        /// </summary>
        public required string DisplaySymbol { get; set; }

        /// <summary>
        /// Gets or sets the FIGI (Financial Instrument Global Identifier) of the stock.
        /// </summary>
        public required string Figi { get; set; }

        /// <summary>
        /// Gets or sets the MIC (Market Identifier Code) of the stock.
        /// </summary>
        public required string Mic { get; set; }

        /// <summary>
        /// Gets or sets the symbol of the stock.
        /// </summary>
        public required string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the type of the stock.
        /// </summary>
        public required string Type { get; set; }
    }

    /// <summary>
    /// Represents a stock quote.
    /// </summary>
    public class QuoteDto
    {
        /// <summary>
        /// Gets or sets the current price of the stock.
        /// </summary>
        [JsonProperty("c")]
        public decimal? CurrentPrice { get; set; }

        /// <summary>
        /// Gets or sets the change in price of the stock.
        /// </summary>
        [JsonProperty("d")]
        public decimal? Change { get; set; }

        /// <summary>
        /// Gets or sets the percentage change in price of the stock.
        /// </summary>
        [JsonProperty("dp")]
        public decimal? PercentChange { get; set; }

        /// <summary>
        /// Gets or sets the highest price of the stock for the day.
        /// </summary>
        [JsonProperty("h")]
        public decimal? HighPrice { get; set; }

        /// <summary>
        /// Gets or sets the lowest price of the stock for the day.
        /// </summary>
        [JsonProperty("l")]
        public decimal? LowPrice { get; set; }

        /// <summary>
        /// Gets or sets the opening price of the stock for the day.
        /// </summary>
        [JsonProperty("o")]
        public decimal? OpenPrice { get; set; }

        /// <summary>
        /// Gets or sets the previous closing price of the stock.
        /// </summary>
        [JsonProperty("pc")]
        public decimal? PreviousClosePrice { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the quote.
        /// </summary>
        [JsonProperty("t")]
        public long? Timestamp { get; set; }
    }
}