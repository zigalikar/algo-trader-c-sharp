using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Core.DTO.Binance
{
    /// <summary>
    /// Wrapper for the response array from the Binance API about instrument prices
    /// </summary>
    public class CandlestickDataResponse : ICandlestick
    {
        public DateTime OpenTime { get; }
        public double OpenPrice { get; }
        public double HighPrice { get; }
        public double LowPrice { get; }
        public double ClosePrice { get; }
        public double BaseAssetVolume { get; }
        public DateTime CloseTime { get; }
        public double Volume { get; }
        public long NumberOfTrades { get; }
        public double TakerBuyBaseAssetVolume { get; }
        public double TakerBuyQuoteAssetVolume { get; }

        private CandlestickDataResponse(IEnumerable<object> array) : this(
                ((long) array.ElementAt(0) / 1000).ToDateTime(),
                double.Parse(array.ElementAt(1) as string, CultureInfo.InvariantCulture),
                double.Parse(array.ElementAt(2) as string, CultureInfo.InvariantCulture),
                double.Parse(array.ElementAt(3) as string, CultureInfo.InvariantCulture),
                double.Parse(array.ElementAt(4) as string, CultureInfo.InvariantCulture),
                double.Parse(array.ElementAt(5) as string, CultureInfo.InvariantCulture),
                ((long) array.ElementAt(6) / 1000).ToDateTime(),
                double.Parse(array.ElementAt(7) as string, CultureInfo.InvariantCulture),
                (long) array.ElementAt(8),
                double.Parse(array.ElementAt(9) as string, CultureInfo.InvariantCulture),
                double.Parse(array.ElementAt(10) as string, CultureInfo.InvariantCulture)
            )
        { }

        //private CandlestickDataResponse(IEnumerable<object> array)
        //{
        //    OpenTime = ((long)array.ElementAt(0)).ToDateTime();
        //    OpenPrice = double.Parse(array.ElementAt(1) as string, CultureInfo.InvariantCulture);
        //    HighPrice = double.Parse(array.ElementAt(2) as string, CultureInfo.InvariantCulture);
        //    LowPrice = double.Parse(array.ElementAt(3) as string, CultureInfo.InvariantCulture);
        //    ClosePrice = double.Parse(array.ElementAt(4) as string, CultureInfo.InvariantCulture);
        //    BaseAssetVolume = double.Parse(array.ElementAt(5) as string, CultureInfo.InvariantCulture);
        //    CloseTime = ((long)array.ElementAt(6)).ToDateTime();
        //    Volume = double.Parse(array.ElementAt(7) as string, CultureInfo.InvariantCulture);
        //    NumberOfTrades = (long) array.ElementAt(8);
        //    TakerBuyBaseAssetVolume = double.Parse(array.ElementAt(9) as string, CultureInfo.InvariantCulture);
        //    TakerBuyQuoteAssetVolume = double.Parse(array.ElementAt(10) as string, CultureInfo.InvariantCulture);
        //}

        private CandlestickDataResponse(DateTime openTime, double openPrice, double highPrice, double lowPrice, double closePrice, double baseAssetVolume, DateTime closeTime, double volume, long numberOfTrades, double takerBuyBaseAssetVolume, double takerBuyQuoteAssetVolume)
        {
            OpenTime = openTime;
            OpenPrice = openPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            ClosePrice = closePrice;
            BaseAssetVolume = baseAssetVolume;
            CloseTime = closeTime;
            Volume = volume;
            NumberOfTrades = numberOfTrades;
            TakerBuyBaseAssetVolume = takerBuyBaseAssetVolume;
            TakerBuyQuoteAssetVolume = takerBuyQuoteAssetVolume;
        }
        
        /// <summary>
        /// Creates a new instance of the Binance candlestick data response object from the response array from the Binance API
        /// </summary>
        /// <param name="array">The array returned from the Binance API</param>
        public static CandlestickDataResponse FromArrayResponse(IEnumerable<object> array) => new CandlestickDataResponse(array);

        /// <summary>
        /// Creates a new instance of the Binance candlestick data response object from raw property values
        /// </summary>
        public static CandlestickDataResponse FromValues(DateTime openTime, double openPrice, double highPrice, double lowPrice, double closePrice, double baseAssetVolume, DateTime closeTime, double volume, long numberOfTrades, double takerBuyBaseAssetVolume, double takerBuyQuoteAssetVolume) => new CandlestickDataResponse(openTime, openPrice, highPrice, lowPrice, closePrice, baseAssetVolume, closeTime, volume, numberOfTrades, takerBuyBaseAssetVolume, takerBuyQuoteAssetVolume);
    }
}
