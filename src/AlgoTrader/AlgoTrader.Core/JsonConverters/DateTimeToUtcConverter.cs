using System;

using Newtonsoft.Json;

namespace AlgoTrader.Core.JsonConverters
{
    public class DateTimeToUtcConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteRawValue(((DateTime) value).ToString("o"));
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => ((DateTime) reader.Value).ToUniversalTime();
    }
}
