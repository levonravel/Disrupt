/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      CommunicationsController
 *      SequencedController
 *      ReliableController
 *      PeerCollection
 *      Packet
 *      Socket
 *      Peer
 *
 *Class Information
 *      This class processes inbound and outbound packets
 */
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RavelNet
{
    public class ProcessPacketController
    {
        private CommunicationController communicationController;
        private readonly SequencedController sequencedLayer;
        private readonly ReliableController reliableLayer;
        private readonly PeerCollection peerCollection;
        private readonly Socket socket;

        public ProcessPacketController(Socket socket, CommunicationController communicationController, PeerCollection peerTracker)
        {
            this.socket = socket;
            this.communicationController = communicationController;
            this.peerCollection = peerTracker;
            Thread inbound = new Thread(ProcessInbound);
            inbound.IsBackground = true;
            inbound.Start();
            Thread outbound = new Thread(ProcessInbound);
            outbound.IsBackground = true;
            outbound.Start();
        }
        public void PreprocessPacket(Packet packet)
        {
            if (packet.Flag == Flags.Con)
            {
                TryAddPeer(packet.Address);
            }
            Peer peer = peerCollection.GetPeer(packet.Address);
            peer.Enqueue(packet, packet.Protocol, TransportLayer.Inbound);
        }
        public void ProcessOutbound()
        {
            Task.Run(async () =>
            {
                while(socket != null)
                {
                    await Task.Delay(1);
                    Parallel.ForEach(peerCollection.GetPeers, peer =>
                    {
                        communicationController.TrySend(peer);
                    });
                }
            });
        }
        private void ProcessInbound()
        {
            Task.Run(async () =>
            {
                while (socket != null)
                {
                    await Task.Delay(1);
                    Parallel.ForEach(peerCollection.GetPeers, peer =>
                    {
                        Packet reliable = peer.Dequeue(Protocol.Reliable, TransportLayer.Inbound);
                        Packet sequenced = peer.Dequeue(Protocol.Sequenced, TransportLayer.Inbound);
                        if (reliable != null)
                        {
                            Packet readyPacket = reliableLayer.TryReceive(peer);
                            if (readyPacket == null) return;
                            communicationController.Acknowledge(peer);
                        }
                        if(sequenced != null)
                        {
                            sequencedLayer.TryReceive(sequenced, peer);
                        }
                    });
                }
            });
        }
        private void TryAddPeer(EndPoint address)
        {
            peerCollection.Add(address);
        }
    }
}
