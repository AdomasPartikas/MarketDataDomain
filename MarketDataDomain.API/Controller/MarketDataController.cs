
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
    public class MarketDataController(ICachingService cachingService) : ControllerBase
    {
        private readonly ICachingService _cachingService = cachingService;

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

            var marketData = await _cachingService.RetrieveMarketDataCache();

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
            var stockSymbols = await _cachingService.RetrieveStockSymbolsCache();

            if (stockSymbols == null || stockSymbols.Count == 0)
                return NotFound("No stock symbols found.");

            return Ok(stockSymbols);
        }

        /// <summary>
        /// Gets the market status.
        /// </summary>
        /// <returns>The market status.</returns>
        [HttpGet("marketstatus")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MarketStatusDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMarketStatus()
        {
            var marketStatus = await _cachingService.RetrieveMarketStatusCache();

            if (marketStatus == null)
                return NotFound("Market status not found.");

            return Ok(marketStatus);
        }
    }
}