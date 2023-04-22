using System.Collections.Generic;

using AlgoTrader.Core.Model.Backtest;

namespace AlgoTrader.Core.Interfaces
{
    public interface IEvaluator : IAdditionalBacktestData
    {
        /// <summary>
        /// Evaluates the results and sorts the supplied list according to the implemented evaluation technique (first item in list is the best result)
        /// </summary>
        /// <param name="results">Results to evaluate</param>
        /// <returns>Sorted list</returns>
        IList<BacktestResultTrainingTestPair> Evaluate(IList<BacktestResultTrainingTestPair> results);
    }
}
