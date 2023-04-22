using System.Linq;

using AlgoTrader.Algos.Core;
using AlgoTrader.Core.Model;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Algo;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Attributes;

namespace AlgoTrader.Algos
{
    /// <summary>
    /// Looks for more recent up bars than down bars. Then looks for a short term pullback to buy.
    /// </summary>
    [Algo("Directional Bars and Pullback Momentum", typeof(DBPM))]
    public class DBPM : AlgoBase
    {
        private readonly int _period;
        private readonly int _pullback;

        private IOrder _orderResponseDetails;

        public DBPM(IFeed<ICandlestick> feed, IExchange exchange, ICurrencyPair currencyPair, TimeFrameEnum timeFrame, AlgoOptions options = null, int period = 14, int pullback = 5) : base(feed, exchange, currencyPair, timeFrame, options)
        {
            _period = period;
            _pullback = pullback;

            exchange.OrderFilled += (s, e) =>
            {
                if (e.Type.IsStop())
                    _orderResponseDetails = null;
            };
        }

        protected override async void OnData(ICandlestick data)
        {
            var history = Feed.GetHistoryData(_period > _pullback + 1 ? _period : _pullback + 1);
            if (history != null)
            {
                var periodHistory = history.Take(_period);
                var upCloses = periodHistory.Where(h => h.ClosePrice > h.OpenPrice).Count();
                var downCloses = periodHistory.Where(h => h.ClosePrice < h.OpenPrice).Count();

                var lastPrice = data.ClosePrice;
                var pullbackPrice = history.ElementAt(_pullback).ClosePrice;

                var closes = history.Select(x => x.ClosePrice).ToList();
                var opens = history.Select(x => x.OpenPrice).ToList();

                if (_orderResponseDetails == null && upCloses > downCloses && lastPrice < pullbackPrice)
                {
                    var balances = Exchange.AvailableBalances;
                    if (balances.TryGetValue(CurrencyPair.Quote, out double balance))
                    {
                        var position = 0.85;
                        var price = data.ClosePrice;
                        var res = await Exchange.MarketOrder(CurrencyPair, balance / price * position, OrderSide.Buy);
                        _orderResponseDetails = res;
                    }
                }
                else if (_orderResponseDetails != null && downCloses > upCloses && lastPrice > pullbackPrice)
                {
                    await Exchange.MarketOrder(CurrencyPair, _orderResponseDetails.Amount, OrderSide.Sell);
                    _orderResponseDetails = null;
                }
            }
        }
    }
}
