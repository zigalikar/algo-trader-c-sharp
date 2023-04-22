using System;

using AlgoTrader.Core.Extensions.Binance;

using Newtonsoft.Json;

namespace AlgoTrader.Core.JsonConverters.Binance
{
    class IntervalToTimeFrameConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => (reader.Value as string).FromBinanceIntervalToTimeFrameEnum();
    }
}
