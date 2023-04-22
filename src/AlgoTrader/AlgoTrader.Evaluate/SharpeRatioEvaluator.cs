using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Evaluate.Core;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.Model.Backtest;

namespace AlgoTrader.Evaluate
{
    /// <summary>
    /// Sort by best Sharpe ratio
    /// </summary>
    public class SharpeRatioEvaluator : EvaluatorBase, IEvaluator
    {
        private readonly double _benchmark;

        private IList<double> _trainingRatios = new List<double>();
        private IList<double> _testRatios = new List<double>();

        public SharpeRatioEvaluator(double benchmark)
        {
            _benchmark = benchmark;
        }

        public override IList<BacktestResultTrainingTestPair> Evaluate(IList<BacktestResultTrainingTestPair> results)
        {
            var res = results.Select(r =>
            {
                return new
                {
                    TrainingSharpeRatio = GetRatio(r.Training),
                    TestSharpeRatio = GetRatio(r.Test),
                    Data = r
                };
            }).ToList();

            res.Sort((a, b) => a.TrainingSharpeRatio > b.TrainingSharpeRatio ? -1 : 1);
            _trainingRatios = res.Select(r => r.TrainingSharpeRatio).ToList();
            _testRatios = res.Select(r => r.TestSharpeRatio).ToList();

            return res.Select(r => r.Data).ToList();
        }

        public override IList<BacktestData> GetAdditionalBacktestData() => new List<BacktestData>
        {
            new SharpeRatioBacktestData("Sharpe ratios")
            {
                TrainingSharpeRatios = _trainingRatios.ToList(),
                TestSharpeRatios = _trainingRatios.ToList()
            }
        };

        private double GetRatio(BacktestResult result)
        {
            var returns = result.AnnualReturns.Select(r => r.PercentageProfit);
            var excessReturns = returns.Select(r => r - _benchmark).ToList();
            var excessReturnsAvg = excessReturns.Average();
            var excessReturnsStdev = excessReturns.Stdev();

            return excessReturnsAvg / excessReturnsStdev;
        }
    }

    public class SharpeRatioBacktestData : BacktestData
    {
        public IList<double> TrainingSharpeRatios { get; set; }
        public IList<double> TestSharpeRatios { get; set; }

        public SharpeRatioBacktestData(string name) : base(name) { }
    }
}
