using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Backtest;
using AlgoTrader.Core.Model.Evaluate;
using AlgoTrader.Core.Model.Exception;
using AlgoTrader.Core.Model.SlippageModel;
using AlgoTrader.Core.Model.TransactionCostModel;

namespace AlgoTrader.Exchanges.Core
{
    /// <summary>
    /// Base class for all backtest exchanges
    /// </summary>
    public abstract class BacktestExchangeBase : ExchangeBase
    {
        protected readonly ICurrencyPair _currencyPair;
        protected readonly BacktestOptions _options;

        protected readonly ITransactionCostModel _makerTxCostModel;
        protected readonly ITransactionCostModel _takerTxCostModel;
        protected readonly ISlippageModel _slippage;

        private double _baseTotalBalance;
        private double _quoteTotalBalance;

        private double _balanceAtLastBuy = -1; // balance at last buy order filled

        private IList<BacktestResultSegment> _segments = new List<BacktestResultSegment>(); // processed backtesting data

        /// <summary>
        /// Gets current total balance nominated in quote currency
        /// </summary>
        /// <returns>Total balance</returns>
        protected abstract double GetCurrentBalanceInQuoteCurrency();

        /// <summary>
        /// IExchange interface implementation
        /// </summary>
        public abstract Task<bool> CancelOrder(string id);

        private IList<double> _losses = new List<double>(); // all losses
        private IList<double> _wins = new List<double>(); // all wins

        private IList<int> _barsHeld = new List<int>(); // bars held for all trades
        private int _holding = 0;

        private int _maxConsecutiveWins;
        private int _consecutiveWins;
        private int _maxConsecutiveLosses;
        private int _consecutiveLosses;

        private double _txCostsPaid = 0; // total fees paid

        private IList<IOrder> _ordersPlaced = new List<IOrder>();
        private IList<IOrder> _ordersFilled = new List<IOrder>();
        private IList<IOrder> _ordersCancelled = new List<IOrder>();

        public BacktestExchangeBase(ICurrencyPair currencyPair, double makerFee, double takerFee, BacktestOptions options)
        {
            _currencyPair = currencyPair;
            _options = options;

            _makerTxCostModel = options.MakerTransactionCostsModel ?? new PercentageModel(makerFee);
            _takerTxCostModel = options.TakerTransactionCostsModel ?? new PercentageModel(takerFee);
            _slippage = options.SlippageModel ?? new FlatPercentageModel(0.0005); // default is 0.05%

            _baseTotalBalance = 0;
            _quoteTotalBalance = options.StartingQuoteBalance;

            _availableBalances[_currencyPair.Base] = 0;
            _availableBalances[_currencyPair.Quote] = _quoteTotalBalance;

            OrderPlaced += (s, e) => _ordersPlaced.Add(e);
            OrderFilled += (s, e) =>
            {
                _ordersFilled.Add(e);

                // NOTICE: use ClosePrice because OnData gets emitted on candle close
                if (e.Side == OrderSide.Buy && _balanceAtLastBuy < 0)
                {
                    _balanceAtLastBuy = GetCurrentBalanceInQuoteCurrency();
                    _holding = 0;
                }
                else if (e.Side == OrderSide.Sell && _balanceAtLastBuy > 0)
                {
                    // calculate win/loss
                    var newBalance = GetCurrentBalanceInQuoteCurrency();
                    var diff = newBalance - _balanceAtLastBuy;
                    if (diff < 0) // loss
                    {
                        // add to losses
                        _losses.Add(diff);

                        // log consecutive loss
                        _consecutiveWins = 0;
                        _consecutiveLosses++;
                        if (_consecutiveLosses > _maxConsecutiveLosses)
                            _maxConsecutiveLosses = _consecutiveLosses;
                    }
                    else if (diff > 0) // win
                    {
                        // add to wins
                        _wins.Add(diff);

                        // log consecutive win
                        _consecutiveLosses = 0;
                        _consecutiveWins++;
                        if (_consecutiveWins > _maxConsecutiveWins)
                            _maxConsecutiveWins = _consecutiveWins;
                    }
                    _balanceAtLastBuy = -1;

                    // bars held
                    _barsHeld.Add(_holding);
                }
            };

            OrderCancelled += (s, e) => _ordersCancelled.Add(e);
        }

