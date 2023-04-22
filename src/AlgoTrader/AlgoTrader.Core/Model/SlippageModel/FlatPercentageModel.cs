using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;

namespace AlgoTrader.Core.Model.SlippageModel
{
    /// <summary>
    /// Assumes a flat percentage slippage on every trade. In general, the simulated slippage shouldn't be more than 2%.
    /// </summary>
    public class FlatPercentageModel : ISlippageModel
    {
        public double Percentage { get; private set; }

        public FlatPercentageModel(double percentage)
        {
            Percentage = percentage;
        }

        public double Calculate(double price, OrderSide side)
        {
            if (side == OrderSide.Buy)
                return price + price * Percentage;
            return price - price * Percentage;
        }
    }
}
