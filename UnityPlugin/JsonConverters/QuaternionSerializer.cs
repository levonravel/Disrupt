﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using UnityEngine;

namespace RavelTek.Disrupt.Custom_Serializers
{
    class QuaternionSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Quaternion));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            float x = (float)jo["x"];
            float y = (float)jo["y"];
            float z = (float)jo["z"];
            float w = (float)jo["w"];
            return new Quaternion(x, y, z, w);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Quaternion quaternion = (Quaternion)value;
            JObject jo = new JObject();
            jo.Add("x", quaternion.x);
            jo.Add("y", quaternion.y);
            jo.Add("z", quaternion.z);
            jo.Add("w", quaternion.w);
            jo.WriteTo(writer);
        }
    }
}