        protected override Task GetStartingAvailableBalances() => Task.CompletedTask;
        protected override Task GetStartingOpenOrders() => Task.CompletedTask;
        protected override Task SetupUserDataFeed(Action<IWebSocketAccountUpdate> onAccountUpdate, Action<IOrder> onOrderUpdate) => Task.CompletedTask;
        public ICurrencyPair GetCurrencyPair(string value) => throw new NotImplementedException();
        public Task<IFeed<ICandlestick>> GetCandlestickFeed(ICurrencyPair currencyPair, TimeFrameEnum timeFrame) => throw new NotImplementedException();
        public Task<IFeed<Trade>> GetTradesFeed(ICurrencyPair currencyPair) => throw new NotImplementedException();
        public Task<IFeed<Orderbook>> GetOrderbookFeed(ICurrencyPair currencyPair) => throw new NotImplementedException();
        public Task<IList<ICandlestick>> GetPriceData(ICurrencyPair currencyPair, TimeFrameEnum timeFrame, int length) => throw new NotImplementedException();
        public Task<Orderbook> GetOrderbook(ICurrencyPair currencyPair, CancellationTokenSource cts = null) => throw new NotImplementedException();

        public BacktestResult GetResult(IBacktestAlgo algo)
        {
            var annualReturns = new List<AnnualReturn>();
            DateTime? currYear = null;
            double currYearStartingBalance = 0;
            BacktestResultSegment lastSegment = null;

            var highWatermark = double.NegativeInfinity;
            var maxDrawdown = 0d;
            var maxDrawdownPercentage = 0d;

            var currentDrawdownDuration = 0;
            var maxDrawdownDuration = 0;

            // loop through all segments
            for (var i = 0; i < _segments.Count; i++)
            {
                var segment = _segments[i];

                // annual returns
                if (currYear.HasValue == false || currYear.Value.Year != segment.Data.CloseTime.Year || i == _segments.Count - 1)
                {
                    if (currYear.HasValue)
                    {
                        // new year found
                        annualReturns.Add(new AnnualReturn
                        {
                            Year = currYear.Value.Year,
                            PercentageProfit = lastSegment.Balance.ClosePrice / currYearStartingBalance - 1,
                            IsComplete = currYear.Value.DayOfYear == 1 && lastSegment.Data.CloseTime.Month == 12 && lastSegment.Data.CloseTime.Day == 31 // TODO: problems with intraday timeframes (data can start on i.e. the 23th hour of the day and still count as full year)
                        });
                    }

                    // first year
                    currYear = segment.Data.CloseTime;
                    currYearStartingBalance = segment.Balance.ClosePrice;
                }

                // drawdown
                if (segment.Balance.HighPrice >= highWatermark)
                {
                    // back above highs - reset drawdown
                    highWatermark = segment.Balance.HighPrice;
                    currentDrawdownDuration = 0;
                }
                else
                {
                    // calculate drawdown at lowest price
                    var currProfit = segment.Balance.LowPrice;

                    // calculate current absolute drawdown
                    var drawdown = highWatermark - currProfit;
                    if (drawdown > maxDrawdown)
                        maxDrawdown = drawdown;
                    
                    // calculate current drawdown percentage
                    var drawdownPercentage = 1 - currProfit / (highWatermark != 0 ? highWatermark : currProfit);
                    if (drawdownPercentage > maxDrawdownPercentage)
                        maxDrawdownPercentage = drawdownPercentage;

                    // calculate current drawdown duration
                    currentDrawdownDuration++;
                    if (currentDrawdownDuration > maxDrawdownDuration)
                        maxDrawdownDuration = currentDrawdownDuration;
                }

                lastSegment = segment;
            }

            // return results
            return new BacktestResult(algo)
            {
                Segments = _segments.ToList(),
                TransactionCosts = _txCostsPaid,
                OrdersPlaced = _ordersPlaced.ToList(),
                OrdersFilled = _ordersFilled.ToList(),
                OrdersCancelled = _ordersCancelled.ToList(),
                Options = _options,
                AnnualReturns = annualReturns,
                Winners = _wins.Count,
                Losers = _losses.Count,
                AverageWin = _wins.Any() ? _wins.Sum() / _wins.Count : 0,
                AverageLoss = _losses.Any() ? _losses.Sum() / _losses.Count : 0,
                AverageBarsHeld = _barsHeld.Any() ? _barsHeld.Average() : 0,
                MaxConsecutiveWins = _maxConsecutiveWins,
                MaxConsecutiveLosses = _maxConsecutiveLosses,
                MaxSystemDrawdown = maxDrawdown,
                MaxSystemDrawdownPercentage = maxDrawdownPercentage,
                MaxSystemDrawdownDuration = maxDrawdownDuration
            };
        }

