using System;
using System.Linq;
using System.Threading.Tasks;

using AlgoTrader.Core.Model;
using AlgoTrader.Feeds.Core;
using AlgoTrader.Exchanges.Core;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Backtest;
using AlgoTrader.Core.Model.Exception;
using AlgoTrader.Core.Model.Attributes;
using AlgoTrader.Core.Model.Order.Backtest;

namespace AlgoTrader.Exchanges.Backtest
{
    public class BacktestPriceTickExchange : BacktestExchangeBase, IBacktestExchange
    {
        private readonly TimeFrameEnum _timeFrame;
        private readonly BacktestPriceTickExchangeCandleCloseFeed _candleCloseFeed;

        private IPriceTick _currentData; // current data of the feed
        private DateTime? _nextOpen;

        public BacktestPriceTickExchange(ICurrencyPair currencyPair, TimeFrameEnum timeFrame, double makerFee, double takerFee, BacktestOptions options) : base(currencyPair, makerFee, takerFee, options)
        {
            _timeFrame = timeFrame;
            _candleCloseFeed = new BacktestPriceTickExchangeCandleCloseFeed();

            _candle = GetEmptyCandleTuple();
            _balance = GetEmptyCandleTuple();
        }

        protected override double GetCurrentBalanceInQuoteCurrency() => GetQuoteTotalBalance() + GetBaseTotalBalance() * _currentData.Price;

        #region Feed data handlers

        private (double? O, double H, double L, double C, double V) _candle;
        private (double? O, double H, double L, double C, double V) _balance;
        /// <summary>
        /// Event handler for new tick data - checks for stop losses - gets called on each tick, emit candle close event manually
        /// </summary>
        public void OnPriceTickData(object sender, IPriceTick data)
        {
            _currentData = data;

            // stop losses
            foreach (var stop in _openOrders.Where(o => o.Type.IsStop()).ToList())
            {
                if (stop.Type == OrderType.StopMarket)
                {
                    // slippage & fees
                    var stopPrice = stop.Price;
                    if (stop.Side == OrderSide.Buy && data.Price > stop.Price)
                    {
                        // buy at market
                        AddQuoteAvailableBalance(stop.Price * stop.Amount);
                        var (fee, executedPrice) = BuyMarket(stop.Amount, stopPrice);
                        AddTxCostsPaid(fee);
                        EmitOrderFilled(new BacktestOrderFill(stop.CurrencyPair, stop.Type, stop.Side, OrderStatus.Filled, executedPrice, stop.Amount, data.Timestamp, stop.Id));
                        _openOrders.Remove(stop);
                    }
                    else if (stop.Side == OrderSide.Sell && data.Price < stop.Price)
                    {
                        // sell at market
                        AddQuoteAvailableBalance(stop.Amount);
                        var (fee, executedPrice) = SellMarket(stop.Amount, stopPrice);
                        AddTxCostsPaid(fee);
                        EmitOrderFilled(new BacktestOrderFill(stop.CurrencyPair, stop.Type, stop.Side, OrderStatus.Filled, executedPrice, stop.Amount, data.Timestamp, stop.Id));
                        _openOrders.Remove(stop);
                    }
                }
                else
                    throw new NotImplementedException(string.Format("Handling stops of type {0} not supported.", stop.Type));
            }

            // emit candle close data if candle closed
            if (_nextOpen.HasValue == false)
                _nextOpen = _timeFrame.GetNextCandleOpen(_currentData.Timestamp);
            else if (_currentData.Timestamp >= _nextOpen)
            {
                // create candles
                var candleOpenTime = _nextOpen.Value.Subtract(_timeFrame.ToTimeSpan());
                var candleCloseTime = _nextOpen.Value.Subtract(TimeSpan.FromTicks(1)); // TODO: maybe use last tick time?
                var priceCandle = new BacktestResultCandlestick(candleOpenTime, candleCloseTime, _candle.O.Value, _candle.H, _candle.L, _candle.C, _candle.V);

                // add partial result to batch
                AddBacktestResultSegment(new BacktestResultSegment
                {
                    Data = priceCandle,
                    Balance = new BacktestResultCandlestick(candleOpenTime, candleCloseTime, _balance.O.Value, _balance.H, _balance.L, _balance.C, _balance.V),
                    Profit = new BacktestResultCandlestick(candleOpenTime, candleCloseTime, _balance.O.Value - _options.StartingQuoteBalance, _balance.H - _options.StartingQuoteBalance, _balance.L - _options.StartingQuoteBalance, _balance.C - _options.StartingQuoteBalance, 0),
                    ProfitPercentage = new BacktestResultCandlestick(candleOpenTime, candleCloseTime, _balance.O.Value / _options.StartingQuoteBalance - 1, _balance.H / _options.StartingQuoteBalance - 1, _balance.L / _options.StartingQuoteBalance - 1, _balance.C / _options.StartingQuoteBalance - 1, 0)
                });

                // emit candle close data
                _candleCloseFeed.EmitCandleClose(priceCandle);

                _candle = GetEmptyCandleTuple();
                _balance = GetEmptyCandleTuple();
                AddBarHoldTime();
                _nextOpen = _timeFrame.GetNextCandleOpen(_currentData.Timestamp);
            }

            // price data candle
            var tickPrice = data.Price;
            if (_candle.O.HasValue == false)
                _candle.O = tickPrice;

            if (tickPrice > _candle.H)
                _candle.H = tickPrice;

            if (tickPrice < _candle.L)
                _candle.L = tickPrice;

            _candle.C = tickPrice;
            _candle.V += data.TradeSize * tickPrice;

            // portfolio balance candle
            var balance = GetCurrentBalanceInQuoteCurrency();
            if (_balance.O.HasValue == false)
                _balance.O = balance;

            if (balance > _balance.H)
                _balance.H = balance;

            if (balance < _balance.L)
                _balance.L = balance;

            _balance.C = balance;
            _balance.V += 0; // TODO
        }

