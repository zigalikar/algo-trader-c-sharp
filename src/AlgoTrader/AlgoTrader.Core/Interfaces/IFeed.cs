using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AlgoTrader.Core.Interfaces
{
    /// <summary>
    /// Interface for all feeds
    /// </summary>
    public interface IFeed<T> : IFeed where T : class
    {
        void Subscribe(EventHandler<T> callback);
        void Unsubscribe(EventHandler<T> callback);

        /// <summary>
        /// Returns the feed's data history (newest data at index 0, older towards the end) or null if the history for that length does not exist
        /// </summary>
        /// <param name="length">Length of the data</param>
        /// <returns>History data</returns>
        IEnumerable<T> GetHistoryData(int length);
    }

    public interface IFeed
    {
        Task Start();
        Task Stop();
    }
}
