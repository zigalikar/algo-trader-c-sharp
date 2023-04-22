using System;
using System.Linq;
using System.Collections.Generic;

namespace AlgoTrader.Core.Model
{
    public class Orderbook
    {
        public DateTime Timestamp { get; }
        public IEnumerable<OrderbookItem> Bids { get; }
        public IEnumerable<OrderbookItem> Asks { get; }

        public Orderbook(DateTime timestamp, IEnumerable<OrderbookItem> bids, IEnumerable<OrderbookItem> asks)
        {
            Timestamp = timestamp;
            Bids = bids.ToList();
            Asks = asks.ToList();
        }
    }

    public class OrderbookItem
    {
        public double Price { get; }
        public double Quantity { get; }

        public OrderbookItem(double price, double quantity)
        {
            Price = price;
            Quantity = quantity;
        }
    }
}
