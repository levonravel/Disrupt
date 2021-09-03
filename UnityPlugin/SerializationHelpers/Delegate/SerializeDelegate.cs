using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SerializeDelegate
{
    public static string SerializeToString<T>(T value)
    {
        using (var stream = new MemoryStream())
        {
            (new BinaryFormatter()).Serialize(stream, value);
            stream.Flush();
            return Convert.ToBase64String(stream.ToArray());
        }
    }
    public static T DeserializeFromString<T>(string data)
    {
        byte[] bytes = Convert.FromBase64String(data);
        using (var stream = new MemoryStream(bytes))
        {
            return (T)(new BinaryFormatter()).Deserialize(stream);
        }
    }
}
