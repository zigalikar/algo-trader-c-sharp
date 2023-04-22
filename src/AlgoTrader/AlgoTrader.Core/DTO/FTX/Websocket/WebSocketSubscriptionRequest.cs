using AlgoTrader.Core.Interfaces;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.FTX.Websocket
{
    public class WebSocketSubscriptionRequest
    {
        [JsonProperty("op")]
        public string Operation { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        private WebSocketSubscriptionRequest(bool subscribe, string channel, string market = null)
        {
            Operation = subscribe ? "subscribe" : "unsubscribe";
            Channel = channel;
            Market = market;
        }

        public static WebSocketSubscriptionRequest GetSubscriptionMessage(string channel, string market) => new WebSocketSubscriptionRequest(true, channel, market);

        public static WebSocketSubscriptionRequest SubscribeTrades(ICurrencyPair market) => new WebSocketSubscriptionRequest(true, WebSocketChannels.Trades, market.ToString());
        public static WebSocketSubscriptionRequest UnsubscribeTrades(ICurrencyPair market) => new WebSocketSubscriptionRequest(false, WebSocketChannels.Trades, market.ToString());

        public static WebSocketSubscriptionRequest SubscribeOrders => new WebSocketSubscriptionRequest(true, WebSocketChannels.Orders);
        public static WebSocketSubscriptionRequest UnsubscribeOrders => new WebSocketSubscriptionRequest(false, WebSocketChannels.Orders);

        public static WebSocketSubscriptionRequest SubscribeOrderbook(ICurrencyPair market) => new WebSocketSubscriptionRequest(true, WebSocketChannels.Orderbook, market.ToString());
        public static WebSocketSubscriptionRequest UnsubscribeOrderbook(ICurrencyPair market) => new WebSocketSubscriptionRequest(false, WebSocketChannels.Orderbook, market.ToString());
    }

    public static class WebSocketChannels
    {
        public static string Trades => "trades";
        public static string Orders => "orders";
        public static string Orderbook => "orderbook";
    }
}
