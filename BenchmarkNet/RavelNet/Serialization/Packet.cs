/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      None
 *Class Information
 *      This container stores data dealing with serialization, it is the main source when sending or recieving through buffers
 */
using System;
using System.Net;

namespace RavelNet
{
    public class Packet
    {
        public EndPoint Address = new IPEndPoint(IPAddress.Any, 0);
        public byte[] Payload = new byte[512];
        public int Length;
        public int CurrentIndex = 3;

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
                    throw new Exception("line 43 method ReliableBufferFlag being read from non reliable packet, class Packet.cs");
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
            Flag = Flags.None;
            CurrentIndex = 3;
            Payload[0] = 0;
            Payload[1] = 0;
            Payload[2] = 0;
            Id = 0;
        }
        public Packet Clone(Packet hardcopy)
        {
            if (hardcopy.Payload.Length < Payload.Length)
            {
                Array.Resize(ref hardcopy.Payload, Payload.Length);
            }
            FastCopy(Payload, 0, hardcopy.Payload, 0, Payload.Length);
            hardcopy.CurrentIndex = CurrentIndex;
            return hardcopy;
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
