using System;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.JsonConverters;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.Binance.Websocket
{
    /// <summary>
    /// Data about the trade received through the websocket
    /// </summary>
    public class WebSocketTradeData : WebSocketMessage
    {
        [JsonProperty("t")]
        public long TradeId { get; set; }

        [JsonProperty("p")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double Price { get; set; }

        [JsonProperty("q")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double Amount { get; set; }

        [JsonProperty("b")]
        public long BuyerOrderId { get; set; }

        [JsonProperty("a")]
        public long SellerOrderId { get; set; }

        [JsonProperty("T")]
        [JsonConverter(typeof(MillisecondsToDateTimeConverter))]
        public DateTime TradeDate { get; set; }

        [JsonProperty("m")]
        public bool BuyerMarketMaker { get; set; }

        [JsonProperty("M")]
        public bool Ignore { get; set; }

        public Trade ToTrade() => new Trade(TradeDate, CurrencyPair, Price, Amount, BuyerMarketMaker ? OrderSide.Buy : OrderSide.Sell, null);
    }
}
