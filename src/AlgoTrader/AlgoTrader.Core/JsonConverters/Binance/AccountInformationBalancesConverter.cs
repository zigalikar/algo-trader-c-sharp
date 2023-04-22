using System;
using System.Collections.Generic;

using AlgoTrader.Core.DTO.Binance;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlgoTrader.Core.JsonConverters.Binance
{
    public abstract class AccountInformationBalancesBaseConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var array = JToken.ReadFrom(reader) as JArray;
            var balances = new List<AccountInformationBalance>();
            foreach (var token in array)
            {
                var balance = AccountInformationBalance.FromJToken(token, GetType() == typeof(AccountInformationBalancesWebsocketConverter));
                balances.Add(balance);
            }
            return balances;
        }
    }

    public class AccountInformationBalancesWebsocketConverter : AccountInformationBalancesBaseConverter { }
    public class AccountInformationBalancesAPIConverter : AccountInformationBalancesBaseConverter { }
}
