using System;
using System.Linq;
using System.Collections.Generic;

namespace AlgoTrader.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static double Stdev(this IEnumerable<double> values)
        {
            if (values.Any())
            {  
                var mean = values.Average();
                return Math.Sqrt(values.Average(v => (v - mean) * (v - mean)));
            }
            return 0d;
        }
    }
}
