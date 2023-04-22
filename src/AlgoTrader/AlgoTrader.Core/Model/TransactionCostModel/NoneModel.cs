using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Core.Model.TransactionCostModel
{
    /// <summary>
    /// Zero transaction costs model
    /// </summary>
    public class NoneModel : ITransactionCostModel
    {
        public double Calculate(double amount) => 0;
    }
}
