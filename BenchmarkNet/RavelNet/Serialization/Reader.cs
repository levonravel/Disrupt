using Newtonsoft.Json;
using System;
using System.Text;

namespace RavelNet
{
public partial class Reader
    {
        private readonly BitConverter converter = new BitConverter();
        private readonly Encoding encoding = new ASCIIEncoding();
        private UTF8Encoding encoder = new UTF8Encoding();

        public T GetObject<T>(Packet packet)
        {
            try
            {
                var clear = GetString(packet);
                return JsonConvert.DeserializeObject<T>(clear, JsonSettings.Instance.Settings);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString() + "line 13 method Object<T> class Reader.cs");
            }
        }
        public string GetString(Packet packet)
        {
            var stringLen = 0;
            try
            {
                stringLen = GetInt(packet);                
                var eString = encoder.GetString(packet.Payload, packet.CurrentIndex, stringLen);
                packet.CurrentIndex += stringLen;
                return eString;
            }
            catch(Exception e)
            {
                throw new Exception(e.ToString() + "line 25 method String class Reader.cs");
            }
        }
        public string[] GetStringArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new string[count];
            for(int i = 0; i < count; i++)
            {
                var stringLen = GetInt(packet);                
                var eString = encoder.GetString(packet.Payload, packet.CurrentIndex, stringLen);
                packet.CurrentIndex += stringLen;
                values[i] = eString;
            }
            return values;
        }
        public bool GetBool(Packet packet)
        {            
            var value = packet.Payload[packet.CurrentIndex];
            packet.CurrentIndex++;
            return value == 1;
        }
        public bool[] GetBoolArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new bool[count];
            for (int i = 0; i < count; i++)
            {
                var value = packet.Payload[packet.CurrentIndex];
                packet.CurrentIndex++;
                values[i] = value == 1;
            }
            return values;
        }
        public sbyte GetSByte(Packet packet)
        {
            var data = converter.ToSByte(packet);
            return data;
        }
        public sbyte[] GetSbyteArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new sbyte[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToSByte(packet);
            }
            return values;
        }
        public byte GetByte(Packet packet)
        {
            var value = converter.ToByte(packet);
            return value;
        }
        public byte[] GetByteArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new byte[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToByte(packet);
            }
            return values;
        }
        public short GetShort(Packet packet)
        {            
            return converter.ToShort(packet);
        }
        public short[] GetShortArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new short[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToShort(packet);
            }
            return values;
        }
        public int GetInt(Packet packet)
        {
            return converter.ToInt(packet);
        }
        public int[] GetIntArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new int[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToInt(packet);
            }
            return values;
        }
        public long GetLong(Packet packet)
        {
            return converter.ToLong(packet);
        }
        public long[] GetLongArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new long[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToLong(packet);
            }
            return values;
        }
        public ushort GetUShort(Packet packet)
        {
            return converter.ToUShort(packet);
        }
        public ushort[] GetUShortArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToUShort(packet);
            }
            return values;
        }
        public uint GetUInt(Packet packet)
        {
            return converter.ToUInt(packet);
        }
        public uint[] GetUIntArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new uint[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToUInt(packet);
            }
            return values;
        }
        public ulong GetULong(Packet packet)
        {
            return converter.ToULong(packet);
        }
        public ulong[] GetULongArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new ulong[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToULong(packet);
            }
            return values;
        }
        public char GetChar(Packet packet)
        {
            return converter.ToChar(packet);
        }
        public char[] GetCharArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new char[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToChar(packet);
            }
            return values;
        }
        public float GetFloat(Packet packet)
        {
            return converter.ToFloat(packet);
        }
        public float[] GetFloatArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new float[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToFloat(packet);
            }
            return values;
        }
        public decimal GetDecimal(Packet packet)
        {
            return converter.ToDecimal(packet);
        }
        public decimal[] GetDecimalArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new decimal[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToDecimal(packet);
            }
            return values;
        }
        public double GetDouble(Packet packet)
        {
            return converter.ToDouble(packet);
        }
        public double[] GetDoubleArray(Packet packet)
        {
            var count = GetInt(packet);
            var values = new double[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToDouble(packet);
            }
            return values;
        }
    }
}