        public async Task CancelAllOrders()
        {
            foreach (var order in _openOrders.ToList())
                await CancelOrder(order.Id);
        }

        #region Protected

        protected double GetBaseTotalBalance() => _baseTotalBalance;

        protected void AddBaseTotalBalance(double value) => _baseTotalBalance += value;

        protected void AddBaseAvailableBalance(double value) => _availableBalances[_currencyPair.Base] += value;

        protected double GetQuoteTotalBalance() => _quoteTotalBalance;

        protected void AddQuoteTotalBalance(double value) => _quoteTotalBalance += value;

        protected void AddQuoteAvailableBalance(double value) => _availableBalances[_currencyPair.Quote] += value;

        protected void AddTxCostsPaid(double value) => _txCostsPaid += value;

        protected void AddBacktestResultSegment(BacktestResultSegment segment) => _segments.Add(segment);

        protected void AddBarHoldTime() => _holding++;

        /// <summary>
        /// Helper function that buys asset at market
        /// </summary>
        /// <param name="amount">Amount to buy</param>
        /// <param name="price">Intended market price to buy for</param>
        /// <returns>Fee and executed price (accounts for slippage - returns average between max and min slippage)</returns>
        protected (double fee, double executedPrice) BuyMarket(double amount, double price)
        {
            // slippage
            var executedPrice = _slippage.Calculate(price, OrderSide.Buy);

            // calculate fees in quote currency
            var cost = amount * executedPrice;
            var fee = _takerTxCostModel.Calculate(cost);
            if (fee + cost > AvailableBalances[_currencyPair.Quote])
                throw new InsufficientBalanceException("Quote, fees and slippage would exceed available balance.");

            AddQuoteTotalBalance(-(cost + fee));
            AddQuoteAvailableBalance(-(cost + fee));
            AddBaseTotalBalance(amount);
            AddBaseAvailableBalance(amount);
            return (fee, executedPrice);
        }

        /// <summary>
        /// Helper function that sells asset at market
        /// </summary>
        /// <param name="amount">Amount to sell</param>
        /// <param name="price">Intended market price to sell for</param>
        /// <returns>Fee and executed price (accounts for slippage - returns average between max and min slippage)</returns>
        protected (double fee, double executedPrice) SellMarket(double amount, double price)
        {
            // slippage
            var executedPrice = _slippage.Calculate(price, OrderSide.Sell);

            if (amount > AvailableBalances[_currencyPair.Base])
                throw new InsufficientBalanceException("Selling would exceed available balance.");

            // calculate fees in base currency
            var fee = _takerTxCostModel.Calculate(amount);
            AddQuoteTotalBalance((amount - fee) * executedPrice);
            AddQuoteAvailableBalance((amount - fee) * executedPrice);
            AddBaseTotalBalance(-amount);
            AddBaseAvailableBalance(-amount);

            return (fee * executedPrice, executedPrice); // TODO: executedAmount
        }

        #endregion
    }
}
