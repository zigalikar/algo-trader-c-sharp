using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Model.Order;

namespace AlgoTrader.Core.Interfaces
{
    /// <summary>
    /// Exchange interface
    /// </summary>
    public interface IExchange
    {
        /// <summary>
        /// List of all open orders
        /// </summary>
        IList<IOrder> OpenOrders { get; }

        /// <summary>
        /// Dictionary of available balances
        /// </summary>
        IDictionary<string, double> AvailableBalances { get; }

        /// <summary>
        /// Emits when a new order has been placed
        /// </summary>
        event EventHandler<IOrder> OrderPlaced;
        
        /// <summary>
        /// Emits when an order has been fully filled
        /// </summary>
        event EventHandler<IOrder> OrderFilled;

        /// <summary>
        /// Emits when an order has been cancelled
        /// </summary>
        event EventHandler<IOrder> OrderCancelled;

        /// <summary>
        /// Initializes the exchange
        /// </summary>
        Task Initialize();

        /// <summary>
        /// Parsed the ICurrencyPair object from the provided string
        /// </summary>
        /// <param name="value">String toparse</param>
        /// <returns>Currency pair</returns>
        ICurrencyPair GetCurrencyPair(string value);

        /// <summary>
        /// Gets the candlestick feed for the specified market and timeframe
        /// </summary>
        /// <param name="currencyPair">Market to listen on</param>
        /// <param name="timeFrame">Time frame to listen on</param>
        /// <returns>Candlestick feed</returns>
        Task<IFeed<ICandlestick>> GetCandlestickFeed(ICurrencyPair currencyPair, TimeFrameEnum timeFrame);

        /// <summary>
        /// Gets the trade data feed for the specified market
        /// </summary>
        /// <param name="market">Market to listen on</param>
        /// <returns>Trade data feed</returns>
        Task<IFeed<Trade>> GetTradesFeed(ICurrencyPair market);

        /// <summary>
        /// Gets the orderbook feed for the specified market
        /// </summary>
        /// <param name="market">Market to listen on</param>
        /// <returns>Orderbook feed</returns>
        Task<IFeed<Orderbook>> GetOrderbookFeed(ICurrencyPair market);

        /// <summary>
        /// Gets the price data for the specified length in the past (sorted by oldest first)
        /// </summary>
        /// <param name="currencyPair">Currency pair to fetch the data for</param>
        /// <param name="timeFrame">Time frame to fetch the data on</param>
        /// <param name="length">Length of the data in the past</param>
        /// <returns>Price data</returns>
        Task<IList<ICandlestick>> GetPriceData(ICurrencyPair currencyPair, TimeFrameEnum timeFrame, int length);

        /// <summary>
        /// Places a market order. Cancels any stop orders currently active.
        /// </summary>
        /// <param name="currencyPair">The currency pair to place a market order for</param>
        /// <param name="amount">Amount of the market order (in base currency)</param>
        /// <param name="side">Side of the market order</param>
        /// <returns>Order fill details</returns>
        Task<IOrder> MarketOrder(ICurrencyPair currencyPair, double amount, OrderSide side);

        /// <summary>
        /// Places a market stop order. Cancels any other stop orders of the same side currently active.
        /// </summary>
        /// <param name="currencyPair">The currency pair to place a stop order for</param>
        /// <param name="amount">Amount of the stop order (in base currency)</param>
        /// <param name="stopPrice">Price of the stop order</param>
        /// <param name="side">Side of the stop order</param>
        /// <returns>Order placement details</returns>
        Task<IOrder> StopMarketOrder(ICurrencyPair currencyPair, double amount, double stopPrice, OrderSide side);

        /// <summary>
        /// Cancels an order
        /// </summary>
        /// <param name="order">The order to cancel</param>
        /// <returns>True if the order was cancelled successfully, false if failed or the specified order does not exist</returns>
        Task<bool> CancelOrder(string id);

        /// <summary>
        /// Cancels all currently open orders
        /// </summary>
        /// <returns></returns>
        Task CancelAllOrders();

        ///// <summary>
        ///// Gets the order book for the specified currency pair
        ///// </summary>
        ///// <param name="currencyPair">Currency pair of the orderbook</param>
        ///// <param name="cts">Cancellation token</param>
        ///// <returns>Orderbook</returns>
        //Task<Orderbook> GetOrderbook(ICurrencyPair currencyPair, CancellationTokenSource cts = null);
    }
}
