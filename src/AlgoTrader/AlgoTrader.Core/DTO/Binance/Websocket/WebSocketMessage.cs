using System;

using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.JsonConverters;
using AlgoTrader.Core.JsonConverters.Binance;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.Binance.Websocket
{
    public class WebSocketMessage<T> : WebSocketMessage where T : class
    {
        [JsonProperty("k")]
        public T Data { get; set; }
    }

    public class WebSocketMessage
    {
        [JsonProperty("E")]
        [JsonConverter(typeof(MillisecondsToDateTimeConverter))]
        public DateTime EventTime { get; set; }

        [JsonProperty("e")]
        public string EventType { get; set; }

        [JsonProperty("s")]
        [JsonConverter(typeof(CurrencyPairConverter))]
        public ICurrencyPair CurrencyPair { get; set; }
    }
}
