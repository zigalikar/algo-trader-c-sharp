using System;
using System.Globalization;

using Newtonsoft.Json;

namespace TwitterAnalyser.Core.JsonConverters
{
    public class StringToDateTimeConverter_v1 : JsonConverter
    {
        public override bool CanConvert(Type objectType) => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => DateTime.ParseExact(reader.Value as string, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture);
    }
}
