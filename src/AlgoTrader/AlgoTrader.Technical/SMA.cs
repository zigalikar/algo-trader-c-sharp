using System;
using System.Linq;

using AlgoTrader.Core.Model;
using AlgoTrader.Technical.Core;
using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Technical
{
    /// <summary>
    /// Simple Moving Average technical indicator class
    /// </summary>
    public class SMA : TechnicalIndicatorBase
    {
        public int Length { get; private set; }
        public SMAType Type { get; private set; }

        public SMA(IFeed<ICandlestick> feed, int length, SMAType type = SMAType.Close) : base(feed)
        {
            Length = length;
            Type = type;
        }

        /// <summary>
        /// Calculates the SMA
        /// </summary>
        /// <returns>SMA value or -1 if not enough data in feed</returns>
        public double Calculate()
        {
            var data = Feed.GetHistoryData(Length);
            if (data != null)
            {
                // save averages
                var ma = data.Average(x =>
                {
                    if (Type == SMAType.High)
                        return x.HighPrice;
                    else if (Type == SMAType.Low)
                        return x.LowPrice;
                    else if (Type == SMAType.Close)
                        return x.ClosePrice;
                    else
                        return x.OpenPrice;
                });

                if (IsBacktest)
                {
                    // save dates
                    DateTime? dt = null;
                    var last = data.First();
                    if (Type == SMAType.High)
                        throw new NotImplementedException();
                    else if (Type == SMAType.Low)
                        throw new NotImplementedException();
                    else if (Type == SMAType.Close)
                        dt = last.CloseTime;
                    else
                        dt = last.OpenTime;

                    SaveBacktestHistory(new DateValuePair(dt.Value, ma));
                }

                return ma;
            }

            return -1;
        }
    }

    public enum SMAType
    {
        Open = 0,
        High = 1,
        Low = 2,
        Close = 3
    }

    public enum SMACrossType
    {
        Upwards = 0,
        Downwards = 1
    }
}
