using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.FTX.Websocket
{
    public class WebSocketAuthenticationMessage
    {
        [JsonProperty("args")]
        public WebSocketAuthenticationArgs Arguments { get; set; }

        [JsonProperty("op")]
        public string Operation => "login";

        public WebSocketAuthenticationMessage(WebSocketAuthenticationArgs args)
        {
            Arguments = args;
        }
    }

    public class WebSocketAuthenticationArgs
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("sign")]
        public string Signature { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }
    }
}
