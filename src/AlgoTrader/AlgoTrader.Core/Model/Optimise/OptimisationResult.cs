using System.Collections.Generic;

using AlgoTrader.Core.Model.Backtest;

namespace AlgoTrader.Core.Model.Optimise
{
    public class OptimisationResult
    {
        /// <summary>
        /// Evaluated results sorted by the most optimal to the least optimal
        /// </summary>
        public IList<BacktestResultTrainingTestPair> Results { get; set; }
    }
}