        #endregion

        public IFeed<ICandlestick> GetCandleCloseFeed() => _candleCloseFeed;

        #region Implementation

        public async Task<IOrder> MarketOrder(ICurrencyPair currencyPair, double amount, OrderSide side)
        {
            // calculate slippage
            var marketPrice = _currentData.Price;
            // process order
            if (side == OrderSide.Buy)
            {
                // cancel stops
                foreach (var stop in _openOrders.Where(o => o.Type.IsStop() && o.Side == OrderSide.Buy).ToList())
                    await CancelOrder(stop.Id);

                // buy at market
                var (fee, executedPrice) = BuyMarket(amount, marketPrice);

                // logger, log fees
                if (_options.LogOrders)
                    logger.Trace(string.Format("BUY {0} shares at {1} (date: {2})", amount, executedPrice, _currentData.Timestamp.ToISOString()));
                AddTxCostsPaid(fee);

                // return order
                var fill = new BacktestOrderFill(_currencyPair, OrderType.Market, side, OrderStatus.Filled, executedPrice, amount, _currentData.Timestamp);
                EmitOrderPlaced(new BacktestOrderPlace(_currencyPair, OrderType.Market, side, OrderStatus.New, marketPrice, amount, _currentData.Timestamp, fill.Id));
                EmitOrderFilled(fill);
                return fill;
            }
            else if (side == OrderSide.Sell && amount >= AvailableBalances[_currencyPair.Base])
            {
                // cancel stops
                foreach (var stop in _openOrders.Where(o => o.Type.IsStop() && o.Side == OrderSide.Sell).ToList())
                    await CancelOrder(stop.Id);

                // sell at market
                var (fee, executedPrice) = SellMarket(amount, marketPrice);

                // logger, log fees
                if (_options.LogOrders)
                    logger.Trace(string.Format("SELL {0} shares at {1} (date: {2})", amount, executedPrice, _currentData.Timestamp.ToISOString()));
                AddTxCostsPaid(fee);

                // return order
                var fill = new BacktestOrderFill(_currencyPair, OrderType.Market, side, OrderStatus.Filled, executedPrice, amount, _currentData.Timestamp);
                EmitOrderPlaced(new BacktestOrderPlace(_currencyPair, OrderType.Market, side, OrderStatus.New, marketPrice, amount, _currentData.Timestamp, fill.Id));
                EmitOrderFilled(fill);
                return fill;
            }

            return null;
        }

        public Task<IOrder> StopMarketOrder(ICurrencyPair currencyPair, double amount, double stopPrice, OrderSide side)
        {
            // check if stopPrice under/above current price
            var currPrice = _currentData.Price;
            if ((side == OrderSide.Buy && stopPrice <= currPrice) || (side == OrderSide.Sell && stopPrice >= currPrice))
                throw new Exception(string.Format("Invalid stop price for {0} market stop: {1} - current price: {2}", side, stopPrice, currPrice));

            // check available balance
            var fee = _takerTxCostModel.Calculate(amount); // for buying - you have to pay fees alongside the price
            var buyTotalExpenses = stopPrice * (amount + fee);
            if ((side == OrderSide.Buy && buyTotalExpenses > AvailableBalances[_currencyPair.Quote]) || (side == OrderSide.Sell && amount > AvailableBalances[_currencyPair.Base]))
                throw new InsufficientBalanceException("Insufficient balance to place a stop loss.");

            // change available balance
            if (side == OrderSide.Buy)
                AddQuoteAvailableBalance(-buyTotalExpenses);
            else if (side == OrderSide.Sell)
                AddBaseAvailableBalance(-amount);

            // override previous stop - cancel previous
            var stop = new BacktestOrderPlace(currencyPair, OrderType.StopMarket, side, OrderStatus.New, stopPrice, amount, _currentData.Timestamp);
            _openOrders.Add(stop);
            EmitOrderPlaced(stop);
            return Task.FromResult<IOrder>(stop);
        }

        public override Task<bool> CancelOrder(string id)
        {
            var order = _openOrders.FirstOrDefault(o => o.Id.Equals(id));
            if (order == null)
                return Task.FromResult(false);

            if (order.Side == OrderSide.Buy)
            {
                AddQuoteAvailableBalance(order.Price * order.Amount);
                EmitOrderCancelled(new BacktestOrderCancel(order, _currentData.Timestamp));
                _openOrders.Remove(order);
            }
            else if (order.Side == OrderSide.Sell)
            {
                AddBaseAvailableBalance(order.Amount);
                EmitOrderCancelled(new BacktestOrderCancel(order, _currentData.Timestamp));
                _openOrders.Remove(order);
            }
            return Task.FromResult(true);
        }

        #endregion

        private (double?, double, double, double, double) GetEmptyCandleTuple() => (null, double.NegativeInfinity, double.PositiveInfinity, 0, 0);

        /// <summary>
        /// Provides candle close data from the backtest price tick exchange
        /// </summary>
        [BacktestFeed]
        public class BacktestPriceTickExchangeCandleCloseFeed : FeedBase<ICandlestick>, IFeed<ICandlestick>
        {
            private NotImplementedException _exception => new NotImplementedException("Do not use this feed directly.");

            public void EmitCandleClose(ICandlestick candlestick)
            {
                // add to history
                _history.Add(candlestick);
                while (_history.Count > HistoryMaxLength)
                    _history.RemoveAt(0);

                // emit
                EmitDataEvent(candlestick);
            }

            public Task Start() => throw _exception;
            public Task Stop() => throw _exception;
        }
    }
}
