using System;

using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Core.DTO.FTX
{
    /// <summary>
    /// Static helper class for currency pairs on FTX
    /// </summary>
    public static class CurrencyPairs
    {
        /// <summary>
        /// Function for converting currency pair to string
        /// </summary>
        private static readonly Func<string, string, string> ToStringFunc = new Func<string, string, string>((b, q) => string.Format("{0}/{1}", b, q));

        /// <summary>
        /// Bitcoin / USD
        /// </summary>
        public static ICurrencyPair BTCUSD => new CurrencyPair("BTC", "USD", ToStringFunc);

        /// <summary>
        /// Converts a string to a currency pair
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <returns>Currency pair analog to the specified string</returns>
        public static ICurrencyPair FromString(string str)
        {
            str = str.ToLower();
            if ("btc/usd".Equals(str))
                return BTCUSD;
            return null;
        }
    }
}
