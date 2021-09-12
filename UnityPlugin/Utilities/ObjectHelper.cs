using UnityEngine;
using System;
using System.Collections.Generic;

namespace RavelTek.Disrupt.Serializers {
  public static class ObjectHelper {
    private static Type type;
    private static Writer writer = new Writer();
    private static Reader reader = new Reader();
    private static List<object[]> data = new List<object[]>();
    private static Dictionary<NetHelper.DataType, Action<Packet, NetHelper.Ts, Writer>> pushData = new Dictionary<NetHelper.DataType, Action<Packet, NetHelper.Ts, Writer>>()
    {
            {
                NetHelper.DataType.Boolean, (packet, value, writer) =>
                {
                    writer.Push((byte)0, packet);
                    writer.Push((bool)value.Boolean, packet);
                }
            },
            {
                NetHelper.DataType.BooleanA, (packet, value, writer) =>
                {
                    writer.Push((byte)1, packet);
                    writer.Push((bool[])value.BooleanA, packet);
                }
            },
            {
                NetHelper.DataType.SByte, (packet, value, writer) =>
                {
                    writer.Push((byte)2, packet);
                    writer.Push((sbyte)value.SByte, packet);
                }
            },
            {
                NetHelper.DataType.SByteA, (packet, value, writer) =>
                {
                    writer.Push((byte)3, packet);
                    writer.Push((sbyte[])value.SByteA, packet);
                }
            },
            {
                NetHelper.DataType.Byte, (packet, value, writer) =>
                {
                    writer.Push((byte)4, packet);
                    writer.Push((byte)value.Byte, packet);
                }
            },
            {
                NetHelper.DataType.ByteA, (packet, value, writer) =>
                {
                    writer.Push((byte)5, packet);
                    writer.Push((byte[])value.ByteA, packet);
                }
            },
            {
                NetHelper.DataType.Char, (packet, value, writer) =>
                {
                    writer.Push((byte)6, packet);
                    writer.Push((char)value.Char, packet);
                }
            },
            {
                NetHelper.DataType.CharA, (packet, value, writer) =>
                {
                    writer.Push((byte)7, packet);
                    writer.Push((char[])value.CharA, packet);
                }
            },
            {
                NetHelper.DataType.UInt16, (packet, value, writer) =>
                {
                    writer.Push((byte)8, packet);
                    writer.Push((ushort)value.UInt16, packet);
                }
            },
            {
                NetHelper.DataType.UInt16A, (packet, value, writer) =>
                {
                    writer.Push((byte)9, packet);
                    writer.Push((ushort[])value.UInt16A, packet);
                }
            },            {
                NetHelper.DataType.Int16, (packet, value, writer) =>
                {
                    writer.Push((byte)10, packet);
                    writer.Push((short)value.Int16, packet);
                }
            },
            {
                NetHelper.DataType.Int16A, (packet, value, writer) =>
                {
                    writer.Push((byte)11, packet);
                    writer.Push((short[])value.Int16A, packet);
                }
            },
            {
                NetHelper.DataType.UInt32, (packet, value, writer) =>
                {
                    writer.Push((byte)12, packet);
                    writer.Push((uint)value.UInt32, packet);
                }
            },
            {
                NetHelper.DataType.UInt32A, (packet, value, writer) =>
                {
                    writer.Push((byte)13, packet);
                    writer.Push((uint[])value.UInt32A, packet);
                }
            },
            {
                NetHelper.DataType.Int32, (packet, value, writer) =>
                {
                    writer.Push((byte)14, packet);
                    writer.Push((int)value.Int32, packet);
                }
            },
            {
                NetHelper.DataType.Int32A, (packet, value, writer) =>
                {
                    writer.Push((byte)15, packet);
                    writer.Push((int[])value.Int32A, packet);
                }
            },
            {
                NetHelper.DataType.Single, (packet, value, writer) =>
                {
                    writer.Push((byte)16, packet);
                    writer.Push((float)value.Single, packet);
                }
            },
            {
                NetHelper.DataType.SingleA, (packet, value, writer) =>
                {
                    writer.Push((byte)17, packet);
                    writer.Push((float[])value.SingleA, packet);
                }
            },
            {
                NetHelper.DataType.UInt64, (packet, value, writer) =>
                {
                    writer.Push((byte)18, packet);
                    writer.Push((ulong)value.UInt64, packet);
                }
            },
            {
                NetHelper.DataType.UInt64A, (packet, value, writer) =>
                {
                    writer.Push((byte)19, packet);
                    writer.Push((ulong[])value.UInt64A, packet);
                }
            },
                        {
                NetHelper.DataType.Int64, (packet, value, writer) =>
                {
                    writer.Push((byte)20, packet);
                    writer.Push((long)value.Int64, packet);
                }
            },
            {
                NetHelper.DataType.Int64A, (packet, value, writer) =>
                {
                    writer.Push((byte)21, packet);
                    writer.Push((long[])value.Int64A, packet);
                }
            },
            {
                NetHelper.DataType.Decimal, (packet, value, writer) =>
                {
                    writer.Push((byte)22, packet);
                    writer.Push((decimal)value.Decimal, packet);
                }
            },
            {
                NetHelper.DataType.DecimalA, (packet, value, writer) =>
                {
                    writer.Push((byte)23, packet);
                    writer.Push((decimal[])value.DecimalA, packet);
                }
            },
            {
                NetHelper.DataType.Double, (packet, value, writer) =>
                {
                    writer.Push((byte)24, packet);
                    writer.Push((double)value.Double, packet);
                }
            },
            {
                NetHelper.DataType.DoubleA, (packet, value, writer) =>
                {
                    writer.Push((byte)25, packet);
                    writer.Push((double[])value.DoubleA, packet);
                }
            },
            {
                NetHelper.DataType.String, (packet, value, writer) =>
                {
                    writer.Push((byte)26, packet);
                    writer.Push((string)value.String, packet);
                }
            },
            {
                NetHelper.DataType.StringA, (packet, value, writer) =>
                {
                    writer.Push((byte)27, packet);
                    writer.Push((string[])value.StringA, packet);
                }
            },
            {
                NetHelper.DataType.Object, (packet, value, writer) =>
                {
                  Type _o = FindType(ref value.Object);
                    writer.Push((byte)28, packet);
                    writer.Push(value.Object, packet);
                }
            },
            {
                NetHelper.DataType.ObjectA, (packet, value, writer) =>
                {
                    var objectValue = (object[])value.ObjectA;
                    writer.Push((byte)33, packet);
                    writer.Push(objectValue.Length, packet);
                    //writer push value length
                    for (int i = 0; i < objectValue.Length; i++)
                    {
                        PushDataObj(packet, FindType(ref objectValue[i]), objectValue[i]);
                    }
                }
            },            
            {
                NetHelper.DataType.Vector3, (packet, value, writer) =>
                {
                    writer.Push((byte)29, packet);
                    var vector3 = (Vector3)value.Vector3;
                    writer.Push(vector3.x, packet);
                    writer.Push(vector3.y, packet);
                    writer.Push(vector3.z, packet);
                }
            },
            {
                NetHelper.DataType.Vector3A, (packet, value, writer) =>
                {
                    writer.Push((byte)30, packet);
                    var vector3a = (Vector3[])value.Vector3A;
                    writer.Push(vector3a.Length, packet);
                    foreach (var vector in vector3a)
                    {
                        writer.Push(vector.x, packet);
                        writer.Push(vector.y, packet);
                        writer.Push(vector.z, packet);
                    }
                }
            },
            {
                NetHelper.DataType.Quaternion, (packet, value, writer) =>
                {
                    writer.Push((byte)31, packet);
                    var quaternion = (Quaternion)value.Quaternion;
                    writer.Push(quaternion.x, packet);
                    writer.Push(quaternion.y, packet);
                    writer.Push(quaternion.z, packet);
                    writer.Push(quaternion.w, packet);
                }
            },
            {
                NetHelper.DataType.QuaternionA, (packet, value, writer) =>
                {
                    writer.Push((byte)32, packet);
                    var quaternionA = (Quaternion[])value.QuaternionA;
                    writer.Push(quaternionA.Length, packet);
                    foreach (var quat in quaternionA)
                    {
                        writer.Push(quat.x, packet);
                        writer.Push(quat.y, packet);
                        writer.Push(quat.z, packet);
                        writer.Push(quat.w, packet);
                    }
                }
            },
        };
    public static void PushData(Packet packet, Queue<NetHelper.Data> value) {
      int _c = value.Count;
      writer.Push(_c, packet);
      for (int i = 0; i < _c; i++) {
        var _o = value.Dequeue();
        PushData(packet, _o.dataType, _o.val);
      }
    }
    public static object[] PullData(Packet packet) {
      var length = reader.PullInt(packet);
      var data = new object[length];
      for (int i = 0; i < length; i++) {
        var index = reader.PullByte(packet);
        data[i] = PullData(packet, index);
      }
      return data;
    }
    private static void PushData(Packet packet, NetHelper.DataType type, NetHelper.Ts value) {
      try {
        pushData[type](packet, value, writer);
      }
      catch {
        Type _o = FindType(ref value.Object);
        writer.Push((byte)28, packet);
        writer.Push(_o.FullName + "," + _o.Assembly.GetName().Name, packet);
        writer.Push(value, packet);
      }
    }
    private static void PushDataObj(Packet packet, Type type, object value) {
      try {
        var _d = new NetHelper.Data();
        _d.dataType = NetHelper.DataType.Object;
        _d.val.Object = value;
        pushData[_d.dataType](packet, _d.val, writer);
      }
      catch {
        writer.Push((byte)28, packet);
        writer.Push(type.FullName + "," + type.Assembly.GetName().Name, packet);
        writer.Push(value, packet);
      }
    }
    private static object PullData(Packet packet, int index) {
      switch (index) {
        case 0:
          return reader.PullBool(packet);
        case 1:
          return reader.PullBoolArray(packet);
        case 2:
          return reader.PullSByte(packet);
        case 3:
          return reader.PullSbyteArray(packet);
        case 4:
          return reader.PullByte(packet);
        case 5:
          return reader.PullByteArray(packet);
        case 6:
          return reader.PullChar(packet);
        case 7:
          return reader.PullCharArray(packet);
        case 8:
          return reader.PullUShort(packet);
        case 9:
          return reader.PullUShortArray(packet);
        case 10:
          return reader.PullShort(packet);
        case 11:
          return reader.PullShortArray(packet);
        case 12:
          return reader.PullUInt(packet);
        case 13:
          return reader.PullUIntArray(packet);
        case 14:
          return reader.PullInt(packet);
        case 15:
          return reader.PullIntArray(packet);
        case 16:
          return reader.PullFloat(packet);
        case 17:
          return reader.PullFloatArray(packet);
        case 18:
          return reader.PullULong(packet);
        case 19:
          return reader.PullULongArray(packet);
        case 20:
          return reader.PullLong(packet);
        case 21:
          return reader.PullLongArray(packet);
        case 22:
          return reader.PullDecimal(packet);
        case 23:
          return reader.PullDecimalArray(packet);
        case 24:
          return reader.PullDouble(packet);
        case 25:
          return reader.PullDoubleArray(packet);
        case 26:
          var stringData = reader.PullString(packet);
          if (stringData == string.Empty)
            return null;
          return stringData;
        case 27:
          return reader.PullStringArray(packet);
        case 28:
          var objectType = reader.PullString(packet);
          var type = Type.GetType(objectType);
          return reader.PullObject(type, packet);
        case 29:
          return new Vector3(reader.PullFloat(packet), reader.PullFloat(packet), reader.PullFloat(packet));
        case 30:
          var vectori = reader.PullInt(packet);
          var vector3a = new Vector3[vectori];
          for (int x = 0; x < vectori; x++) {
            vector3a[x] = new Vector3(reader.PullFloat(packet), reader.PullFloat(packet), reader.PullFloat(packet));
          }
          return vector3a;
        case 31:
          return new Quaternion(reader.PullFloat(packet), reader.PullFloat(packet),
              reader.PullFloat(packet), reader.PullFloat(packet));
        case 32:
          var quaternioni = reader.PullInt(packet);
          var quaternionA = new Quaternion[quaternioni];
          for (int x = 0; x < quaternioni; x++) {
            quaternionA[x] = new Quaternion(reader.PullFloat(packet), reader.PullFloat(packet),
                reader.PullFloat(packet), reader.PullFloat(packet));
          }
          return quaternionA;
        case 33:
          var objectLength = reader.PullInt(packet);
          var tempArray = new object[objectLength];
          for (int x = 0; x < objectLength; x++) {
            var idx = reader.PullByte(packet);
            tempArray[x] = PullData(packet, idx);
          }
          return tempArray;
        default:
          Debug.LogError($"Something went wrong in ObjectHelper.cs pull conversion couldnt be completed {index}. Packet ID: {packet.Id}");
          return null;
      }
    }
    private static Type FindType(ref object value) {
      if (value == null) {
        value = string.Empty;
        return typeof(string);
      }
      return value.GetType();
    }
  }
}
