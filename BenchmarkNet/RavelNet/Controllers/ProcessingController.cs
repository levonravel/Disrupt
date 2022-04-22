using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RavelNet
{
    public class ProcessingController
    {
        private Queue<Packet> inbound = new Queue<Packet>();
        private static object inCollection = new object();
        private readonly PeerCollection peerCollection = new PeerCollection();
        private readonly CommunicationController communicationController;
        private Client client;
        private readonly Socket socket;

        public ProcessingController(Client client, Socket socket)
        {
            this.socket = socket;
            this.client = client;
            communicationController = new CommunicationController(socket, client);
            client.OnDisconnect += Events_OnDisconnect;
            client.OnConnectStringIp += Events_OnOutboundConnection;
            client.OnConnectPeer += Events_OnOutboundConnectionPeer;
            client.OnSend += Events_OnSend;
            var outgoing = new Thread(OutgoingLoop);
            outgoing.IsBackground = true;
            outgoing.Start();
        }
        private void Events_OnDisconnect(EndPoint address)
        {
            peerCollection.Remove(address);
        }
        private void Events_OnOutboundConnection(string address, int port)
        {
            var destination = new IPEndPoint(IPAddress.Parse(address), port);
            var peer = peerCollection.Add(destination);
            var packet = client.GetPacket();
            packet.Flag = Flags.Con;
            packet.Protocol = Protocol.Reliable;
            packet.Address = destination;
            peer.Enqueue(packet);
        }
        private void Events_OnOutboundConnectionPeer(Peer peer)
        {
            var packet = client.GetPacket();
            packet.Flag = Flags.Con;
            packet.Protocol = Protocol.Reliable;
            packet.Address = peer.Address;
            peer.Enqueue(packet);
        }
        private void Events_OnSend(Packet packet, Protocol protocol, Peer peer)
        {
            packet.Protocol = protocol;
            packet.Address = peer.Address;
            if (packet.Protocol == Protocol.Sequenced)
            {
                communicationController.TrySendSequenced(peer, packet);
                return;
            }
            peer.Enqueue(packet);
        }
        public void PreprocessPacket(Packet packet)
        {
            if (packet.Flag == Flags.Con)
            {
                TryAddPeer(packet.Address);
            }
            lock(inCollection)
                inbound.Enqueue(packet);
        }
        public void Poll()
        {
            while (inbound.Count > 0)
            {
                Packet packet = null;
                lock(inCollection)
                    packet = inbound.Dequeue();
                var peer = peerCollection.GetPeer(packet.Address);
                communicationController.TryReceive(peer, packet);
            }
        }
        private void OutgoingLoop()
        {
            Task.Run(async () =>
            {
                while (socket != null)
                {
                    //TODO this needs a delay but awaiting is not the right solution (Quick fix to help cpu)
                    await Task.Delay(10);
                    var peers = peerCollection.GetPeers;
                    foreach(var peer in peers)
                    {
                        if(peer.IsConnected)
                        {
                            communicationController.Ping(peer);
                        }
                        if (!communicationController.TrySendReliable(peer)) break;
                    }
                }
            });
        }
        private void TryAddPeer(EndPoint address)
        {
            peerCollection.Add(address);
        }
    }
}
