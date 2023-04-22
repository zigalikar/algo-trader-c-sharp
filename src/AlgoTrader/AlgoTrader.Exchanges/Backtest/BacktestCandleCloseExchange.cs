using System;
using System.Linq;
using System.Threading.Tasks;

using AlgoTrader.Exchanges.Core;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Backtest;
using AlgoTrader.Core.Model.Exception;
using AlgoTrader.Core.Model.Order.Backtest;

namespace AlgoTrader.Exchanges.Backtest
{
    /// <summary>
    /// Exchange class used for backtesting
    /// This class only takes fees and other data that might affect backtesting
    /// </summary>
    public class BacktestCandleCloseExchange : BacktestExchangeBase, IBacktestExchange
    {        
        private ICandlestick _currentData; // current data of the feed

        public BacktestCandleCloseExchange(ICurrencyPair currencyPair, double makerFee, double takerFee, BacktestOptions options) : base(currencyPair, makerFee, takerFee, options) { }

        protected override double GetCurrentBalanceInQuoteCurrency() => GetQuoteTotalBalance() + GetBaseTotalBalance() * _currentData.ClosePrice;

        #region Feed data handlers

        /// <summary>
        /// Event handler for new data - checks for stop losses - separate handler so stops don't get triggered as soon as they are created (in order: this.BeforeOnData [check for stops set up on previous candle close], Algo.OnData [set up potential stops], this.OnData)
        /// </summary>
        public void BeforeOnData(object sender, ICandlestick data)
        {
            AddBarHoldTime();
            _currentData = data;

            // stop losses
            foreach (var stop in _openOrders.Where(o => o.Type.IsStop()).ToList())
            {
                if (stop.Type == OrderType.StopMarket)
                {
                    // slippage & fees
                    var stopPrice = stop.Price;
                    if (stop.Side == OrderSide.Buy && data.HighPrice > stop.Price)
                    {
                        // buy at market
                        AddQuoteAvailableBalance(stop.Price * stop.Amount);
                        var (fee, executedPrice) = BuyMarket(stop.Amount, stopPrice);
                        AddTxCostsPaid(fee);
                        EmitOrderFilled(new BacktestOrderFill(stop.CurrencyPair, stop.Type, stop.Side, OrderStatus.Filled, executedPrice, stop.Amount, data.CloseTime, stop.Id));
                        _openOrders.Remove(stop);
                    }
                    else if (stop.Side == OrderSide.Sell && data.LowPrice < stop.Price)
                    {
                        // sell at market
                        AddQuoteAvailableBalance(stop.Amount);
                        var (fee, executedPrice) = SellMarket(stop.Amount, stopPrice);
                        AddTxCostsPaid(fee);
                        EmitOrderFilled(new BacktestOrderFill(stop.CurrencyPair, stop.Type, stop.Side, OrderStatus.Filled, executedPrice, stop.Amount, data.CloseTime, stop.Id));
                        _openOrders.Remove(stop);
                    }
                }
                else
                    throw new NotImplementedException(string.Format("Handling stops of type {0} not supported.", stop.Type));
            }
        }

        /// <summary>
        /// Event handler for new data - batches backtesting results and emits event (THIS GETS CALLED AFTER OnData INSIDE THE ALGO!!!)
        /// </summary>
        public void OnData(object sender, ICandlestick data)
        {
            // NOTICE: use ClosePrice because OnData gets emitted on candle close
            // annual returns
            var balance = GetCurrentBalanceInQuoteCurrency();

            // add partial result to batch
            AddBacktestResultSegment(new BacktestResultSegment
            {
                Data = data,
                Balance = new BacktestResultCandlestick(data.OpenTime, data.CloseTime, balance, 0),
                Profit = new BacktestResultCandlestick(data.OpenTime, data.CloseTime, balance - _options.StartingQuoteBalance, 0),
                ProfitPercentage = new BacktestResultCandlestick(data.OpenTime, data.CloseTime, balance / _options.StartingQuoteBalance - 1, 0)
            });
        }

        #endregion

        #region Implementation

        public async Task<IOrder> MarketOrder(ICurrencyPair currencyPair, double amount, OrderSide side)
        {
            // NOTICE: use ClosePrice because OnData gets emitted on candle close
            // calculate slippage
            var marketPrice = _currentData.ClosePrice;
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
                    logger.Trace(string.Format("BUY {0} shares at {1} (date: {2})", amount, executedPrice, _currentData.CloseTime.ToString("dd-MM-yyyy H:mm:ss")));
                AddTxCostsPaid(fee);

                // return order
                var fill = new BacktestOrderFill(_currencyPair, OrderType.Market, side, OrderStatus.Filled, executedPrice, amount, _currentData.CloseTime);
                EmitOrderPlaced(new BacktestOrderPlace(_currencyPair, OrderType.Market, side, OrderStatus.New, marketPrice, amount, _currentData.CloseTime, fill.Id));
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
                    logger.Trace(string.Format("SELL {0} shares at {1} (date: {2})", amount, executedPrice, _currentData.CloseTime.ToString("dd-MM-yyyy H:mm:ss")));
                AddTxCostsPaid(fee);

                // return order
                var fill = new BacktestOrderFill(_currencyPair, OrderType.Market, side, OrderStatus.Filled, executedPrice, amount, _currentData.CloseTime);
                EmitOrderPlaced(new BacktestOrderPlace(_currencyPair, OrderType.Market, side, OrderStatus.New, marketPrice, amount, _currentData.CloseTime, fill.Id));
                EmitOrderFilled(fill);
                return fill;
            }

            return null;
        }
        
        public Task<IOrder> StopMarketOrder(ICurrencyPair currencyPair, double amount, double stopPrice, OrderSide side)
        {
            // NOTICE: use ClosePrice because OnData gets emitted on candle close
            // check if stopPrice under/above current price
            var currPrice = _currentData.ClosePrice;
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
            var stop = new BacktestOrderPlace(currencyPair, OrderType.StopMarket, side, OrderStatus.New, stopPrice, amount, _currentData.CloseTime);
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
                EmitOrderCancelled(new BacktestOrderCancel(order, _currentData.CloseTime));
                _openOrders.Remove(order);
            }
            else if (order.Side == OrderSide.Sell)
            {
                AddBaseAvailableBalance(order.Amount);
                EmitOrderCancelled(new BacktestOrderCancel(order, _currentData.CloseTime));
                _openOrders.Remove(order);
            }
            return Task.FromResult(true);
        }

        #endregion
    }
}
