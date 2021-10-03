using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RavelNet
{
    public class Events
    {
        public delegate void InternalReceive(Packet packet);
        public delegate void Receive(Packet packet);
        public delegate void Connected(Packet packet);
        public delegate void InternalConnection(Packet packet);
        public delegate void UnPlug(Packet packet);
        public delegate void Ping(Packet packet);
        public delegate void InternalSend(Packet packet);
        public event InternalReceive OnInternalReceive;
        public event Receive OnReceive;
        public event Connected OnConnect;
        public event InternalConnection OnInternalConnect;
        public event UnPlug OnDisconnect;
        public event Ping OnPing;
        public event InternalSend OnInternalSend;
        private static readonly object queue = new object();
        private readonly Queue<Packet> awaited = new Queue<Packet>();

        public void AwaitPacket(Packet packet)
        {
            lock (queue) awaited.Enqueue(packet);
        }
        public void RaiseInternalReceive(Packet packet)
        {
            if (OnInternalReceive == null) return;
            OnInternalReceive(packet);
        }
        public void RaiseReceive(Packet packet)
        {
            if (OnReceive == null) return;
            OnReceive(packet);
        }
        public void RaiseConnect(Packet packet)
        {
            if (OnConnect == null) return;
            OnConnect(packet);
        }
        public void RaiseDisconnect(Packet packet)
        {
            if (OnDisconnect == null) return;
            OnDisconnect(packet);
        }
        public void RaisePing(Packet packet)
        {
            if (OnPing == null) return;
            OnPing(packet);
        }
        public void RaiseInternalConnection(Packet packet)
        {
            if (OnInternalConnect == null) return;
            OnInternalConnect(packet);
        }
        public void RaiseInternalSend(Packet packet)
        {
            if (OnInternalSend == null) return;
            OnInternalSend(packet);
        }
        public void Poll()
        {
            while (awaited.Count != 0)
            {
                lock (queue)
                {
                    var packet = awaited.Dequeue();
                    switch (packet.Flag)
                    {
                        case Flags.None:
                            break;
                        case Flags.NatReq:
                            break;
                        case Flags.NatIntro:
                            break;
                        case Flags.UPD:
                            RaisePing(packet);
                            break;
                        case Flags.Dat:
                            RaiseReceive(packet);
                            break;
                        case Flags.Dc:
                            RaiseDisconnect(packet);
                            break;
                        case Flags.Con:
                            RaiseInternalConnection(packet);
                            break;
                        case Flags.NatHost:
                            break;
                        case Flags.HostList:
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
