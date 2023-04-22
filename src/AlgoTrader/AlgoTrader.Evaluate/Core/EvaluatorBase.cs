using System.Collections.Generic;

using AlgoTrader.Core.Model.Backtest;

namespace AlgoTrader.Evaluate.Core
{
    public abstract class EvaluatorBase
    {
        protected static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public abstract IList<BacktestResultTrainingTestPair> Evaluate(IList<BacktestResultTrainingTestPair> results);
        public virtual IList<BacktestData> GetAdditionalBacktestData() => new List<BacktestData>();

        public EvaluatorBase()
        {
            logger.Trace("Constructor");
        }
    }
}
