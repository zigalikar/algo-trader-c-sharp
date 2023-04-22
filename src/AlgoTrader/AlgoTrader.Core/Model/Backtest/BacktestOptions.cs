using System;

using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Core.Model.Backtest
{
    public class BacktestOptions
    {
        /// <summary>
        /// Datetime from where to backtest in the dataset
        /// </summary>
        public DateTime? From { get; set; } = null;

        /// <summary>
        /// Datetime to where to backtest in the dataset
        /// </summary>
        public DateTime? To { get; set; } = null;

        /// <summary>
        /// The starting balance of the backtest
        /// </summary>
        public double StartingQuoteBalance { get; set; } = 10000;

        /// <summary>
        /// Whether to log SELL and BUY orders
        /// </summary>
        public bool LogOrders { get; set; } = true;

        /// <summary>
        /// Constructor parameters for the algo (excluding the parameters for the algo's base type)
        /// </summary>
        public object[] AlgoParams { get; set; }

        /// <summary>
        /// Model for estimating maker transaction costs.
        /// </summary>
        public ITransactionCostModel MakerTransactionCostsModel { get; set; }

        /// <summary>
        /// Model for estimating taker transaction costs
        /// </summary>
        public ITransactionCostModel TakerTransactionCostsModel { get; set; }

        /// <summary>
        /// Model for estimating slippage
        /// </summary>
        public ISlippageModel SlippageModel { get; set; }

        public BacktestOptions() { }

        public BacktestOptions Clone() => new BacktestOptions
        {
            From = From,
            To = To,
            StartingQuoteBalance = StartingQuoteBalance,
            LogOrders = LogOrders,
            AlgoParams = AlgoParams
        };
    }
}
