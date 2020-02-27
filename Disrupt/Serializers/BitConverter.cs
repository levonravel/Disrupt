using System;

namespace RavelTek.Disrupt.Serializers
{
    public class BitConverter
    {
        ////////////////////////////////////////////////////////////////
        //////////                 TO BYTES                 ////////////
        ////////////////////////////////////////////////////////////////
        public unsafe void GetBytes(Packet packet, bool value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *b = *(byte*)&value;
            packet.CurrentIndex += 1;
        }
        public unsafe void GetBytes(Packet packet, byte value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *b = *&value;
            packet.CurrentIndex += 1;
        }
        public unsafe void GetBytes(Packet packet, sbyte value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *((sbyte*)b) = *&value;
            packet.CurrentIndex += 1;
        }
        public unsafe void GetBytes(Packet packet, short value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *((short*)b) = *&value;
            packet.CurrentIndex += 2;
        }
        public unsafe void GetBytes(Packet packet, ushort value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *((ushort*)b) = *&value;
            packet.CurrentIndex += 2;
        }
        public unsafe void GetBytes(Packet packet, int value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *((int*)b) = *&value;
            packet.CurrentIndex += 4;
        }
        public unsafe void GetBytes(Packet packet, uint value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *((uint*)b) = *&value;
            packet.CurrentIndex += 4;            
        }
        public unsafe void GetBytes(Packet packet, float value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *((float*)b) = *&value;
            packet.CurrentIndex += 4;
        }
        public unsafe void GetBytes(Packet packet, double value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *((double*)b) = *&value;
            packet.CurrentIndex += 8;
        }
        public unsafe void GetBytes(Packet packet, long value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *((long*)b) = *&value;
            packet.CurrentIndex += 8;
        }
        public unsafe void GetBytes(Packet packet, ulong value)
        {
            fixed (byte* b = &packet.PayLoad[packet.CurrentIndex])
                *((ulong*)b) = *&value;
            packet.CurrentIndex += 8;
        }
        ////////////////////////////////////////////////////////////////
        //////////                FROM BYTES                ////////////
        ////////////////////////////////////////////////////////////////
        ///[X,X, 1,2,3,4,5,6,7]
        ///      2 3 4 5 6 7 8
        public unsafe byte ToByte(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 1;
                return *((byte*)(int*)ptr);
            }
        }
        public unsafe sbyte ToSByte(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 1;
                return *((sbyte*)(int*)ptr);
            }
        }
        public unsafe short ToShort(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 2;
                return *((short*)(int*)ptr);
            }
        }
        public unsafe ushort ToUShort(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 2;
                return *((ushort*)(int*)ptr);
            }
        }
        public unsafe int ToInt(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 4;
                return *((int*)ptr);
            }
        }
        public unsafe uint ToUInt(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 4;
                return *((uint*)(int*)ptr);
            }
        }
        public unsafe float ToFloat(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 4;
                return *((float*)(int*)ptr);
            }
        }
        public unsafe decimal ToDecimal(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 16;
                return *((decimal*)(int*)ptr);
            }
        }
        public unsafe double ToDouble(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 8;
                return *((double*)(int*)ptr);
            }
        }
        public unsafe long ToLong(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 8;
                return *((long*)(int*)ptr);
            }
        }
        public unsafe ulong ToULong(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 8;
                return *((ulong*)(int*)ptr);
            }
        }
        public unsafe char ToChar(Packet packet)
        {
            fixed (byte* ptr = &packet.PayLoad[packet.CurrentIndex])
            {
                packet.CurrentIndex += 2;
                return *((char*)(int*)ptr);
            }
        }
        public unsafe string ToString(Packet packet)
        {
            var length = ToInt(packet);
            var eString = System.Text.Encoding.ASCII.GetString(packet.PayLoad, packet.CurrentIndex, length);
            packet.CurrentIndex += length;
            return eString;
        }
    }
}