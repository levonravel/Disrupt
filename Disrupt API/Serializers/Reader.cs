using Newtonsoft.Json;
using RavelTek.Disrupt.Custom_Serializers;
using System;
using System.Text;

namespace RavelTek.Disrupt.Serializers
{
public partial class Reader
    {
        private readonly BitConverter converter = new BitConverter();
        private readonly Encoding encoding = new ASCIIEncoding();
        private UTF8Encoding encoder = new UTF8Encoding();
        /// <summary>
        /// Used for Unity3ds Plugin 
        /// Everything uses Generics so this was needed to get the type
        /// for the return
        /// </summary>
        /// <param name="type"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public object PullObject(Type type, Packet packet)
        {
            string clear = null;
            try
            {
                clear = PullString(packet);
                return JsonConvert.DeserializeObject(clear, type, JsonSettings.Instance.Settings);
            }catch(Exception e)
            {               
                return null;
            }
        }
        public T PullObject<T>(Packet packet)
        {
            try
            {
                var clear = PullString(packet);
                return JsonConvert.DeserializeObject<T>(clear, JsonSettings.Instance.Settings);
            }
            catch (Exception e)
            {
                return default(T);
            }
        }
        public string PullString(Packet packet)
        {
            var stringLen = 0;
            try
            {
                stringLen = PullInt(packet);                
                var eString = encoder.GetString(packet.PayLoad, packet.CurrentIndex, stringLen);
                packet.CurrentIndex += stringLen;
                return eString;
            }
            catch(Exception e)
            {
                return null;
            }
        }
        public string[] PullStringArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new string[count];
            for(int i = 0; i < count; i++)
            {
                var stringLen = PullInt(packet);                
                var eString = encoder.GetString(packet.PayLoad, packet.CurrentIndex, stringLen);
                packet.CurrentIndex += stringLen;
                values[i] = eString;
            }
            return values;
        }
        public bool PullBool(Packet packet)
        {            
            var value = packet.PayLoad[packet.CurrentIndex];
            packet.CurrentIndex++;
            return value == 1;
        }
        public bool[] PullBoolArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new bool[count];
            for (int i = 0; i < count; i++)
            {
                var value = packet.PayLoad[packet.CurrentIndex];
                packet.CurrentIndex++;
                values[i] = value == 1;
            }
            return values;
        }
        public sbyte PullSByte(Packet packet)
        {
            var data = converter.ToSByte(packet);
            return data;
        }
        public sbyte[] PullSbyteArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new sbyte[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToSByte(packet);
            }
            return values;
        }
        public byte PullByte(Packet packet)
        {
            var value = converter.ToByte(packet);
            return value;
        }
        public byte[] PullByteArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new byte[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToByte(packet);
            }
            return values;
        }
        public short PullShort(Packet packet)
        {            
            return converter.ToShort(packet);
        }
        public short[] PullShortArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new short[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToShort(packet);
            }
            return values;
        }
        public int PullInt(Packet packet)
        {
            return converter.ToInt(packet);
        }
        public int[] PullIntArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new int[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToInt(packet);
            }
            return values;
        }
        public long PullLong(Packet packet)
        {
            return converter.ToLong(packet);
        }
        public long[] PullLongArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new long[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToLong(packet);
            }
            return values;
        }
        public ushort PullUShort(Packet packet)
        {
            return converter.ToUShort(packet);
        }
        public ushort[] PullUShortArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToUShort(packet);
            }
            return values;
        }
        public uint PullUInt(Packet packet)
        {
            return converter.ToUInt(packet);
        }
        public uint[] PullUIntArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new uint[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToUInt(packet);
            }
            return values;
        }
        public ulong PullULong(Packet packet)
        {
            return converter.ToULong(packet);
        }
        public ulong[] PullULongArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new ulong[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToULong(packet);
            }
            return values;
        }
        public char PullChar(Packet packet)
        {
            return converter.ToChar(packet);
        }
        public char[] PullCharArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new char[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToChar(packet);
            }
            return values;
        }
        public float PullFloat(Packet packet)
        {
            return converter.ToFloat(packet);
        }
        public float[] PullFloatArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new float[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToFloat(packet);
            }
            return values;
        }
        public decimal PullDecimal(Packet packet)
        {
            return converter.ToDecimal(packet);
        }
        public decimal[] PullDecimalArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new decimal[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToDecimal(packet);
            }
            return values;
        }
        public double PullDouble(Packet packet)
        {
            return converter.ToDouble(packet);
        }
        public double[] PullDoubleArray(Packet packet)
        {
            var count = PullInt(packet);
            var values = new double[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = converter.ToDouble(packet);
            }
            return values;
        }
    }
}
