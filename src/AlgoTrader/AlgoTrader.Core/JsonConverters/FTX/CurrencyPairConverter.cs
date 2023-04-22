using System;

using AlgoTrader.Core.DTO.FTX;

using Newtonsoft.Json;

namespace AlgoTrader.Core.JsonConverters.FTX
{
    public class CurrencyPairConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var val = reader.Value as string;
            var pair = CurrencyPairs.FromString(val);
            return pair ?? throw new NotImplementedException();
        }
    }
}
