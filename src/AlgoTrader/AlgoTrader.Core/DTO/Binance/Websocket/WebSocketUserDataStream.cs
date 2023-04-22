using System;
using System.Collections.Generic;

using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.JsonConverters;
using AlgoTrader.Core.JsonConverters.Binance;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlgoTrader.Core.DTO.Binance.Websocket
{
    public class WebSocketUserDataStream : WebSocketMessage
    {
        public static string AccountUpdateType => "outboundaccountposition";
        public static string OrderUpdateType => "executionreport";
        public static string OrderUpdateOCOType => "liststatus";
    }

    public class WebSocketAccountUpdate : WebSocketUserDataStream, IWebSocketAccountUpdate
    {
        [JsonProperty("u")]
        [JsonConverter(typeof(MillisecondsToDateTimeConverter))]
        public DateTime TimeOfLastAccountUpdate { get; set; }

        [JsonProperty("B")]
        [JsonConverter(typeof(AccountInformationBalancesWebsocketConverter))]
        public IList<IAccountInformationBalance> Balances { get; set; }
    }

    public abstract class WebSocketOrderUpdateBase : WebSocketUserDataStream
    {
        [JsonProperty("T")]
        [JsonConverter(typeof(MillisecondsToDateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }

    public class WebSocketOrderUpdate : WebSocketOrderUpdateBase, IOrder
    {
        /// <summary>
        /// Order ID
        /// </summary>
        [JsonProperty("c")]
        public string Id { get; set; }

        [JsonProperty("S")]
        [JsonConverter(typeof(OrderSideConverter))]
        public OrderSide Side { get; set; }

        [JsonProperty("o")]
        [JsonConverter(typeof(OrderTypeConverter))]
        public OrderType Type { get; set; }

        [JsonProperty("f")]
        public string TimeInForce { get; set; }

        [JsonProperty("q")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double Amount { get; set; }

        [JsonProperty("p")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double Price { get; set; }

        [JsonProperty("P")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double StopPrice { get; set; }

        [JsonProperty("F")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double IcebergQuantity { get; set; }

        [JsonProperty("C")]
        public string OriginalClientOrderId { get; set; } // ID of the order being canceled

        [JsonProperty("x")]
        public string CurrentExecutionType { get; set; } // TODO: converter

        [JsonProperty("X")]
        [JsonConverter(typeof(OrderStatusConverter))]
        public OrderStatus Status { get; set; }

        [JsonProperty("r")]
        public string OrderRejectReason { get; set; }
        
        [JsonProperty("i")]
        public long OrderId { get; set; }

        [JsonProperty("l")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double LastExecutedQuantity { get; set; }

        [JsonProperty("z")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double CumulativeFilledQuantity { get; set; }

        [JsonProperty("L")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double LastExecutedPrice { get; set; }

        [JsonProperty("n")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double CommisionAmount { get; set; }

        [JsonProperty("N")]
        public string CommisionAsset { get; set; }

        [JsonProperty("t")]
        public int TradeId { get; set; }

        [JsonProperty("w")]
        public bool OrderOnTheBook { get; set; }

        [JsonProperty("m")]
        public bool IsMaker { get; set; }

        [JsonProperty("O")]
        [JsonConverter(typeof(MillisecondsToDateTimeConverter))]
        public DateTime OrderCreationTime { get; set; }

        [JsonProperty("Z")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double CumulativeQuoteAssetTransactedQuantity { get; set; }

        [JsonProperty("Y")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double LastQuoteAssetTransactedQuantity { get; set; } // i.e. lastPrice * lastQty

        [JsonProperty("Q")]
        [JsonConverter(typeof(StringToDoubleConverter))]
        public double QuoteOrderQuantity { get; set; }
    }

    public class WebSocketOrderUpdateOCO : WebSocketOrderUpdateBase
    {
        [JsonProperty("g")]
        public int OrderListId { get; set; }

        [JsonProperty("c")]
        public string ContingencyType { get; set; }

        [JsonProperty("l")]
        public string ListStatusType { get; set; } // TODO: converter

        [JsonProperty("L")]
        public string ListOrderStatus { get; set; } // TODO: converter

        [JsonProperty("r")]
        public string ListRejectReason { get; set; } // TODO: converter

        [JsonProperty("C")]
        public string ListClientOrderId { get; set; }

        [JsonProperty("O")]
        [JsonConverter(typeof(WebSocketOrderUpdateOCOItemConverter))]
        public IList<WebSocketOrderUpdateOCOItem> Orders { get; set; }
    }

    public class WebSocketOrderUpdateOCOItem
    {
        public ICurrencyPair CurrencyPair { get; set; }
        public int OrderId { get; set; }
        public string ClientOrderId { get; set; }
        
        private WebSocketOrderUpdateOCOItem() { }

        public static WebSocketOrderUpdateOCOItem FromJToken(JToken obj)
        {
            var symbol = obj.Value<string>("s");
            var orderId = obj.Value<int>("i");
            var clientOrderId = obj.Value<string>("c");
            return new WebSocketOrderUpdateOCOItem
            {
                CurrencyPair = CurrencyPairs.FromString(symbol),
                OrderId = orderId,
                ClientOrderId = clientOrderId
            };
        }
    }
}
