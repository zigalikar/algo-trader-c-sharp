using System;
using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Feeds.Core
{
    /// <summary>
    /// Model for price tick data read from CSV files
    /// </summary>
    public class CSVPriceTick : IPriceTick
    {
        public DateTime Timestamp { get; private set; }
        public double Price { get; private set; }
        public double TradeSize { get; private set; }

        public CSVPriceTick(IEnumerable<object> array)
        {
            Timestamp = (DateTime) array.ElementAt(0);
            Price = double.Parse(array.ElementAt(1) as string);
            TradeSize = double.Parse(array.ElementAt(2) as string);
        }
    }
}
