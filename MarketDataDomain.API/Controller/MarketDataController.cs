
using MarketDataDomain.API.Models;
using MarketDataDomain.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketDataDomain.API.Controller
{
    /// <summary>
    /// Controller for handling market data related requests.
    /// </summary>
    [Route("api")]
    [ApiController]
    /// <summary>
    /// Initializes a new instance of the <see cref="MarketDataController"/> class.
    /// </summary>
    /// <param name="finnhubService">The service for interacting with Finnhub API.</param>
    public class MarketDataController(IFinnhubService finnhubService) : ControllerBase
    {
        private readonly IFinnhubService _finnhubService = finnhubService;

        /// <summary>
        /// Gets the market data.
        /// </summary>
        /// <returns>The market data.</returns>
        [HttpGet("marketdata")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MarketDataDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetMarketData()
        {
            Console.WriteLine("Market data requested");

            var marketData = await _finnhubService.GetMarketDataAsync();

            if (marketData == null)
                return NoContent();

            return Ok(marketData);
        }

        /// <summary>
        /// Gets the stock symbols.
        /// </summary>
        /// <returns>The list of stock symbols.</returns>
        [HttpGet("stocksymbols")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<StockSymbolDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStockSymbols()
        {
            var stockSymbols = await _finnhubService.GetStockSymbolsAsync();

            if (stockSymbols == null || stockSymbols.Count == 0)
                return NotFound("No stock symbols found.");

            return Ok(stockSymbols);
        }

        /// <summary>
        /// Gets the stock quote for a specific symbol.
        /// </summary>
        /// <param name="symbol">The stock symbol.</param>
        /// <returns>The stock quote.</returns>
        [HttpGet("quote/{symbol}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(QuoteDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStockQuote(string symbol)
        {
            if (symbol == null)
                return BadRequest("Invalid stock symbol.");

            var quotes = await _finnhubService.GetStockQuoteAsync(symbol);

            if (quotes == null)
                return NoContent();

            return Ok(quotes);
        }
    }
}