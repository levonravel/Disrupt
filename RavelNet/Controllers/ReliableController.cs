/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      CommunicationsController
 *      RavelNetEvents
 *      Fragmentation
 *      Writer
 *Class Information
 *      Analyzes sent data through or over the wire, if its above the maxlimit bytes the controller fragments or constucts given packet
 *      Also ensures a window aspect and requires a signature to process reliably
 */
using System;

namespace RavelNet
{
    public class ReliableController
    {
        private readonly FragmentationController fragmentationController;

        public Packet TryReceive(Peer peer)
        {
            var id = peer.Peek(Protocol.Reliable, TransportLayer.Inbound);
            if (id == -1 || peer.ReceiveBuffer[id] != null) return null;
            Packet packet = peer.Dequeue(Protocol.Reliable, TransportLayer.Inbound);
            peer.ReceiveBuffer[packet.Id] = packet;
            peer.ReceivedBits |= 1 << packet.Id;
            peer.ReceivedFlags[packet.Id] = !peer.ReceivedFlags[packet.Id];
            return packet;
        }
        public Packet TrySend(Peer peer)
        {            
            var id = peer.Peek(Protocol.Reliable, TransportLayer.Outbound);
            if (id == -1 || peer.SendBuffer[id] != null) return null;
            Packet packet = peer.Dequeue(Protocol.Reliable, TransportLayer.Outbound);
            var fragPacket = fragmentationController.ShouldFragment(packet, peer);
            if (fragPacket == null) return null;
            peer.SendBuffer[fragPacket.Id] = packet;
            return fragPacket;
        }
    }
}
