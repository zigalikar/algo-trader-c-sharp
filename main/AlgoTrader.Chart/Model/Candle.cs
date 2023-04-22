using System;

using FancyCandles;

namespace AlgoTrader.Chart.Model
{
    public class Candle : CandlePrices, ICandle
    {
        public DateTime t { get; set; }
        public ICandlePrices Portfolio { get; set; }
        public ICandleOrderInfo Order { get; set; }

        public Candle(DateTime t, double o, double h, double l, double c, long v) : base(o, h, l, c, v)
        {
            this.t = t;
        }

        public Candle(DateTime t, double o, double h, double l, double c, long v, CandlePrices portfolio) : this(t, o, h, l, c, v)
        {
            Portfolio = portfolio;
        }
    }

    public class CandlePrices : ICandlePrices
    {
        public double O { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        public double C { get; set; }
        public long V { get; set; }

        public CandlePrices(double o, double h, double l, double c, long v)
        {
            O = o;
            H = h;
            L = l;
            C = c;
            V = v;
        }

        public CandlePrices(double price)
        {
            O = price;
            H = price;
            L = price;
            C = price;
        }
    }

    public class CandleOrderInfo : ICandleOrderInfo
    {
        public WholeContainerCandleOrderType Type { get; }
        public double Price { get; }

        public CandleOrderInfo(WholeContainerCandleOrderType type, double price)
        {
            Type = type;
            Price = price;
        }
    }
}
