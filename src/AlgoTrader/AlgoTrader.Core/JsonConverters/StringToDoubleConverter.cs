using System;
using System.Globalization;

using Newtonsoft.Json;

namespace AlgoTrader.Core.JsonConverters
{
    public class StringToDoubleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => double.Parse(reader.Value as string, NumberFormatInfo.InvariantInfo);
    }
}
