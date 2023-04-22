using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Extensions.Binance;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.Binance.Websocket
{
    public class WebSocketSubscriptionRequest
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public List<string> Parameters { get; set; }

        private static int id;
        [JsonProperty("id")]
        public int Id { get; set; }

        private WebSocketSubscriptionRequest(bool subscribe, params string[] param)
        {
            Method = subscribe ? "SUBSCRIBE" : "UNSUBSCRIBE";
            Parameters = param.ToList();
            Id = id++;
        }

        //public static WebSocketSubscriptionRequest GetSubscriptionMessage(string channel, string market) => new WebSocketSubscriptionRequest(true, channel, market);

        public static WebSocketSubscriptionRequest SubscribeCandlesticks(ICurrencyPair market, TimeFrameEnum timeFrame) => new WebSocketSubscriptionRequest(true, WebSocketChannels.Candlesticks(market, timeFrame));
        public static WebSocketSubscriptionRequest UnsubscribeCandlesticks(ICurrencyPair market, TimeFrameEnum timeFrame) => new WebSocketSubscriptionRequest(false, WebSocketChannels.Candlesticks(market, timeFrame));

        //public static WebSocketSubscriptionRequest SubscribeOrders => new WebSocketSubscriptionRequest(true, WebSocketChannels.Orders);
        //public static WebSocketSubscriptionRequest UnsubscribeOrders => new WebSocketSubscriptionRequest(false, WebSocketChannels.Orders);

        //public static WebSocketSubscriptionRequest SubscribeOrderbook(ICurrencyPair market) => new WebSocketSubscriptionRequest(true, WebSocketChannels.Orderbook, market.ToString());
        //public static WebSocketSubscriptionRequest UnsubscribeOrderbook(ICurrencyPair market) => new WebSocketSubscriptionRequest(false, WebSocketChannels.Orderbook, market.ToString());
    }

    public static class WebSocketChannels
    {
        public static string Candlesticks(ICurrencyPair market, TimeFrameEnum timeFrame) => string.Format("{0}@kline_{1}", market.ToString().ToLower(), timeFrame.ToBinanceTimeFrameInterval());
        //public static string Orders => "orders";
        //public static string Orderbook => "orderbook";
    }
}
