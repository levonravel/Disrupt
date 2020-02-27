
using Newtonsoft.Json;
using System;

namespace RavelTek.Disrupt
{
    public class EnumSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            string strValue = value.ToString();
            return Enum.Parse(objectType, strValue);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}