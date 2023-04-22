using System;

namespace AlgoTrader.Core.Interfaces
{
    /// <summary>
    /// Holds information about a candlestick
    /// </summary>
    public interface ICandlestick
    {
        /// <summary>
        /// Open time of the candlestick
        /// </summary>
        DateTime OpenTime { get; }

        /// <summary>
        /// Close time of the candlestick
        /// </summary>
        DateTime CloseTime { get; }

        /// <summary>
        /// Price the candlestick opened at
        /// </summary>
        double OpenPrice { get; }

        /// <summary>
        /// The high price of the candlestick
        /// </summary>
        double HighPrice { get; }

        /// <summary>
        /// The low price of the candlestick
        /// </summary>
        double LowPrice { get; }

        /// <summary>
        /// Price the candlestick closed at
        /// </summary>
        double ClosePrice { get; }

        /// <summary>
        /// Volume of the candlestick (in quote currency)
        /// </summary>
        double Volume { get; }
    }
}
