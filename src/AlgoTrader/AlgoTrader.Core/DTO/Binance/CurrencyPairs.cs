using System;

using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Core.DTO.Binance
{
    /// <summary>
    /// Static helper class for currency pairs on Binance
    /// </summary>
    public static class CurrencyPairs
    {
        /// <summary>
        /// Function for converting currency pair to string
        /// </summary>
        private static readonly Func<string, string, string> ToStringFunc = new Func<string, string, string>((b, q) => string.Format("{0}{1}", b, q));

        /// <summary>
        /// Bitcoin / USD Tether
        /// </summary>
        public static ICurrencyPair BTCUSDT => new CurrencyPair("BTC", "USDT", ToStringFunc);

        /// <summary>
        /// BNB / USD Tether
        /// </summary>
        public static ICurrencyPair BNBUSDT => new CurrencyPair("BNB", "USDT", ToStringFunc);

        /// <summary>
        /// Converts a string to a currency pair
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <returns>Currency pair analog to the specified string</returns>
        public static ICurrencyPair FromString(string str)
        {
            str = str.ToLower();
            if ("btcusdt".Equals(str))
                return BTCUSDT;
            else if ("bnbusdt".Equals(str))
                return BNBUSDT;
            return null;
        }
    }
}
