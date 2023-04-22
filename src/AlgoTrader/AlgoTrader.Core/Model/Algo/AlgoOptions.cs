namespace AlgoTrader.Core.Model.Algo
{
    /// <summary>
    /// Class for supplying additinal options to algos
    /// </summary>
    public class AlgoOptions
    {
        /// <summary>
        /// Whether the algo should use automatic stops
        /// </summary>
        public bool UseStops { get; set; } = false;

        /// <summary>
        /// Whether the algo should trail stops automatically
        /// </summary>
        public bool TrailStops { get; set; } = false;

        /// <summary>
        /// Percentage of the stop loss in the opposite direction of the trade
        /// </summary>
        public double StopPercentage { get; set; } = 0.03d;

        /// <summary>
        /// Percentage above entry when the trailing stop should be activated and moved to break-even
        /// </summary>
        public double StopToBreakEvenPercentage { get; set; } = 0.05d;

        /// <summary>
        /// Percentage above previous stop loss when the stop should be moved
        /// </summary>
        public double TrailAbovePreviousStopTriggerPercentage { get; set; } = 0.1d;

        /// <summary>
        /// Percentage under current price where the trailing stop should be moved once the trailing stop has been triggered
        /// </summary>
        public double TrailUnderPricePercentage { get; set; } = 0.05d;
    }
}
