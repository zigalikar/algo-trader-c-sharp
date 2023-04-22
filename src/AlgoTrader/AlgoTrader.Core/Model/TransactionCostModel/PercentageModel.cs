using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Core.Model.TransactionCostModel
{
    /// <summary>
    /// Simple percentage transaction costs model
    /// </summary>
    public class PercentageModel : ITransactionCostModel
    {
        public double Percentage { get; private set; }

        public PercentageModel(double percentage)
        {
            Percentage = percentage;
        }

        public double Calculate(double amount) => amount * Percentage;
    }
}
