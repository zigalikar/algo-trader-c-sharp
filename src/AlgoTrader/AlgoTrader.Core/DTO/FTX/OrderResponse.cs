using System;
using System.Collections.Generic;

using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.JsonConverters;
using AlgoTrader.Core.JsonConverters.FTX;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.FTX
{
    /// <summary>
    /// Response DTO for FTX orders
    /// </summary>
    public class OrderResponse : ResponseBase<List<OrderResponseData>> { }

    public class OrderResponseData : IOrder
    {
        [JsonProperty("clientId")]
        public string Id { get; set; }

        [JsonProperty("createdAt")]
        [JsonConverter(typeof(DateTimeToUtcConverter))]
        public DateTime Timestamp { get; set; }

        [JsonProperty("filledSize")]
        public double FilledSize { get; set; }

        [JsonProperty("future")]
        public ICurrencyPair Future { get; set; }

        [JsonProperty("id")]
        public long OrderId { get; set; }

        [JsonProperty("market")]
        [JsonConverter(typeof(CurrencyPairConverter))]
        public ICurrencyPair CurrencyPair { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("avgFillPrice")]
        public double AverageFillPrice { get; set; }

        [JsonProperty("remainingSize")]
        public double RemainingSize { get; set; }

        [JsonProperty("side")]
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }

        [JsonProperty("size")]
        public double Amount { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(OrderStatusConverter))]
        public OrderStatus Status { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(OrderTypeConverter))]
        public OrderType Type { get; set; }

        [JsonProperty("reduceOnly")]
        public bool ReduceOnly { get; set; }

        [JsonProperty("ioc")]
        public bool IOC { get; set; }

        [JsonProperty("postOnly")]
        public bool PostOnly { get; set; }
    }
}
