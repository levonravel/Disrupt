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
        private Packet packet;

        public void CheckPayLoadSize(int needed)
        {
            if (packet.CurrentIndex + needed >= packet.Payload.Length)
                Array.Resize(ref packet.Payload, (packet.Payload.Length + needed) + 1);
        }   
        public Writer Open(Packet packet)
        {
            this.packet = packet;            
            return this;
        }
        public Writer Add(bool value)
        {
            CheckPayLoadSize(1);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(bool[] value)
        {
            CheckPayLoadSize(value.Length);
            converter.GetBytes(packet, value.Length);
            for(int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(sbyte value)
        {
            CheckPayLoadSize(1);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(sbyte[] value)
        {
            CheckPayLoadSize(value.Length);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(byte value)
        {
            CheckPayLoadSize(1);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(byte[] value)
        {
            CheckPayLoadSize(value.Length);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(float value)
        {
            CheckPayLoadSize(4);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(float[] value)
        {
            CheckPayLoadSize(value.Length * 4);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(double value)
        {
            CheckPayLoadSize(8);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(double[] value)
        {
            CheckPayLoadSize(value.Length * 8);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(uint value)
        {
            CheckPayLoadSize(4);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(uint[] value)
        {
            CheckPayLoadSize(value.Length * 4);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(ushort value)
        {
            CheckPayLoadSize(2);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(ushort[] value)
        {
            CheckPayLoadSize(value.Length * 2);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(ulong value)
        {
            CheckPayLoadSize(8);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(ulong[] value)
        {
            CheckPayLoadSize(value.Length * 8);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(int value)
        {
            CheckPayLoadSize(4);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(int[] value)
        {
            CheckPayLoadSize(value.Length * 4);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(long value)
        {
            CheckPayLoadSize(8);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(long[] value)
        {
            CheckPayLoadSize(value.Length * 8);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(short value)
        {
            CheckPayLoadSize(2);
            converter.GetBytes(packet, value);
            return this;
        }
        public Writer Add(short[] value)
        {
            CheckPayLoadSize(value.Length * 2);
            converter.GetBytes(packet, value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                converter.GetBytes(packet, value[i]);
            }
            return this;
        }
        public Writer Add(string value)
        {            
            var data = encoder.GetBytes(value);
            CheckPayLoadSize(data.Length);
            if(data.Length > 512)
            {
                Console.WriteLine("bIGGER");
            }
            converter.GetBytes(packet, data.Length);            
            foreach(var i in data)
            {
                converter.GetBytes(packet, i);
            }
            return this;
        }
        public Writer Add(string[] value)
        {
            converter.GetBytes(packet, value.Length);
            foreach(var a in value)
            {
                var byteA = encoder.GetBytes(a);
                CheckPayLoadSize(byteA.Length);
                converter.GetBytes(packet, byteA.Length);
                foreach(var i in byteA)
                {
                    CheckPayLoadSize(1);
                    converter.GetBytes(packet, i);
                }
            }
            return this;
        }
        public Writer Add<T>(T value)
        {
            ObjectToBytePayLoad(value);
            return this;
        }
        private void ObjectToBytePayLoad<T>(T obj)
        {
            try
            {
                var clear = JsonConvert.SerializeObject(obj, JsonSettings.Instance.Settings);
                Add(clear);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} Cannot Serialize {obj.GetType()}");
            }
        }
    }
}
