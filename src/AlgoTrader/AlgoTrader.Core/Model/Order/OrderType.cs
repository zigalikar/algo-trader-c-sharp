namespace AlgoTrader.Core.Model.Order
{
    public enum OrderType
    {
        Market = 0,
        Limit = 1,
        StopMarket = 2,
        StopLimit = 3
    }

    public static class OrderTypeExtensions
    {
        public static bool IsStop(this OrderType type) => type == OrderType.StopMarket || type == OrderType.StopLimit;
    }
}
