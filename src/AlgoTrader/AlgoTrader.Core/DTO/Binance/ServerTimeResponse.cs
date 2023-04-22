using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.Binance
{
    /// <summary>
    /// Response object about server time from Binance API
    /// </summary>
    public class ServerTimeResponse
    {
        [JsonProperty("serverTime")]
        public long ServerTime { get; set; }
    }
}
