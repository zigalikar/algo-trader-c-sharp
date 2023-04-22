using System;

namespace AlgoTrader.Core.Model
{
    public struct DateValuePair
    {
        public DateTime Date { get; private set; }
        public double Value { get; private set; }

        public DateValuePair(DateTime date, double value) : this()
        {
            Date = date;
            Value = value;
        }
    }
}
