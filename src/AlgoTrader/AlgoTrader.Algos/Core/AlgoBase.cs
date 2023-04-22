using System;
using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Model.Algo;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Backtest;

namespace AlgoTrader.Algos.Core
{
    /// <summary>
    /// Base class for any algo
    /// </summary>
    public abstract class AlgoBase : IDisposable, IBacktestAlgo
    {
        protected static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IFeed<ICandlestick> Feed { get; }
        public IExchange Exchange { get; }
        public ICurrencyPair CurrencyPair { get; }
        public TimeFrameEnum TimeFrame { get; }

        private readonly AlgoOptions _options;
        
        protected abstract void OnData(ICandlestick data);
        public virtual IList<BacktestData> GetAdditionalBacktestData() => new List<BacktestData>();

        /// <summary>
        /// Initiates a new instance of an algorithm
        /// </summary>
        /// <param name="feed">The feed that will supply the algo with data</param>
        /// <param name="exchange">The exchange the algorithm will run on</param>
        /// <param name="currencyPair">The currency pair the algorithm will trade</param>
        /// <param name="timeFrame">The time frame the algorithm will trade on</param>
        /// <param name="options">Additional options of the algo</param>
        public AlgoBase(IFeed<ICandlestick> feed, IExchange exchange, ICurrencyPair currencyPair, TimeFrameEnum timeFrame, AlgoOptions options)
        {
            Feed = feed;
            Exchange = exchange;
            CurrencyPair = currencyPair;
            TimeFrame = timeFrame;

            _options = options ?? new AlgoOptions();

            Exchange.Initialize();
            Exchange.OrderPlaced += OnExchangeOrderPlaced;
            Exchange.OrderFilled += OnExchangeOrderFilled;
            Exchange.OrderCancelled += OnExchangeOrderCancelled;
            Feed.Subscribe(FeedEventHandler);

            logger.Trace("Constructor");
        }

        public void Dispose()
        {
            Exchange.OrderPlaced -= OnExchangeOrderPlaced;
            Exchange.OrderFilled -= OnExchangeOrderFilled;
            Exchange.OrderCancelled -= OnExchangeOrderCancelled;
            Feed.Unsubscribe(FeedEventHandler);
            Exchange.CancelAllOrders();
            logger.Trace("Disposed");
        }

        private async void FeedEventHandler(object sender, ICandlestick data)
        {
            OnData(data);

            // NOTICE: async? OnData should continue without waiting for the orders to get cancelled, problems with backtesting if the cancel orders are not awaited and ran in a new thread
            // TODO: stops should move on each orderbook tick instead of candle close
            // trail stop
            if (_options.TrailStops)
            {
                foreach (var pair in _stops.ToList())
                {
                    // NOTICE: using ClosePrice because OnData is called on candle close
                    var forOrder = pair.Key;
                    var stop = pair.Value;

                    var currPrice = data.ClosePrice;
                    if (stop.Price > forOrder.Price * 0.99) // if already put into profit - 0.99 is threshold
                    {
                        var percentage = currPrice / stop.Price;
                        if (percentage >= (1 + _options.TrailAbovePreviousStopTriggerPercentage))
                        {
                            // trail stops into profit
                            await Exchange.CancelOrder(stop.Id);
                            var newStop = await Exchange.StopMarketOrder(forOrder.CurrencyPair, forOrder.Amount, currPrice * (1 - _options.TrailUnderPricePercentage), OrderSide.Sell);
                            _stops[forOrder] = newStop;
                        }
                    }
                    else
                    {
                        var percentage = currPrice / forOrder.Price;
                        if (percentage >= (1 + _options.StopToBreakEvenPercentage))
                        {
                            // move stop to break-even
                            await Exchange.CancelOrder(stop.Id);
                            var newStop = await Exchange.StopMarketOrder(forOrder.CurrencyPair, forOrder.Amount, forOrder.Price, OrderSide.Sell);
                            _stops[forOrder] = newStop;
                        }
                    }
                }
            }
        }

        private void OnExchangeOrderPlaced(object sender, IOrder e)
        {
        }

        private readonly IDictionary<IOrder, IOrder> _stops = new Dictionary<IOrder, IOrder>(); // dictionary of stops for each order
        private async void OnExchangeOrderFilled(object sender, IOrder e)
        {
            var type = e.Type;
            // automatic stops
            if (_options.UseStops)
            {
                // TODO: support for other order types
                if (type.IsStop())
                    RemoveStopFromDictionary(e.Id);
                else
                {
                    if (type == OrderType.Market)
                    {
                        if (e.Side == OrderSide.Buy)
                        {
                            // place sell stop for buy order
                            var stop = await Exchange.StopMarketOrder(e.CurrencyPair, e.Amount, e.Price * (1 - _options.StopPercentage), OrderSide.Sell);
                            _stops[e] = stop;
                        }
                        else
                        {
                            // TODO: support sell stops on leverage trading
                        }
                    }
                    else
                        throw new NotImplementedException(string.Format("Order type {0} not implemented for automatic stops.", type));
                }
            }
        }

        private void OnExchangeOrderCancelled(object sender, IOrder e)
        {
            if (e.Type.IsStop())
                RemoveStopFromDictionary(e.Id); // try to remove order from the dictionary if it was a stop
            else
            {
                // check if there is a stop for this order in the dictionary and remove it
                var found = _stops.ToList().Find(s => s.Key.Id == e.Id).Value;
                if (found != null)
                    Exchange.CancelOrder(found.Id);
            }
        }

        private void RemoveStopFromDictionary(string id)
        {
            var found = _stops.FirstOrDefault(x => x.Value.Id.Equals(id)).Key;
            if (found != null)
                _stops.Remove(found);
        }
    }
}
