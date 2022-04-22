/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      None
 *Class Information
 *      An internal bridge to other classes that can be consumed
 */
using System;
using System.Net;

namespace RavelNet
{
    public class RavelNetEvents : PacketPool
    {
        public Action<Packet, Peer> OnReceive;
        public Action<Packet, Protocol, Peer> OnSend;
        public Action<string, int> OnConnectStringIp;
        public Action<Peer> OnConnectPeer;
        public Action<Peer> OnConnectionReceived;
        public Action<EndPoint> OnDisconnect;

        public void Receive(Packet packet, Peer peer)
        {
            if (packet == null) return;
            OnReceive?.Invoke(packet, peer);
            PutPacket(packet);
        }
        public void Send(Packet packet, Protocol protocol, Peer peer)
        {
            OnSend?.Invoke(packet, protocol, peer);
            if (protocol == Protocol.Sequenced) PutPacket(packet);
        }
        public void Connect(string address, int port)
        {
            OnConnectStringIp?.Invoke(address, port);
        }
        public void Connect(Peer peer)
        {
            OnConnectPeer?.Invoke(peer);
        }
        public void InboundConnection(Peer peer)
        {
            peer.IsConnected = true;
            OnConnectionReceived?.Invoke(peer);
        }
        public void Disconnect(EndPoint address)
        {
            OnDisconnect?.Invoke(address);
        }
    }
}
