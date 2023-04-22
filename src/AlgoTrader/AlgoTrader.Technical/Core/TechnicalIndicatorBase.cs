using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Attributes;

namespace AlgoTrader.Technical.Core
{
    public abstract class TechnicalIndicatorBase
    {
        protected static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected IFeed<ICandlestick> Feed { get; private set; }

        protected bool IsBacktest { get; private set; }
        private readonly IList<object> _backtestHistory = new List<object>();

        public TechnicalIndicatorBase(IFeed<ICandlestick> feed)
        {
            Feed = feed;
            IsBacktest = feed.GetType().GetCustomAttribute<BacktestFeed>() != null;

            logger.Trace("Constructor");
        }

        protected void SaveBacktestHistory(object data) => _backtestHistory.Add(data);

        public IList<T> GetBacktestHistory<T>()
        {
            if (IsBacktest)
                return _backtestHistory.Select(x => (T) x).ToList();
            throw new Exception("The supplied feed is not a backtest feed.");
        }
    }
}
