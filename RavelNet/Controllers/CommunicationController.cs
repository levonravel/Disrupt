/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      ReliableController
 *      SequencedController
 *      RavelNetEvents
 *      Packet
 *      Peer
 *
 *Class Information
 *      The main communication between the application and peers over the network
 */
using System;
using System.Net;
using System.Net.Sockets;

namespace RavelNet
{
    public class CommunicationController
    {
        private readonly Writer writer = new Writer();
        private readonly Socket socket;
        private readonly PeerCollection peerCollection;
        private readonly ReliableController reliableController;
        private readonly SequencedController sequencedController;
        private MethodTracker methodTracker = new MethodTracker();

        public CommunicationController(Socket socket, RavelNetEvents events)
        {
            this.socket = socket;
            peerCollection = new PeerCollection();
            reliableController = new ReliableController();
            sequencedController = new SequencedController();
            events.OnOutboundConnection += Events_OnOutboundConnection;
            events.OnSend += Events_OnSend;
            events.OnDisconnect += Events_OnDisconnect;
            events.OnMethodTracked += Events_OnMethodTracked;
            events.OnReceive += Events_OnReceive;
        }

        private void Events_OnReceive(Packet packet, Peer peer)
        {
            methodTracker.ReleasePacket(packet, peer);
        }
        private void Events_OnMethodTracked(params Action<Packet, Peer>[] methods)
        {
            methodTracker.AddMethods(methods);
        }
        private void Events_OnDisconnect(EndPoint address)
        {
            peerCollection.Remove(address);
        }
        private void Events_OnSend(Packet packet, Protocol protocol, Peer peer, string method)
        {
            var methodId = methodTracker.GetMethodId(method);
            writer.Open(packet).PackMethod(methodId);
            peer.Enqueue(packet, protocol, TransportLayer.Outbound);
        }
        private void Events_OnOutboundConnection(string address, int port)
        {
            var destination = new IPEndPoint(IPAddress.Parse(address), port));
            var peer = peerCollection.Add(destination);
            var packet = new Packet();
            packet.Flag = Flags.Con;
            peer.Enqueue(packet, Protocol.Reliable, TransportLayer.Outbound);
        }


        public void Acknowledge(Peer peer)
        {
            var packet = new Packet();
            writer.Open(packet)
                .Add(peer.ReceivedBits);
            packet.Flag = Flags.UPD;
            peer.Enqueue(packet, Protocol.Sequenced, TransportLayer.Outbound);
        }
        public void TrySend(Peer peer, Packet packet = null)
        {
            TrySendReliable(peer);
            TrySendSequenced(peer, packet);
        }
        private void TrySendReliable(Peer peer)
        {            
            Packet packet = reliableController.TrySend(peer);            
            if (packet == null) return;
            Send(packet);

        }
        private void TrySendSequenced(Peer peer, Packet packet)
        {
            packet = sequencedController.TrySend(packet, peer);
            if (packet == null) return;
        }
        private void Send(Packet packet)
        {
            CheckFlag(packet);
            socket.SendTo(packet.Payload, packet.CurrentIndex, SocketFlags.None, packet.Address);
        }
        private void CheckFlag(Packet packet)
        {
            if (packet.Flag == Flags.None)
            {
                packet.Flag = Flags.Dat;
            }
        }
    }
}
