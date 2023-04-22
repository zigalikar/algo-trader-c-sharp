using System;
using System.Collections.Generic;

using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.JsonConverters;
using AlgoTrader.Core.JsonConverters.Binance;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlgoTrader.Core.DTO.Binance
{
    public class AccountInformationResponse
    {
        [JsonProperty("makerCommission")]
        public double MakerCommission { get; set; }

        [JsonProperty("takerCommission")]
        public double TakerCommission { get; set; }

        [JsonProperty("buyerCommission")]
        public double BuyerCommission { get; set; }

        [JsonProperty("sellerCommission")]
        public double SellerCommission { get; set; }

        [JsonProperty("canTrade")]
        public bool CanTrade { get; set; }

        [JsonProperty("canWithdraw")]
        public bool CanTWithdraw { get; set; }

        [JsonProperty("canDeposit")]
        public bool CanDeposit { get; set; }

        [JsonProperty("updateTime")]
        [JsonConverter(typeof(MillisecondsToDateTimeConverter))]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("balances")]
        [JsonConverter(typeof(AccountInformationBalancesAPIConverter))]
        public IList<AccountInformationBalance> Balances { get; set; }

        [JsonProperty("permissions")]
        public IList<string> Permissions { get; set; }
    }

    public class AccountInformationBalance : IAccountInformationBalance
    {
        public string Asset { get; set; }
        public double Free { get; set; }
        public double Locked { get; set; }

        private AccountInformationBalance() { }

        public static AccountInformationBalance FromJToken(JToken obj, bool websocket)
        {
            var asset = obj.Value<string>(websocket ? "a" : "asset");
            var free = double.Parse(obj.Value<string>(websocket ? "f" : "free"));
            var locked = double.Parse(obj.Value<string>(websocket ? "l" : "locked"));
            return new AccountInformationBalance
            {
                Asset = asset,
                Free = free,
                Locked = locked
            };
        }
    }
}
