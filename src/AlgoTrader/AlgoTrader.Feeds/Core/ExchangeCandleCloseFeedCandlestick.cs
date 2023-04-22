using System;

using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Feeds.Core
{
    public class ExchangeCandleCloseFeedCandlestick : ICandlestick
    {
        public DateTime OpenTime { get; }
        public DateTime CloseTime { get; }
        public double OpenPrice { get; }
        public double HighPrice { get; }
        public double LowPrice { get; }
        public double ClosePrice { get; }
        public double Volume { get; }

        public ExchangeCandleCloseFeedCandlestick(DateTime openTime, DateTime closeTime, double openPrice, double highPrice, double lowPrice, double closePrice, double volume)
        {
            OpenTime = openTime;
            CloseTime = closeTime;
            OpenPrice = openPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            ClosePrice = closePrice;
            Volume = volume;
        }
    }
}
