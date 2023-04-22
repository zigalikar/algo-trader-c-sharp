using System.Collections.Generic;

using Newtonsoft.Json;

namespace AlgoTrader.Core.DTO.FTX
{
    /// <summary>
    /// Response DTO for FTX account balance
    /// </summary>
    public class BalancesResponse : ResponseBase<List<BalancesResponseData>> { }

    public class BalancesResponseData
    {
        [JsonProperty("coin")]
        public string Coin { get; set; }

        [JsonProperty("free")]
        public double Free { get; set; }

        [JsonProperty("total")]
        public double Total { get; set; }
    }
}
