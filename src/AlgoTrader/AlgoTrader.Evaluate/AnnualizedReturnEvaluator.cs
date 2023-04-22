using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Evaluate.Core;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Backtest;

namespace AlgoTrader.Evaluate
{
    /// <summary>
    /// Sorts by average annualized return
    /// </summary>
    public class AnnualizedReturnEvaluator : EvaluatorBase, IEvaluator
    {
        public override IList<BacktestResultTrainingTestPair> Evaluate(IList<BacktestResultTrainingTestPair> results)
        {
            var res = results.ToList();
            res.Sort((a, b) => a.Training.AnnualReturns.Average(ret => ret.PercentageProfit) > b.Training.AnnualReturns.Average(ret => ret.PercentageProfit) ? -1 : 1);
            return res.ToList();
        }
    }
}
