namespace AlgoTrader.Core.Interfaces
{
    /// <summary>
    /// Interface for all transaction cost models
    /// </summary>
    public interface ITransactionCostModel
    {
        /// <summary>
        /// Calculates the fees for the specified amount
        /// </summary>
        /// <param name="amount">The amount of the order</param>
        /// <returns>Fee in backtesting</returns>
        double Calculate(double amount);
    }
}
