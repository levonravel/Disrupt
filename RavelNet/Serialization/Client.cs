/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      RavelNetEvents
 *      CommunicationController
 *
 *Class Information
 *      Creates a socket that pulls data from the wire and uses the CommunicationController to publish packets
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RavelNet
{
    public class Client : RavelNetEvents
    {
        public IPEndPoint Address;
        private Socket socket;
        private CommunicationController communicationController;
        private readonly SequencedController sequencedLayer;
        private readonly ReliableController reliableLayer;
        private readonly PeerCollection peerCollection;

        public Client(string applicationName, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Address = new IPEndPoint(IPAddress.Any, port);
            socket.ReceiveBufferSize = int.MaxValue;
            socket.SendBufferSize = int.MaxValue;
            socket.DontFragment = true;
            socket.EnableBroadcast = true;
            socket.Bind(Address);
            communicationController = new CommunicationController(socket, this);
            var listenerThread = new Thread(Listen)
            {
                IsBackground = true
            };
            listenerThread.Start();
        }
        private void Listen()
        {
            while (socket != null)
            {
                try
                {
                    var packet = new Packet();
                    packet.Length = socket.ReceiveFrom(packet.Payload, 0, 512, SocketFlags.None, ref packet.Address);
                    PreprocessPacket(packet);
                }
                catch (Exception e)
                {
                    throw new Exception(e.ToString() + " line 55 in method Listener, class Listener.cs");
                }
            };
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
        public void Poll()
        {
            foreach(var peer in peerCollection.GetPeers)
            {
                Packet reliable = peer.Dequeue(Protocol.Reliable, TransportLayer.Inbound);
                Packet sequenced = peer.Dequeue(Protocol.Sequenced, TransportLayer.Inbound);
                if (reliable != null)
                {
                    var result = reliableLayer.TryReceive(peer);
                    if (result == null) return;
                    communicationController.Acknowledge(peer);
                    Receive(result, peer);
                }
                if (sequenced != null)
                {
                    var result = sequencedLayer.TryReceive(sequenced, peer);
                    Receive(result, peer);
                }
            }
        }
        private void Events_OnDisconnect(EndPoint address)
        {
            peerCollection.Remove(address);
        }
        private void Events_OnOutboundConnection(string address, int port)
        {
            var destination = new IPEndPoint(IPAddress.Parse(address), port));
            var peer = peerCollection.Add(destination);
            var packet = new Packet();
            packet.Flag = Flags.Con;
            peer.Enqueue(packet, Protocol.Reliable, TransportLayer.Outbound);
        }
        private void TryAddPeer(EndPoint address)
        {
            peerCollection.Add(address);
        }
        public void Dispose()
        {
            socket.Dispose();
            socket = null;
        }
    }
}
