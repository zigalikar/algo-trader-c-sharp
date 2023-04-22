using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.JsonConverters.FTX;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlgoTrader.Core.DTO.FTX.Websocket
{
    public class WebSocketMessage<T> : WebSocketMessage where T : class
    {
        [JsonProperty("data")]
        public new T Data { get; set; }
    }

    public class WebSocketMessage
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("market")]
        [JsonConverter(typeof(CurrencyPairConverter))]
        public ICurrencyPair Market { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("code")]
        public int? ErrorCode { get; set; }

        [JsonProperty("msg")]
        public string ErrorMessage { get; set; }

        [JsonProperty("data")]
        public JToken Data { get; set; }

        public WebSocketMessage<T> CastData<T>() where T : class => new WebSocketMessage<T>
        {
            Channel = Channel,
            Market = Market,
            Type = Type,
            ErrorCode = ErrorCode,
            ErrorMessage = ErrorMessage,
            Data = Data.ToObject<T>()
        };
    }
}
