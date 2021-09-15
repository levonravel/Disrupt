using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public class FragmentProcess
    {
        private const int fragmentLimit = 512;
        private Queue<Packet> fragments = new Queue<Packet>();
        public Queue<Packet> Awaited = new Queue<Packet>();
        private DisruptClient client;

        public Packet ConstructPacket(Packet packet)
        {
            if(packet.Fragmented == Fragment.Begin)
            {
                fragments.Enqueue(packet);
                return null;
            }
            else if(fragments.Count == 0)
            {
                return packet;
            }
            fragments.Enqueue(packet);
            var constructed = client.Exchange.CreatePacket();
            var offset = fragmentLimit - 3;
            constructed.Payload = new byte[fragments.Count * (offset) + 3];
            constructed.CurrentIndex = 3;
            for(int i = fragments.Count; i > 0; i--)
            {
                var fragPacket = fragments.Dequeue();
                FastCopy(fragPacket.Payload, 3, constructed.Payload, constructed.CurrentIndex, offset);
                constructed.CurrentIndex += offset;
                if (i == 1)
                {
                    FastCopy(fragPacket.Payload, 0, constructed.Payload, 0, 3);
                    constructed.CurrentIndex = 3;
                }
            }
            return constructed;
        }
        public void ShouldFragment(Packet packet)
        {
            if (packet.CurrentIndex <= fragmentLimit)
            {
                Awaited.Enqueue(packet);
                return;
            }
            var needed = (int)Math.Ceiling((double)(packet.CurrentIndex - 3) / (fragmentLimit - 3));
            var totalPayload = packet.CurrentIndex;
            var endAmount = totalPayload - ((needed - 1) * (fragmentLimit - 3));
            for (int i = 0; i < needed; i++)
            {
                var lastFrag = i + 1 == needed;
                var frag = client.Exchange.CreatePacket();
                frag.Protocol = packet.Protocol;
                frag.Flag = packet.Flag;
                var copyStartIndex = i * (fragmentLimit - 3);
                var copyLength = lastFrag ? endAmount : fragmentLimit - 3;
                frag.CurrentIndex = lastFrag ? copyLength : fragmentLimit;
                FastCopy(packet.Payload, i == 0 ? 3 : copyStartIndex + 3, frag.Payload, 3, copyLength);
                frag.Fragmented = lastFrag ? Fragment.End : Fragment.Begin;
                Awaited.Enqueue(frag);
            }
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
