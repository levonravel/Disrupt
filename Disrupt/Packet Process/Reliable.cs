using System;
using System.Threading;
using System.Threading.Tasks;

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
            SendLoop();
        }
        public override void ReceiveReady(Packet packet)
        {
            base.ReceiveReady(packet);
            if(packet.Flag == Flags.PacketUpdate)
            {
                Client.Exchange.Peers[packet.Address].Confirmation(packet);
                return;
            }
            Client.Exchange.Peers[packet.Address].Receive(packet);
        }
        public override void SendReady(Packet packet)
        {
            base.SendReady(packet);
            currentPeer = Client.Exchange.Peers[packet.Address];
            currentPeer.EnqueuePacket(packet);
        }
        private void SendLoop()
        {
            Task.Run(async () =>
            {
                while (Client.Socket != null)
                {
                    try
                    {
                        foreach (var i in Client.Exchange.Peers.Values)
                        {
                            i.TrySend();
                        }
                        await Task.Delay(1);
                    }catch (Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
                }
            });
        }
    }
}
