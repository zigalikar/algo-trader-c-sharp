using System;

using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Exchanges.Core
{
    public class BacktestResultCandlestick : ICandlestick
    {
        public DateTime OpenTime { get; private set; }
        public DateTime CloseTime { get; private set; }
        public double OpenPrice { get; private set; }
        public double HighPrice { get; private set; }
        public double LowPrice { get; private set; }
        public double ClosePrice { get; private set; }
        public double Volume { get; private set; }

        public BacktestResultCandlestick(DateTime openTime, DateTime closeTime, double openPrice, double highPrice, double lowPrice, double closePrice, double volume)
        {
            OpenTime = openTime;
            CloseTime = closeTime;
            OpenPrice = openPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            ClosePrice = closePrice;
            Volume = volume;
        }

        public BacktestResultCandlestick(DateTime openTime, DateTime closeTime, double price, double volume)
        {
            OpenTime = openTime;
            CloseTime = closeTime;
            OpenPrice = price;
            HighPrice = price;
            LowPrice = price;
            ClosePrice = price;
            Volume = volume;
        }
    }
}
