using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RavelNet
{
    public class ProcessingController
    {
        private readonly PeerCollection peerCollection = new PeerCollection();
        private readonly SequencedController sequencedController = new SequencedController();
        private readonly ReliableController reliableController = new ReliableController();
        private readonly CommunicationController communicationController;
        private readonly RavelNetEvents events;
        private readonly Socket socket;

        public ProcessingController(RavelNetEvents events, Socket socket)
        {
            this.events = events;
            this.socket = socket;
            communicationController = new CommunicationController(socket, events);
            events.OnDisconnect += Events_OnDisconnect;
            events.OnOutboundConnection += Events_OnOutboundConnection;
            var outgoingThread = new Thread(OutgoingLoop)
            {
                IsBackground = true
            };
            outgoingThread.Start();
        }

        private void Events_OnDisconnect(EndPoint address)
        {
            peerCollection.Remove(address);
        }
        private void Events_OnOutboundConnection(string address, int port)
        {
            var destination = new IPEndPoint(IPAddress.Parse(address), port);
            var peer = peerCollection.Add(destination);
            var packet = new Packet();
            packet.Flag = Flags.Con;
            packet.Address = destination;
            peer.Enqueue(packet, Protocol.Sequenced, CollectionType.Outbound);
        }


        public void Poll()
        {
            foreach (var peer in peerCollection.GetPeers)
            {
                IterateCollection(peer, Protocol.Reliable, CollectionType.Inbound);
                IterateCollection(peer, Protocol.Sequenced, CollectionType.Inbound);
            }
        }
        private void OutgoingLoop()
        {
            Task.Run(async () =>
            {
                while (socket != null)
                {
                    await Task.Delay(1);
                    try
                    {
                        foreach (var peer in peerCollection.GetPeers)
                        {
                            communicationController.TrySend(peer);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }
        public void PreprocessPacket(Packet packet)
        {
            if (packet.Flag == Flags.Con)
            {
                TryAddPeer(packet.Address);
            }
            Peer peer = peerCollection.GetPeer(packet.Address);
            peer.Enqueue(packet, packet.Protocol, CollectionType.Inbound);
        }
        public void IterateCollection(Peer peer, Protocol protocol, CollectionType layer)
        {
            Packet result = null;
            while (peer.CanIterate(protocol, layer))
            {
                if (layer == CollectionType.Outbound)
                {
                    communicationController.TrySend(peer);
                    break;
                }

                switch (protocol)
                {
                    case Protocol.Reliable:
                        result = reliableController.TryReceive(peer);
                        if (result != null)
                        {
                            //need to acknowledge before clearing lowerbound
                            communicationController.Acknowledge(peer);
                            //update peers receive buffer
                            reliableController.UpdateReceiverLowerBound(peer);
                        }
                        break;
                    case Protocol.Sequenced:
                        result = sequencedController.TryReceive(peer);
                        //this is a UPD packet contains Acked information
                        if (result == null) continue;
                        if (result.Flag == Flags.UPD)
                        {
                            communicationController.Confirmation(result, peer);
                            continue;
                        }
                        break;
                    default:
                        break;
                }
                events.Receive(result, peer);
            }
        }
        private void TryAddPeer(EndPoint address)
        {
            peerCollection.Add(address);
        }
    }
}
