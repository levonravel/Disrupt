using System;

namespace RavelNet
{
    public sealed class FragmentationController 
    {
        private const int fragmentLimit = 511;

        public void ConstructPacket(Packet packet, Peer peer)
        {
            if (packet.Fragmented == Fragment.Begin)
            {
                peer.FragmentPackets.Enqueue(packet);
            }
            else if (peer.FragmentPackets.Count == 0)
            {
                peer.Enqueue(packet, Protocol.Reliable, TransportLayer.Inbound);
            }
            peer.FragmentPackets.Enqueue(packet);
            var constructed = new Packet();
            var offset = fragmentLimit - 3;
            constructed.Payload = new byte[peer.FragmentPackets.Count * (offset) + 3];
            constructed.CurrentIndex = 3;
            for(int i = peer.FragmentPackets.Count; i > 0; i--)
            {
                var fragPacket = peer.FragmentPackets.Dequeue();
                FastCopy(fragPacket.Payload, 3, constructed.Payload, constructed.CurrentIndex, offset);
                constructed.CurrentIndex += offset;
                if (i == 1)
                {
                    FastCopy(fragPacket.Payload, 0, constructed.Payload, 0, 3);
                    constructed.CurrentIndex = 3;
                }
            }
            peer.Enqueue(constructed, Protocol.Reliable, TransportLayer.Inbound);
        }
        public Packet ShouldFragment(Packet packet, Peer peer)
        {
            if (packet.CurrentIndex <= fragmentLimit)
            {
                AssignId(packet, peer);                
                return packet;
            }
            var needed = (int)Math.Ceiling((double)(packet.CurrentIndex - 3) / (fragmentLimit - 3));
            var totalPayload = packet.CurrentIndex;
            var endAmount = totalPayload - ((needed - 1) * (fragmentLimit - 3));
            for (int i = 0; i < needed; i++)
            {
                var lastFrag = i + 1 == needed;
                Packet frag = new Packet
                {
                    Protocol = packet.Protocol,
                    Flag = packet.Flag
                };
                var copyStartIndex = i * (fragmentLimit - 3);
                var copyLength = lastFrag ? endAmount : fragmentLimit - 3;
                frag.CurrentIndex = lastFrag ? copyLength : fragmentLimit;
                FastCopy(packet.Payload, i == 0 ? 3 : copyStartIndex + 3, frag.Payload, 3, copyLength);
                frag.Fragmented = lastFrag ? Fragment.End : Fragment.Begin;
                AssignId(packet, peer);
                peer.Enqueue(frag, Protocol.Reliable, TransportLayer.Outbound);
            }
            return null;
        }
        private void AssignId(Packet packet, Peer peer)
        {
            packet.Id = peer.ReliableOutIndex;
            peer.ReliableOutIndex++;
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
