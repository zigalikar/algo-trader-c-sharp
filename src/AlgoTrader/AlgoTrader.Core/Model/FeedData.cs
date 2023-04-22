using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Core.Model
{
    public class FeedData
    {
        public ICandlestick Candlestick { get; }

        private readonly IList<Trade> _trades;
        public IList<Trade> Trades => _trades.ToList();

        public FeedData(ICandlestick candlestick, IList<Trade> trades)
        {
            Candlestick = candlestick;
            _trades = (_trades ?? new List<Trade>()).ToList();
        }
    }
}
