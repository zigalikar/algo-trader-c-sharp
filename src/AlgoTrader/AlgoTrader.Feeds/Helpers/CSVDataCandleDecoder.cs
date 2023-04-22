using System;
using System.Linq;

using AlgoTrader.Feeds.Core;
using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Feeds.Helpers
{
    /// <summary>
    /// Backtesting data decoder for CSV candle data files
    /// </summary>
    internal class CSVDataCandleDecoder : ICSVDataDecoder<ICandlestick>
    {
        public ICandlestick DecodeLine(string line)
        {
            var cols = line.Split(',').ToList<object>();
            cols[0] = DateTime.Parse(cols[0] as string);
            cols[1] = DateTime.Parse(cols[1] as string).ToUniversalTime();
            
            return new CSVCandlestick(cols);
        }
    }
}
