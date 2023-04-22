using System;

using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Core.Model.Order.Backtest
{
    /// <summary>
    /// Response for a backtest order
    /// </summary>
    public abstract class BacktestOrder : IOrder
    {
        public string Id { get; private set; }
        public ICurrencyPair CurrencyPair { get; private set; }
        public OrderType Type { get; private set; }
        public OrderSide Side { get; private set; }
        public OrderStatus Status { get; private set; }
        public double Price { get; private set; }
        public double Amount { get; private set; }
        public DateTime Timestamp { get; private set; }

        public BacktestOrder(ICurrencyPair currencyPair, OrderType type, OrderSide side, OrderStatus status, double price, double amount, DateTime timestamp, string id = null)
        {
            if (id == null)
                id = Guid.NewGuid().ToString();
            
            CurrencyPair = currencyPair;
            Type = type;
            Side = side;
            Status = status;
            Price = price;
            Amount = amount;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Response for a backtest order place
    /// </summary>
    public class BacktestOrderPlace : BacktestOrder, IOrder
    {
        public BacktestOrderPlace(ICurrencyPair currencyPair, OrderType type, OrderSide side, OrderStatus status, double price, double amount, DateTime timestamp, string id = null) : base(currencyPair, type, side, status, price, amount, timestamp, id) { }

        public BacktestOrderPlace(IOrder order, DateTime timestamp) : this(order.CurrencyPair, order.Type, order.Side, order.Status, order.Price, order.Amount, timestamp, order.Id) { }
    }

    /// <summary>
    /// Response for a backtest order fill
    /// </summary>
    public class BacktestOrderFill : BacktestOrder, IOrder
    {
        public BacktestOrderFill(ICurrencyPair currencyPair, OrderType type, OrderSide side, OrderStatus status, double price, double amount, DateTime timestamp, string id = null) : base(currencyPair, type, side, status, price, amount, timestamp, id) { }

        public BacktestOrderFill(IOrder order, DateTime timestamp) : this(order.CurrencyPair, order.Type, order.Side, order.Status, order.Price, order.Amount, timestamp, order.Id) { }
    }

    /// <summary>
    /// Response for a backtest order cancel
    /// </summary>
    public class BacktestOrderCancel : BacktestOrder, IOrder
    {
        public BacktestOrderCancel(ICurrencyPair currencyPair, OrderType type, OrderSide side, OrderStatus status, double price, double amount, DateTime timestamp, string id = null) : base(currencyPair, type, side, status, price, amount, timestamp, id) { }

        public BacktestOrderCancel(IOrder order, DateTime timestamp) : this(order.CurrencyPair, order.Type, order.Side, order.Status, order.Price, order.Amount, timestamp, order.Id) { }
    }
}
