namespace MarketDataDomain.API.Models
{
    /// <summary>
    /// Represents market data for a stock.
    /// </summary>
    public class MarketDataDto
    {
        // Stock Symbol Information

        /// <summary>
        /// Gets or sets the stock symbol.
        /// </summary>
        public string Symbol { get; set; } = "";
        /// <summary>
        /// Gets or sets the description of the stock.
        /// </summary>
        public string Description { get; set; } = "";
        /// <summary>
        /// Gets or sets the currency of the stock.
        /// </summary>
        public string Currency { get; set; } = "";

        // Stock Quote Information

        /// <summary>
        /// Gets or sets the current price of the stock.
        /// </summary>
        public decimal? CurrentPrice { get; set; }
        /// <summary>
        /// Gets or sets the highest price of the stock for the day.
        /// </summary>
        public decimal? HighPrice { get; set; }
        /// <summary>
        /// Gets or sets the lowest price of the stock for the day.
        /// </summary>
        public decimal? LowPrice { get; set; }
        /// <summary>
        /// Gets or sets the opening price of the stock for the day.
        /// </summary>
        public decimal? OpenPrice { get; set; }
        /// <summary>
        /// Gets or sets the previous closing price of the stock.
        /// </summary>
        public decimal? PreviousClosePrice { get; set; }
        /// <summary>
        /// Gets or sets the change in price of the stock.
        /// </summary>
        public decimal? Change { get; set; }
        /// <summary>
        /// Gets or sets the percentage change in price of the stock.
        /// </summary>
        public decimal? PercentChange { get; set; }

        // Additional fields

        /// <summary>
        /// Gets or sets the timestamp of the quote.
        /// </summary>
        public DateTime Date { get; set; }  // The timestamp of the quote
    }
}