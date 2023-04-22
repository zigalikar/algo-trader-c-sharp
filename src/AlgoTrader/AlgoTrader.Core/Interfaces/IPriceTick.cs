using System;

namespace AlgoTrader.Core.Interfaces
{
    public interface IPriceTick
    {
        DateTime Timestamp { get; }
        double Price { get; }
        double TradeSize { get; }
    }
}
