using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Algo;
using AlgoTrader.Core.Model.Evaluate;

namespace AlgoTrader.Core.Model.Backtest
{
    public class BacktestResultSegment
    {
        /// <summary>
        /// Price data of the segment
        /// </summary>
        public ICandlestick Data { get; set; }

        /// <summary>
        /// Account balance of the segment
        /// </summary>
        public ICandlestick Balance { get; set; }

        /// <summary>
        /// Profit of the segment
        /// </summary>
        public ICandlestick Profit { get; set; }

        /// <summary>
        /// Profit percentage of the segment
        /// </summary>
        public ICandlestick ProfitPercentage { get; set; }
    }

    public class BacktestResult
    {
        /// <summary>
        /// Segments of the backtesting
        /// </summary>
        public IList<BacktestResultSegment> Segments { get; set; }

        /// <summary>
        /// Profit at the end of backtesting
        /// </summary>
        public double Profit => Segments.Last().Profit.ClosePrice;

        /// <summary>
        /// Profit percentage at the end of backtesting
        /// </summary>
        public double ProfitPercentage => Segments.Last().ProfitPercentage.ClosePrice;

        /// <summary>
        /// Total transaction costs (quote currency)
        /// </summary>
        public double TransactionCosts { get; set; }

        /// <summary>
        /// Backtesting orders placed
        /// </summary>
        public IList<IOrder> OrdersPlaced { get; set; } = new List<IOrder>();

        /// <summary>
        /// Backtesting orders filled
        /// </summary>
        public IList<IOrder> OrdersFilled { get; set; } = new List<IOrder>();

        /// <summary>
        /// Backtesting orders cancelled
        /// </summary>
        public IList<IOrder> OrdersCancelled { get; set; } = new List<IOrder>();

        /// <summary>
        /// The options this backtest was ran on
        /// </summary>
        public BacktestOptions Options { get; set; }

        /// <summary>
        /// Algo options of the backtest
        /// </summary>
        public AlgoOptions AlgoOptions { get; set; }

        /// <summary>
        /// Annual returns of the backtest
        /// </summary>
        public IEnumerable<AnnualReturn> AnnualReturns { get; set; }

        /// <summary>
        /// Number of all trades
        /// </summary>
        public int TotalTrades => Winners + Losers;

        /// <summary>
        /// Total number of winning traders
        /// </summary>
        public int Winners { get; set; }

        /// <summary>
        /// Total number of losing trades
        /// </summary>
        public int Losers { get; set; }

        /// <summary>
        /// Ratio of total profit/wins (sum of all wins) and number of winners
        /// </summary>
        public double AverageWin { get; set; }

        /// <summary>
        /// Ratio of total loss (sum of all losses) and number of losers
        /// </summary>
        public double AverageLoss { get; set; }

        /// <summary>
        /// Average holding period per trade
        /// </summary>
        public double AverageBarsHeld { get; set; }

        /// <summary>
        /// Maximum number of consecutive wins
        /// </summary>
        public int MaxConsecutiveWins { get; set; }

        /// <summary>
        /// Maximum number of consecutive losses
        /// </summary>
        public int MaxConsecutiveLosses { get; set; }

        /// <summary>
        /// Maximum drawdown percentage of the backtest
        /// </summary>
        public double MaxSystemDrawdown { get; set; }

        /// <summary>
        /// Maximum drawdown percentage of the backtest
        /// </summary>
        public double MaxSystemDrawdownPercentage { get; set; }

        /// <summary>
        /// Maximum drawdown duration of the backtest (amount of bars)
        /// </summary>
        public double MaxSystemDrawdownDuration { get; set; }

        /// <summary>
        /// Ratio of net profit and maximum system drawdown
        /// </summary>
        public double RecoveryFactor => MaxSystemDrawdown > 0 ? (Profit / MaxSystemDrawdown) : double.PositiveInfinity;

        private readonly IBacktestAlgo _algo;

        public BacktestResult(IBacktestAlgo algo)
        {
            _algo = algo;
        }

        public IList<BacktestData> GetAdditionalBacktestData() => _algo.GetAdditionalBacktestData();
    }

    public class BacktestResultTrainingTestPair
    {
        public BacktestResult Training { get; }
        public BacktestResult Test { get; }

        public BacktestResultTrainingTestPair(BacktestResult training, BacktestResult test)
        {
            Training = training;
            Test = test;
        }
    }
}
