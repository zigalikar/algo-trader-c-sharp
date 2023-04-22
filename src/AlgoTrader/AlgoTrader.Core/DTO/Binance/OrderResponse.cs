using System;
using System.Collections.Generic;

using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.JsonConverters;
using AlgoTrader.Core.JsonConverters.Binance;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.Binance
{
    /// <summary>
    /// Response DTO for Binance orders
    /// </summary>
    public class OrderResponse : IOrder
    {
        [JsonProperty("orderId")]
        public long OrderId { get; set; }

        [JsonProperty("clientOrderId")]
        public string Id { get; set; }

        [JsonProperty("symbol")]
        [JsonConverter(typeof(CurrencyPairConverter))]
        public ICurrencyPair CurrencyPair { get; set; }

        [JsonProperty("price")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double Price { get; set; }

        [JsonProperty("origQty")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double Amount { get; set; }

        [JsonProperty("executedQty")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double ExecutedAmount { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(OrderStatusConverter))]
        public OrderStatus Status { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(OrderTypeConverter))]
        public OrderType Type { get; set; }

        [JsonProperty("side")]
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }

        [JsonProperty("transactTime")]
        [JsonConverter(typeof(MillisecondsToDateTimeConverter))]
        public DateTime Timestamp { get; set; }

        [JsonProperty("fills")]
        public IList<OrderResponseFill> Fills { get; set; }
    }

    public class OrderResponseFill
    {
        [JsonProperty("price")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double Price { get; set; }

        [JsonProperty("qty")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double Amount { get; set; }

        [JsonProperty("commission")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double Commission { get; set; }

        [JsonProperty("commissionAsset")]
        public string CommissionAsset { get; set; }
    }
}
