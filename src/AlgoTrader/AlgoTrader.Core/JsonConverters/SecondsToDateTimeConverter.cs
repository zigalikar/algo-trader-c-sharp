using System;

using AlgoTrader.Core.Extensions;

using Newtonsoft.Json;

namespace AlgoTrader.Core.JsonConverters
{
    public class SecondsToDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is long l)
                return l.ToDateTime();
            else if (reader.Value is double d)
                return ((long) d).ToDateTime();

            throw new NotImplementedException();
        }
    }
}
