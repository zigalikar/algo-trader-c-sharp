namespace AlgoTrader.Core.Model.Order
{
    public enum OrderStatus
    {
        Filled = 0,
        PartiallyFilled = 1,
        Expired = 2, // Binance specific?
        New = 3, // Binance specific?
        Cancelled = 4
    }
}
