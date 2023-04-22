using System;
using System.Linq;
using System.Collections.Generic;

namespace AlgoTrader.Feeds.Core
{
    /// <summary>
    /// Base class for feeds
    /// </summary>
    public abstract class FeedBase<T> where T : class
    {
        protected static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private event EventHandler<T> _data = delegate { };

        protected readonly List<T> _history = new List<T>();
        protected int HistoryMaxLength => 100;

        public void Subscribe(EventHandler<T> callback) => _data += callback;
        public void Unsubscribe(EventHandler<T> callback) => _data -= callback;

        protected void EmitDataEvent(T data) => _data.Invoke(this, data);

        public FeedBase()
        {
            logger.Trace("Constructor");
        }
        
        public IEnumerable<T> GetHistoryData(int length)
        {
            if (_history.Count >= length)
            {
                var data = _history.ToList();
                data.Reverse();
                return data.Take(length);
            }
            return null;
        }
    }
}
