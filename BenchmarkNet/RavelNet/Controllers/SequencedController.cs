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
using System.Threading;

namespace RavelNet
{
    public class SequencedController
    {
        public Reader reader = new Reader();
        private CommunicationController communicationController;

        public SequencedController(CommunicationController communicationController)
        {
            this.communicationController = communicationController;
        }

        public bool TrySend(Peer peer, Packet packet)
        {
            packet.Id = peer.SequencedOutIndex;
            peer.SequencedOutIndex++;
            communicationController.Send(packet);
            return true;
        }
        public void TryReceive(Peer peer, Client client, Packet packet)
        {
            if (packet.Flag == Flags.UPD)
            {
                Confirmation(packet, peer, client);                
                return;
            }
            if (packet.Id == 0 || LatestPacket(peer.SequencedInIndex, (byte)packet.Id))
            {
                peer.SequencedInIndex = (byte)packet.Id;
                client.Receive(packet, peer);
            }
        }
        private bool LatestPacket(int s2, int s1)
        {
            return ((s1 > s2) && (s1 - s2 <= 128)) || ((s1 < s2) && (s2 - s1 > 128));
        }

        /*************************************************
         *****    Reliable Communication Received    *****
         *****       It's Passed As Sequenced        *****
         *************************************************/

        public void Confirmation(Packet packet, Peer peer, Client client)
        {
            peer.LastSeenPing = DateTime.UtcNow;
            //cycle through the bitfield and remove the packets from the peers SentPackets
            var bitfield = reader.Int(packet);
            for(int i = packet.Id; i > 0; i--)
            {
                if ((bitfield & (1 << i)) == 0 && peer.SentPackets[i] != null)
                {
                    //resend packet
                    communicationController.Send(peer.SentPackets[i]);
                }
                else if(peer.SentPackets[i] != null)
                {
                    client.PutPacket(peer.SentPackets[i]);
                }                
            }
            //check if all bits are set on both bitfield and sentReliable
            if (bitfield == -1 && peer.SentReliableId == 32)
            {
                peer.ResetSentId = true;
            }
            client.PutPacket(packet);
        }
    }
}
