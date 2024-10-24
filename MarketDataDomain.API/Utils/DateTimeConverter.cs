using System;

namespace MarketDataDomain.API.Utils
{
    /// <summary>
    /// Utility class for converting Unix timestamps to DateTime objects.
    /// </summary>
    public static class DateTimeConverter
    {
        /// <summary>
        /// Converts a Unix timestamp to a DateTime object.
        /// </summary>
        /// <param name="unixTimestamp">The Unix timestamp to convert.</param>
        /// <returns>A DateTime object representing the specified Unix timestamp.</returns>
        public static DateTime UnixTimestampToDateTime(long unixTimestamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTimestamp).ToLocalTime();
        }
    }
}