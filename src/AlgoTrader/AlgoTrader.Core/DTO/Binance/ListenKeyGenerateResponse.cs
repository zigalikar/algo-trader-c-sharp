using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.Binance
{
    public class ListenKeyGenerateResponse
    {
        [JsonProperty("listenKey")]
        public string ListenKey { get; set; }
    }
}
