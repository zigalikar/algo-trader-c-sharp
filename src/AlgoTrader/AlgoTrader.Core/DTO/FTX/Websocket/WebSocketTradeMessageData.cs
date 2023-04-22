using System;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.JsonConverters;
using AlgoTrader.Core.JsonConverters.FTX;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.FTX.Websocket
{
    public class WebSocketTradeMessageData
    {
        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("size")]
        public double Size { get; set; }

        [JsonProperty("side")]
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide TakerSide { get; set; }

        [JsonProperty("liquidation")]
        public bool Liquidation { get; set; }

        [JsonProperty("time")]
        [JsonConverter(typeof(DateTimeToUtcConverter))]
        public DateTime Timestamp { get; set; }

        public Trade ToTrade(ICurrencyPair currencyPair) => new Trade(Timestamp, currencyPair, Price, Size, TakerSide == OrderSide.Sell ? OrderSide.Buy : OrderSide.Sell, Liquidation);
    }
}
