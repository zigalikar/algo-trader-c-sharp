using AlgoTrader.Core.Model.Backtest;

namespace AlgoTrader.Core.Model.Optimise
{
    public class OptimisationOptions
    {
        /// <summary>
        /// Options for the backtesters in the optimiser
        /// </summary>
        public BacktestOptions BacktestOptions { get; set; } = new BacktestOptions();

        /// <summary>
        /// Number of active workers for optimisation
        /// </summary>
        public int WorkerCount { get; set; } = 8;
    }
}
