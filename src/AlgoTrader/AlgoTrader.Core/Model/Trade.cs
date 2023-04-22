using System;

using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;

namespace AlgoTrader.Core.Model
{
    public class Trade : IPriceTick
    {
        public DateTime Timestamp { get; set; }
        public ICurrencyPair CurrencyPair { get; set; }
        public double Price { get; set; }
        public double TradeSize { get; set; }
        public OrderSide MakerSide { get; set; }
        public bool? Liquidation { get; set; }

        public Trade(DateTime timestamp, ICurrencyPair currencyPair, double price, double amount, OrderSide makerSide, bool? liquidation)
        {
            Timestamp = timestamp;
            CurrencyPair = currencyPair;
            Price = price;
            TradeSize = amount;
            MakerSide = makerSide;
            Liquidation = liquidation;
        }
    }
}
