using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavelNet
{
    public class Sequenced
    {
        private byte seqIndex;
        private byte seqRecv;
        public Packet PrepSequenced(Packet packet)
        {
            if (packet.Flag == Flags.None) packet.Flag = Flags.Dat;
            seqIndex++;
            packet.Id = seqIndex;
            return packet;
        }
        public Packet ReceiveSequenced(Packet packet)
        {
            if (LatestPacket(seqRecv, packet.Id))
            {
                seqRecv = packet.Id;
                return packet;
            }
            return null;
        }
        private bool LatestPacket(int s2, int s1)
        {
            return ((s1 > s2) && (s1 - s2 <= 128)) || ((s1 < s2) && (s2 - s1 > 128));
        }
    }
}
