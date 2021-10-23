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

        public T Object<T>(Packet packet)
        {
            try
            {
                var clear = String(packet);
                return JsonConvert.DeserializeObject<T>(clear, JsonSettings.Instance.Settings);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString() + "line 13 method Object<T> class Reader.cs");
            }
        }
        public string String(Packet packet)
        {
            var stringLen = 0;
            try
            {
                stringLen = Int(packet);                
                var eString = encoder.GetString(packet.Payload, packet.CurrentIndex, stringLen);
                packet.CurrentIndex += stringLen;
                return eString;
            }
            catch(Exception e)
            {
                throw new Exception(e.ToString() + "line 25 method String class Reader.cs");
            }
        }
        public string[] StringArray(Packet packet)
        {
            var count = Int(packet);
            var values = new string[count];
            for(int i = 0; i < count; i++)
            {
                var stringLen = Int(packet);                
                var eString = encoder.GetString(packet.Payload, packet.CurrentIndex, stringLen);
                packet.CurrentIndex += stringLen;
                values[i] = eString;
            }
            return values;
        }
        public bool Bool(Packet packet)
        {            
            var value = packet.Payload[packet.CurrentIndex];
            packet.CurrentIndex++;
            return value == 1;
        }
        public bool[] BoolArray(Packet packet)
        {
            var count = Int(packet);
            var values = new bool[count];
            for (int i = 0; i < count; i++)
            {
                var value = packet.Payload[packet.CurrentIndex];
                packet.CurrentIndex++;
                values[i] = value == 1;
            }
            return values;
        }
        public sbyte SByte(Packet packet)
        {
            var data = converter.ToSByte(packet);
            return data;
        }
        public sbyte[] SbyteArray(Packet packet)
        {
            var count = Int(packet);
            var values = new sbyte[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToSByte(packet);
            }
            return values;
        }
        public byte Byte(Packet packet)
        {
            var value = converter.ToByte(packet);
            return value;
        }
        public byte[] ByteArray(Packet packet)
        {
            var count = Int(packet);
            var values = new byte[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToByte(packet);
            }
            return values;
        }
        public short Short(Packet packet)
        {            
            return converter.ToShort(packet);
        }
        public short[] ShortArray(Packet packet)
        {
            var count = Int(packet);
            var values = new short[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToShort(packet);
            }
            return values;
        }
        public int Int(Packet packet)
        {
            return converter.ToInt(packet);
        }
        public int[] IntArray(Packet packet)
        {
            var count = Int(packet);
            var values = new int[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToInt(packet);
            }
            return values;
        }
        public long Long(Packet packet)
        {
            return converter.ToLong(packet);
        }
        public long[] LongArray(Packet packet)
        {
            var count = Int(packet);
            var values = new long[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToLong(packet);
            }
            return values;
        }
        public ushort UShort(Packet packet)
        {
            return converter.ToUShort(packet);
        }
        public ushort[] UShortArray(Packet packet)
        {
            var count = Int(packet);
            var values = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToUShort(packet);
            }
            return values;
        }
        public uint UInt(Packet packet)
        {
            return converter.ToUInt(packet);
        }
        public uint[] UIntArray(Packet packet)
        {
            var count = Int(packet);
            var values = new uint[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToUInt(packet);
            }
            return values;
        }
        public ulong ULong(Packet packet)
        {
            return converter.ToULong(packet);
        }
        public ulong[] ULongArray(Packet packet)
        {
            var count = Int(packet);
            var values = new ulong[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToULong(packet);
            }
            return values;
        }
        public char Char(Packet packet)
        {
            return converter.ToChar(packet);
        }
        public char[] CharArray(Packet packet)
        {
            var count = Int(packet);
            var values = new char[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToChar(packet);
            }
            return values;
        }
        public float Float(Packet packet)
        {
            return converter.ToFloat(packet);
        }
        public float[] FloatArray(Packet packet)
        {
            var count = Int(packet);
            var values = new float[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToFloat(packet);
            }
            return values;
        }
        public decimal Decimal(Packet packet)
        {
            return converter.ToDecimal(packet);
        }
        public decimal[] DecimalArray(Packet packet)
        {
            var count = Int(packet);
            var values = new decimal[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToDecimal(packet);
            }
            return values;
        }
        public double Double(Packet packet)
        {
            return converter.ToDouble(packet);
        }
        public double[] DoubleArray(Packet packet)
        {
            var count = Int(packet);
            var values = new double[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToDouble(packet);
            }
            return values;
        }
    }
}
