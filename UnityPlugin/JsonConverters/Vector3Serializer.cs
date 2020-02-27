using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using UnityEngine;

namespace RavelTek.Disrupt.Custom_Serializers
{
    class Vector3Serializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Vector3));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            float x = (float)jo["x"];
            float y = (float)jo["y"];
            float z = (float)jo["z"];
            return new Vector3(x, y, z);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Vector3 vector3 = (Vector3)value;
            JObject jo = new JObject();
            jo.Add("x", vector3.x);
            jo.Add("y", vector3.y);
            jo.Add("z", vector3.z);
            jo.WriteTo(writer);
        }
    }
}
