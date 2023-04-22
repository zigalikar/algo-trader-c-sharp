namespace AlgoTrader.Core.Interfaces
{
    /// <summary>
    /// Currency pair denoted by a base and a quote currency (ex: BTCUSD)
    /// </summary>
    public interface ICurrencyPair
    {
        /// <summary>
        /// Base currency of the pair
        /// </summary>
        string Base { get; }

        /// <summary>
        /// Quote currency of the pair
        /// </summary>
        string Quote { get; }
    }
}
