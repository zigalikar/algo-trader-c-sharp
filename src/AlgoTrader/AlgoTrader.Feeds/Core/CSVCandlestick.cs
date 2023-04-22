using System;
using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Feeds.Core
{
    /// <summary>
    /// Model for candlestick data read from CSV files
    /// </summary>
    public class CSVCandlestick : ICandlestick
    {
        public DateTime OpenTime { get; private set; }
        public DateTime CloseTime { get; private set; }
        public double OpenPrice { get; private set; }
        public double HighPrice { get; private set; }
        public double LowPrice { get; private set; }
        public double ClosePrice { get; private set; }
        public double Volume { get; private set; }

        public CSVCandlestick(IEnumerable<object> array)
        {
            OpenTime = (DateTime) array.ElementAt(0);
            CloseTime = (DateTime) array.ElementAt(1);
            OpenPrice = double.Parse(array.ElementAt(2) as string);
            HighPrice = double.Parse(array.ElementAt(3) as string);
            LowPrice = double.Parse(array.ElementAt(4) as string);
            ClosePrice = double.Parse(array.ElementAt(5) as string);
            //AdjustedClose = double.Parse(array.ElementAt(6) as string);
            Volume = double.Parse(array.ElementAt(7) as string);
        }
    }
}
