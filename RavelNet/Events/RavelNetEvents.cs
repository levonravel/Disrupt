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
    public class RavelNetEvents
    {
        public delegate void Incomming(Packet packet, Peer peer);
        public delegate void Sending(Packet packet, Protocol protocol, Peer peer, string receivingMethod);
        public delegate void RequestConnect(string address, int port);
        public delegate void ConnectionReceived(Peer peer);
        public delegate void RequestDisconnect(EndPoint address);
        public delegate void TrackMethod(params Action<Packet, Peer>[] methods);
        public event Incomming OnReceive;
        public event Sending OnSend;
        public event RequestConnect OnOutboundConnection;
        public event ConnectionReceived OnInboundConnection;
        public event RequestDisconnect OnDisconnect;
        public event TrackMethod OnMethodTracked;

        public void Receive(Packet packet, Peer peer)
        {
            if (packet == null) return;
            if(packet.Flag == Flags.Con)
            {
                InboundConnection(peer);
                return;
            }
            OnReceive?.Invoke(packet, peer);
        }
        public void Send(Packet packet, Protocol protocol, Peer peer, string receivingMethod)
        {
            OnSend?.Invoke(packet, protocol, peer, receivingMethod);
        }
        public void Connect(string address, int port)
        {
            OnOutboundConnection?.Invoke(address, port);
        }
        public void InboundConnection(Peer peer)
        {
            OnInboundConnection?.Invoke(peer);
        }
        public void Disconnect(EndPoint address)
        {
            OnDisconnect?.Invoke(address);
        }
        public void TrackMethods(params Action<Packet, Peer>[] methods)
        {
            OnMethodTracked?.Invoke(methods);
        }
    }
}
