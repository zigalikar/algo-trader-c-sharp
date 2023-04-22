using System.Linq;

using AlgoTrader.Feeds.Core;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Extensions;

namespace AlgoTrader.Feeds.Helpers
{
    /// <summary>
    /// Backtesting data decoder for CSV price tick data files
    /// </summary>
    internal class CSVDataPriceTickDecoder : ICSVDataDecoder<IPriceTick>
    {
        public IPriceTick DecodeLine(string line)
        {
            var cols = line.Split(';').ToList<object>();
            cols[0] = (long.Parse(cols[0] as string) / 1000).ToDateTime();

            return new CSVPriceTick(cols);
        }
    }
}
