using AlgoTrader.Core.Interfaces;

namespace AlgoTrader.Core.Model.Http.WebsocketTypes
{
    /// <summary>
    /// Collection of websockets that are available on the exchange
    /// </summary>
    public class ExchangeWebsocketCollection
    {
        public DataWebsocket<ICandlestick> Price { get; }

        public ExchangeWebsocketCollection(DataWebsocket<ICandlestick> price)
        {
            Price = price;
        }
    }
}
