using System;
using System.Linq;
using System.Threading.Tasks;

using AlgoTrader.Core.Model;
using AlgoTrader.Feeds.Core;
using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Feeds
{
    /// <summary>
    /// Feed that connects with an exchange
    /// </summary>
    public class ExchangeCandleCloseFeed<E> : ExchangeCandleCloseFeed where E : IExchange
    {
        /// <summary>
        /// Initiates a new instance of an exchange feed
        /// </summary>
        public ExchangeCandleCloseFeed(ICurrencyPair currencyPair, TimeFrameEnum timeFrame, bool constructCandlesFromTicks = true, ExchangeEnvironment environment = ExchangeEnvironment.Production) : base(currencyPair, timeFrame, typeof(E), constructCandlesFromTicks, environment) { }
    }

    public class ExchangeCandleCloseFeed : FeedBase<ICandlestick>, IFeed<ICandlestick>
    {
        private readonly ICurrencyPair _currencyPair;
        private readonly TimeFrameEnum _timeFrame;
        private readonly IExchange _exchange;
        private readonly bool _constructCandlesFromTicks;
        
        private IFeed<Trade> _tradesFeed;
        private IFeed<ICandlestick> _closeFeed;

        public ExchangeCandleCloseFeed(ICurrencyPair currencyPair, TimeFrameEnum timeFrame, Type exchangeType, bool constructCandlesFromTicks = true, ExchangeEnvironment environment = ExchangeEnvironment.Production)
        {
            _currencyPair = currencyPair;
            _timeFrame = timeFrame;
            _exchange = Activator.CreateInstance(exchangeType, environment) as IExchange;
            _constructCandlesFromTicks = constructCandlesFromTicks;
        }

        private DateTime nextOpen;
        public async Task Start()
        {
            logger.Trace("Start");

            // initialize exchange
            await _exchange.Initialize();

            _closeFeed = await _exchange.GetCandlestickFeed(_currencyPair, _timeFrame);
            if (_closeFeed != null)
            {
                _closeFeed.Subscribe(OnCandleCloseFeed);
                await _closeFeed.Start();
            }
            else
            {
                _tradesFeed = await _exchange.GetTradesFeed(_currencyPair);
                await _tradesFeed.Start();

                // subscribe to trade feed
                _tradesFeed.Subscribe(OnTradeFeed);
            }
        }

        public async Task Stop()
        {
            logger.Trace("Stop");
            
            if (_closeFeed != null)
            {
                _closeFeed.Unsubscribe(OnCandleCloseFeed);
                await _closeFeed.Stop();
            }
            else
            {
                _tradesFeed.Unsubscribe(OnTradeFeed);
                await _tradesFeed.Stop();
            }
        }

        private void OnCandleCloseFeed(object sender, ICandlestick cd) => EmitDataEvent(cd);

        private bool firstTradeData;
        private (DateTime? dt, double? O, double H, double L, double C, double V) _current = GetEmptyTrade();
        private static readonly object tradeFeedLock = new object();
        private void OnTradeFeed(object sender, Trade e)
        {
            // ŽL: some trade feeds do not provide data in real time but in smaller batches
            lock (tradeFeedLock)
            {
                if (!firstTradeData)
                {
                    // TODO: what if local & server time are not synced?
                    nextOpen = _timeFrame.GetNextCandleOpen();
                    firstTradeData = true;
                }

                if (_history.Count == 0)
                {
                    if (e.Timestamp > nextOpen)
                    {
                        // preload price data
                        var price = _exchange.GetPriceData(_currencyPair, _timeFrame, HistoryMaxLength).Result;
                        // TODO: orderflow data
                        _history.AddRange(price);

                        // set new next event timestamp
                        var newest = GetHistoryData(1).First();
                        nextOpen = newest.OpenTime.Add(TimeSpan.FromTicks(_timeFrame.ToTimeSpan().Ticks * 2));

                        // emit event
                        EmitDataEvent(newest);
                    }
                }
                else
                {
                    if (e.Timestamp >= nextOpen)
                    {
                        ICandlestick candle;
                        if (_constructCandlesFromTicks)
                        {
                            var openTime = nextOpen.Subtract(_timeFrame.ToTimeSpan());
                            var closeTime = nextOpen.Subtract(TimeSpan.FromTicks(1));

                            candle = new ExchangeCandleCloseFeedCandlestick(openTime, closeTime, _current.O.Value, _current.H, _current.L, _current.C, _current.V);
                            _history.Add(candle);
                        }
                        else
                        {
                            var newCandles = _exchange.GetPriceData(_currencyPair, _timeFrame, 1).Result;
                            candle = newCandles.First();
                            _history.Add(candle);
                        }

                        // set next open
                        var newest = GetHistoryData(1).First();
                        nextOpen = newest.OpenTime.Add(TimeSpan.FromTicks(_timeFrame.ToTimeSpan().Ticks * 2));
                        logger.Debug("NEXTOPEN: " + nextOpen.ToISOString());

                        // emit event
                        EmitDataEvent(candle);
                        _current = GetEmptyTrade();
                    }
                }

                _current.dt = e.Timestamp;
                if (_current.O.HasValue == false)
                    _current.O = e.Price;

                if (e.Price > _current.H)
                    _current.H = e.Price;

                if (e.Price < _current.L)
                    _current.L = e.Price;

                _current.C = e.Price;
                _current.V += e.TradeSize * e.Price;
            }
        }

        private static (DateTime?, double?, double, double, double, double) GetEmptyTrade() => (null, null, double.NegativeInfinity, double.PositiveInfinity, 0, 0);
    }
}
