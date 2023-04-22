using System;

namespace FancyCandles
{
    public interface ICandlePrices
    {
        double O { get; }
        double H { get; }
        double L { get; }
        double C { get; }
        long V { get; }
    }

    public interface ICandle : ICandlePrices
    {
        DateTime t { get; }
        ICandlePrices Portfolio { get; }
        ICandleOrderInfo Order { get; }
    }

    public interface ICandleOrderInfo
    {
        WholeContainerCandleOrderType Type { get; }
        double Price { get; }
    }
}
