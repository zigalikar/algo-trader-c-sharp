using System.Collections.Generic;

using AlgoTrader.Technical;
using AlgoTrader.Algos.Core;
using AlgoTrader.Core.Model;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Algo;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Backtest;
using AlgoTrader.Core.Model.Attributes;

namespace AlgoTrader.Algos
{
    [Algo("Simple Moving Averages", typeof(SMAAlgo))]
    public class SMAAlgo : AlgoBase
    {
        private SMA _sma1;
        private SMA _sma2;

        private IOrder _orderResponseDetails;

        public SMAAlgo(IFeed<ICandlestick> feed, IExchange exchange, ICurrencyPair currencyPair, TimeFrameEnum timeFrame, AlgoOptions options = null, int sma1Length = 20, int sma2Length = 50) : base(feed, exchange, currencyPair, timeFrame, options)
        {
            _sma1 = new SMA(feed, sma1Length);
            _sma2 = new SMA(feed, sma2Length);

            exchange.OrderFilled += (s, e) =>
            {
                if (e.Type.IsStop())
                    _orderResponseDetails = null;
            };
        }

        private double _last_sma1;
        private double _last_sma2;
        protected override async void OnData(ICandlestick data)
        {
            var sma1Value = _sma1.Calculate();
            var sma2Value = _sma2.Calculate();

            if (sma1Value != -1 && sma2Value != -1)
            {
                // check for cross
                if (_last_sma1 != 0 && _last_sma2 != 0 && _last_sma1 <= _last_sma2 && sma1Value > sma2Value)
                {
                    if (_orderResponseDetails == null)
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
                }
                else if (_last_sma1 != 0 && _last_sma2 != 0 && _last_sma1 >= _last_sma2 && sma1Value < sma2Value)
                {
                    if (_orderResponseDetails != null)
                    {
                        await Exchange.MarketOrder(CurrencyPair, _orderResponseDetails.Amount, OrderSide.Sell);
                        _orderResponseDetails = null;
                    }
                }

                _last_sma1 = sma1Value;
                _last_sma2 = sma2Value;
            }
        }

        public override IList<BacktestData> GetAdditionalBacktestData()
        {
            return new List<BacktestData>
            {
                new BacktestData<IList<DateValuePair>>("SMA20", _sma1.GetBacktestHistory<DateValuePair>()),
                new BacktestData<IList<DateValuePair>>("SMA50", _sma2.GetBacktestHistory<DateValuePair>()),
            };
        }
    }
}
