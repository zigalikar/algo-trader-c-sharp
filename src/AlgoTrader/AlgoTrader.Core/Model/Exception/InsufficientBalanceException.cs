namespace AlgoTrader.Core.Model.Exception
{
    public class InsufficientBalanceException : System.Exception
    {
        public InsufficientBalanceException(string reason) : base(reason) { }
    }
}
