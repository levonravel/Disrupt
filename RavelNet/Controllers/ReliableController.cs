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
using System.Collections.Generic;

namespace RavelNet
{
    public class ReliableController
    {
        private readonly FragmentationController fragmentationController = new FragmentationController();

        private bool IsAcked(Packet packet, Peer peer)
        {
            return ((1 << packet.Id) & peer.SendBits) != 0;
        }

        /**************************************
         ****         Receive Logic        ****
         **************************************/

        public Packet TryReceive(Peer peer)
        {
            var packet = peer.Dequeue(Protocol.Reliable, CollectionType.Inbound);
            if (packet == null) return null;
            var expectedFlag = peer.ReceivedFlags[packet.Id];
            if (packet.ReliableBufferFlag != expectedFlag)
            {
                return null;
            }
            peer.ReceivedBits += 1 << packet.Id;
            peer.ReceiveBuffer[packet.Id] = packet;
            return packet;
        }
        public void UpdateReceiverLowerBound(Peer peer)
        {
            var oldLowerBound = peer.ReceivedLowerBound;
            // If buffer is -1, all pending packets have been received
            var allReceived = peer.ReceivedBits == -1;
            // If ALL received, there is no new lower bound
            if (!allReceived)
                peer.ReceivedLowerBound = FindLowerBound(peer.ReceivedLowerBound, peer.ReceivedBits);

            if (allReceived)
            {
                peer.ReceivedBits = 0;
                for (int i = 0; i < 400; i++)
                {
                    var index = (peer.ReceivedLowerBound + i) % 31;

                    var packet = peer.ReceiveBuffer[index];
                    if (ReferenceEquals(packet, null)) continue;
                    peer.ReceiveBuffer[index] = null;
                    peer.ReceivedFlags[index] = !peer.ReceivedFlags[index];
                    fragmentationController.ConstructPacket(packet, peer);
                }
            }
            // If Lower bound hasn't changed, there's no need to free up spaces in the buffer
            else if (oldLowerBound != peer.ReceivedLowerBound)
            {
                // Updates buffer to fit new lower bound
                for (int i = oldLowerBound; i != peer.ReceivedLowerBound; i = (i + 1) % 31)
                {
                    peer.ReceivedBits -= 1 << i;
                    var packet = peer.ReceiveBuffer[i];
                    if (ReferenceEquals(packet, null)) continue;
                    peer.ReceiveBuffer[i] = null;
                    peer.ReceivedFlags[i] = !peer.ReceivedFlags[i];
                    fragmentationController.ConstructPacket(packet, peer);
                }
            }
        }

        /**************************************
         ****          Send Logic          ****
         **************************************/

        public List<Packet> TrySend(Peer peer)
        {
            UpdateBuffer(peer);
            List<Packet> packets = new List<Packet>();
            for (int i = 0; i < 32; i++)
            {
                var index = (peer.SendLowerBound + i) % 31;
                if (peer.SendBuffer[index] == null) continue;
                var packet = peer.SendBuffer[index];
                if (packet == null || IsAcked(packet, peer)) continue;
                packets.Add(packet);
            }
            return packets;
        }
        void UpdateBuffer(Peer peer)
        {
            for (int i = 0; i < 32; i++)
            {                
                var index = (peer.SendLowerBound + i) % 31;
                if (peer.SendBuffer[index] != null) continue;
                var packet = peer.Dequeue(Protocol.Reliable, CollectionType.Outbound);
                if (packet == null) break;
                packet.Id = (byte)index;
                packet.ReliableBufferFlag = peer.SendFlags[index];
                peer.SendBuffer[index] = packet;
            }
        }

        private static int FindLowerBound(int currentLowerBound, int buffer)
        {
            // Loops 32 times, starting from current _lowerBound (IMPORTANT)
            for (int i = 0; i < 32; i++)
            {
                var index = (currentLowerBound + i) % 31;

                if ((buffer & (1 << index)) != 0) continue;
                return (byte)index;
            }
            return currentLowerBound;
        }
    }
}
