using System.Collections.Generic;

using AlgoTrader.Core.Model.Backtest;

namespace AlgoTrader.Core.Interfaces
{
    public interface IAdditionalBacktestData
    {
        /// <summary>
        /// Retrieves additional backtest data.
        /// </summary>
        /// <returns>Backtesting data</returns>
        IList<BacktestData> GetAdditionalBacktestData();
    }
}
