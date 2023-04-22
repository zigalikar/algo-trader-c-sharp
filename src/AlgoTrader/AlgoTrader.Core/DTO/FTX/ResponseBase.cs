using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.FTX
{
    public class ResponseBase<T> where T : class
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }
    }
}
