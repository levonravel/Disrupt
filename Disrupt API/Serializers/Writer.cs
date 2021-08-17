using Newtonsoft.Json;
using RavelTek.Disrupt.Custom_Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RavelTek.Disrupt.Serializers
{
    public partial class Writer
    {
        public BitConverter converter = new BitConverter();
        private UTF8Encoding encoder = new UTF8Encoding();

        public void CheckPayLoadSize(int needed, Packet packet)
        {
            if (packet.CurrentIndex + needed >= packet.Payload.Length)
                Array.Resize(ref packet.Payload, (packet.Payload.Length + needed) + 1);
        }        
        public void Push(bool value, Packet packet)
        {
            CheckPayLoadSize(1, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(bool[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length, packet);
            converter.GetBytes(packet, value.Length);
            for(int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(sbyte value, Packet packet)
        {
            CheckPayLoadSize(1, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(sbyte[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length, packet);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(byte value, Packet packet)
        {
            CheckPayLoadSize(1, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(byte[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length, packet);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(float value, Packet packet)
        {
            CheckPayLoadSize(4, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(float[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length * 4, packet);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(double value, Packet packet)
        {
            CheckPayLoadSize(8, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(double[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length * 8, packet);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(uint value, Packet packet)
        {
            CheckPayLoadSize(4, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(uint[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length * 4, packet);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(ushort value, Packet packet)
        {
            CheckPayLoadSize(2, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(ushort[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length * 2, packet);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(ulong value, Packet packet)
        {
            CheckPayLoadSize(8, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(ulong[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length * 8, packet);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(int value, Packet packet)
        {
            CheckPayLoadSize(4, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(int[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length * 4, packet);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(long value, Packet packet)
        {
            CheckPayLoadSize(8, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(long[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length * 8, packet);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(short value, Packet packet)
        {
            CheckPayLoadSize(2, packet);
            converter.GetBytes(packet, value);
        }
        public void Push(short[] value, Packet packet)
        {
            CheckPayLoadSize(value.Length * 2, packet);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
        }
        public void Push(string value, Packet packet)
        {
            var data = encoder.GetBytes(value);
            converter.GetBytes(packet, data.Length);
            CheckPayLoadSize(data.Length, packet);
            foreach(var i in data)
            {
                converter.GetBytes(packet, i);
            }
        }
        public void Push(string[] value, Packet packet)
        {
            converter.GetBytes(packet, value.Length);
            foreach(var a in value)
            {
                var byteA = encoder.GetBytes(a);
                CheckPayLoadSize(byteA.Length, packet);
                converter.GetBytes(packet, byteA.Length);
                foreach(var i in byteA)
                {
                    CheckPayLoadSize(1, packet);
                    converter.GetBytes(packet, i);
                }
            }
        }
        public void Push<T>(T value, Packet packet)
        {
            ObjectToBytePayLoad(value, packet);
        }
        private void ObjectToBytePayLoad<T>(T obj, Packet packet)
        {
            try
            {
                var clear = JsonConvert.SerializeObject(obj, JsonSettings.Instance.Settings);
                Push(clear, packet);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} Cannot Serialize {obj.GetType()}");
            }
        }
    }
}
