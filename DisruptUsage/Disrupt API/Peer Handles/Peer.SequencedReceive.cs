using System;

namespace RavelTek.Disrupt
{
    public partial class Peer
    {
        private byte recvSeq;
        
        public void ProcessSequenced(Packet packet)
        {            
            if (LatestPacket(recvSeq, packet.Id))
            {
                recvSeq = packet.Id;
                client.RaiseEventData(packet);
            }
            else
            {
                client.Recycle(packet);
            }
        }
        private bool LatestPacket(int s2, int s1)
        {
            return ((s1 > s2) && (s1 - s2 <= 128)) || ((s1 < s2) && (s2 - s1 > 128));
        }
    }
}
