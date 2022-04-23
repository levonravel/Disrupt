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
namespace RavelNet
{
    public class ReliableController
    {
        private readonly FragmentationController fragmentationController = new FragmentationController();
        private readonly CommunicationController communicationController;
    
        public ReliableController(CommunicationController communicationController)
        {
            this.communicationController = communicationController;
        }

        /**************************************
         ****         Receive Logic        ****
         **************************************/

        public void TryReceive(Peer peer, Client client, Packet packet)
        {
            if(packet.Id == 0 && peer.ReceivedBitfield == -1)
            {
                peer.ReceivedBitfield = 0;
            }
            if ((peer.ReceivedBitfield & (1 << packet.Id)) == 1) return;    
            peer.ReceivedBitfield |= (1 << packet.Id);
            if (packet.Flag == Flags.Con)
            {
                client.InboundConnection(peer);
                return;
            }  
            fragmentationController.ConstructPacket(packet, peer, client);
        }

        /**************************************
         ****          Send Logic          ****
         **************************************/

        public bool TrySend(Peer peer)
        {
            if(peer.ResetSentId)
            {
                peer.SentReliableId = 0;
                peer.ResetSentId = false;
            }
            for(int i = peer.SentReliableId; i < 32; i++)
            {
                if (peer.AwaitingPackets == 0) return false;
                peer.SentPackets[i] = peer.Dequeue();                
                peer.SentPackets[i].Id = (byte)(peer.SentReliableId % 32);
                communicationController.Send(peer.SentPackets[i]);
                peer.SentReliableId++;
            }
            return true;
        }
    }
}
