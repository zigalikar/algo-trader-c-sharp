namespace AlgoTrader.Core.Model.Evaluate
{
    public class AnnualReturn
    {
        /// <summary>
        /// Year of the annual return
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Percentage profit of the year (PercentageProfit == 1 -> 100% profit)
        /// </summary>
        public double PercentageProfit { get; set; }

        /// <summary>
        /// Whether the backtesting data included the whole year
        /// </summary>
        public bool IsComplete { get; set; }
    }
}
