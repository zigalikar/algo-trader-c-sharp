using AlgoTrader.Core.Model.Order;

namespace AlgoTrader.Core.Interfaces
{
    /// <summary>
    /// Interface for all slippage models
    /// </summary>
    public interface ISlippageModel
    {
        /// <summary>
        /// Calculates the backtest executed price
        /// </summary>
        /// <param name="price">The price of the order</param>
        /// <param name="side">The side of the order</param>
        /// <returns>Executed price in backtesting</returns>
        double Calculate(double price, OrderSide side);
    }
}
