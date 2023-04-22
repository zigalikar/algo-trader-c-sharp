using System;

using AlgoTrader.Core.Model.Order;

namespace AlgoTrader.Core.Interfaces
{
    public interface IOrder
    {
        /// <summary>
        /// ID of the order
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Currency pair of the order
        /// </summary>
        ICurrencyPair CurrencyPair { get; }

        /// <summary>
        /// Type of the order
        /// </summary>
        OrderType Type { get; }

        /// <summary>
        /// Side of the order
        /// </summary>
        OrderSide Side { get; }

        /// <summary>
        /// Status of the order
        /// </summary>
        OrderStatus Status { get; }

        /// <summary>
        /// Price of the order
        /// </summary>
        double Price { get; }

        /// <summary>
        /// Amount of the order
        /// </summary>
        double Amount { get; }

        /// <summary>
        /// Timestamp of the order (timestamp of place, cancel, fill, ...)
        /// </summary>
        DateTime Timestamp { get; }
    }
}
