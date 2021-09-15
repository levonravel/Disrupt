namespace RavelTek.Disrupt
{
    public class Reliable : OperationLoop
    {
        private PeerContainer currentPeer;

        public Reliable(DisruptClient client)
        {
            Initiate(client);
        }

        public override void Initiate(DisruptClient client)
        {
            base.Initiate(client);
            client.Events.OnPacketUpdate += Events_OnPacketUpdate;
        }
        private void Events_OnPacketUpdate(Packet packet)
        {
            Client.Exchange.Peers[packet.Address].PacketUpdate(packet);
        }
        public override void ReceiveReady(Packet packet)
        {
            base.ReceiveReady(packet);
            var constructed = Client.Exchange.Peers[packet.Address].Receive(packet);
            if (constructed == null) return;
            Client.Events.RaiseEventData(packet);
        }
        public override void SendReady(Packet packet)
        {
            base.SendReady(packet);
            currentPeer = Client.Exchange.Peers[packet.Address];
            currentPeer.Awaited.Enqueue(packet);
            currentPeer.SendLoop();            
        }
    }
}
