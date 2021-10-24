/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      CommunicationsController
 *      RavelNetEvents
 *Class Information
 *      Analyzes the data from the wire and ensures the most recent packet is processed
 */

using System;

namespace RavelNet
{
    public class SequencedController
    {
        public Packet TrySend(Packet packet, Peer peer)
        {            
            packet.Id = peer.SequencedOutIndex;
            return packet;
        }
        public Packet TryReceive(Peer peer)
        {
            var packet = peer.Dequeue(Protocol.Sequenced, TransportLayer.Inbound);            
            if (packet == null) return null;
            if (packet.Id == 0 || LatestPacket(peer.SequencedInIndex, packet.Id))
            {
                peer.SequencedInIndex = packet.Id;
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
