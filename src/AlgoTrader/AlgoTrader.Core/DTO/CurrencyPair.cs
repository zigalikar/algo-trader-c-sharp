using System;

using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Core.DTO
{
    /// <summary>
    /// Describes a tradeable currency pair
    /// </summary>
    public sealed class CurrencyPair : ICurrencyPair
    {
        private readonly Func<string, string, string> _toStringFunc;

        public string Base { get; private set; }
        public string Quote { get; private set; }

        /// <summary>
        /// Creates a new instance of the currency pair.
        /// </summary>
        /// <param name="b">Base currency</param>
        /// <param name="q">Quote currency</param>
        internal CurrencyPair(string b, string q, Func<string, string, string> toStringFunc = null)
        {
            Base = b;
            Quote = q;

            _toStringFunc = toStringFunc ?? throw new ArgumentNullException(nameof(toStringFunc));
        }

        public override string ToString() => _toStringFunc.Invoke(Base, Quote);
    }
}
