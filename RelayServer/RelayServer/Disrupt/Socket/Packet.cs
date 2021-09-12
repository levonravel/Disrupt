using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace RavelTek.Disrupt
{
    [System.Serializable]
    public class Packet
    {
        public const int HeaderSize = 3;
        public static int count = 0;
        public static Client client;
        public EndPoint Address = new IPEndPoint(IPAddress.Any, 0);
        public bool SingleSend;
        public byte[] Payload = new byte[256];
        public int Length;
        public int CurrentIndex = HeaderSize;
        public Client Client;
        public string lastUsage;
        public int packetCount;

        public Fragment Fragmented
        {
            get
            {
                return (Payload[0] & (1 << 7)) != 0 ? Fragment.Begin : Fragment.End;
            }
            set
            {
                Payload[0] |= (byte)(value == Fragment.Begin ? 128 : 0);
            }
        }
        public byte Id
        {
            get
            {
                return Payload[1];
            }
            set
            {
                Payload[1] = value;
            }
        }
        public bool ReliableBufferFlag
        {
            get
            {
                if (Protocol != Protocol.Reliable)
                    throw new Exception("Reliable Buffer Flag being read from non reliable packet");
                return Payload[2] == 1;
            }
            set
            {
                Payload[2] = (value ? (byte)1 : (byte)0);
            }
        }
        public Protocol Protocol
        {
            get
            {
                return (Payload[0] & (1 << 6)) != 0 ? Protocol.Reliable : Protocol.Sequenced;
            }
            set
            {
                Payload[0] |= (byte)(value == Protocol.Reliable ? 64 : 0);
            }
        }
        public Flags Flag
        {
            get
            {
                int flag = 0;
                for (byte i = 0; i < 6; i++)
                {
                    if ((Payload[0] & (1 << i)) != 0)
                    {
                        flag += (byte)(1 << i);
                    }
                }
                return (Flags)flag;
            }
            set
            {
                Payload[0] |= (byte)value;
            }
        }
        public void Reset()
        {
            if(Payload.Length > 256)
            {
                Array.Resize(ref Payload, 256);
            }
            Payload[0] = 0;
            Payload[1] = 0;
            Payload[2] = 0;
            CurrentIndex = HeaderSize;
            SingleSend = false;
        }
        public Packet Clone()
        {
            var packet = client.CreatePacket();
            FastCopy(Payload, 0, packet.Payload, 0, Payload.Length);
            packet.CurrentIndex = CurrentIndex;
            return packet;
        }
        private unsafe void FastCopy(byte[] src, int src_offset, byte[] dst, int dst_offset, int length)
        {
            if (length > 0)
            {
                fixed (byte* srcPtr = &src[src_offset])
                {
                    fixed (byte* dstPtr = &dst[dst_offset])
                    {
                        Buffer.MemoryCopy(srcPtr, dstPtr, length, length);
                    }
                }
            }
        }
    }
}