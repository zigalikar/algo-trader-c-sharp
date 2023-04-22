using System;
using System.Threading.Tasks;

using AlgoTrader.Feeds.Helpers;

namespace AlgoTrader.Feeds.Core
{
    public abstract class CSVFeedBase<T> : FeedBase<T> where T : class
    {
        protected CSVDataReader<T> _reader;

        public CSVFeedBase(CSVDataReader<T> reader)
        {
            _reader = reader;
        }

        public Task Start()
        {
            logger.Trace("Start");
            foreach (var line in _reader.Read())
            {
                // add to history
                _history.Add(line);
                while (_history.Count > HistoryMaxLength)
                    _history.RemoveAt(0);

                // emit
                EmitDataEvent(line);
            }

            return Task.CompletedTask;
        }

        public Task Stop() => throw new NotImplementedException();
    }
}
