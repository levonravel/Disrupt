using System;
using System.Net;
using System.Collections.Generic;
using RavelTek.Disrupt.Serializers;

namespace RavelTek.Disrupt
{
    public class Events
    {
        public delegate void Connected(Packet packet);
        public delegate void Disconnected(Packet packet);
        public delegate void Message(Packet packet);
        public delegate void NatSuccess(Packet packet);
        public delegate void LanDiscoveryClient(Packet packet);
        public delegate void LanDiscoveryHost(Packet packet);
        public delegate void PacketUpdate(Packet packet);
        public delegate void HostList(Packet packet);
        public delegate void HostSuccess();
        public delegate void AddRequested(Packet packet);
        public delegate void RequestDenied(Packet packet);
        public event Disconnected OnDisconnected;
        public event Message OnIncomingMessage;
        public event LanDiscoveryClient OnDiscoveryClient;
        public event LanDiscoveryHost OnDiscoveryHost;
        public event PacketUpdate OnPacketUpdate;
        public event HostList OnHostList;
        public event HostSuccess OnHostSuccess;
        public event AddRequested OnAddRequest;
        public event RequestDenied OnDeniedRequest;
        private DisruptClient client;
        private static readonly Queue<Action> EventCollection = new Queue<Action>();
        private static readonly object it = new object();
        private readonly Reader reader = new Reader();

        public Events(DisruptClient client)
        {
            this.client = client;
        }
        public void RaiseEventPacketUpdate(Packet packet)
        {
            if (OnPacketUpdate == null) return;
            lock (it) EventCollection.Enqueue(() =>
             {
                 OnPacketUpdate(packet);
                 client.Exchange.RecyclePacket(packet);
             });
        }
        public void RaiseEventDisconnect(Packet packet)
        {
            if (OnDisconnected == null) return;
            lock (it) EventCollection.Enqueue(() => 
            { 
                OnDisconnected(packet);
                client.Exchange.RecyclePacket(packet);
            });
        }
        public void RaiseEventData(Packet packet)
        {
            if (OnIncomingMessage == null)
            {
                client.Exchange.RecyclePacket(packet);
                return;
            }
            lock (it) EventCollection.Enqueue(() =>
            {
                OnIncomingMessage(packet);
                client.Exchange.RecyclePacket(packet);
            });
        }
        public void RaiseEventDiscovery(Packet packet)
        {
            if(client.IsHost)
            {
                if (OnDiscoveryHost == null)
                {
                    client.Exchange.RecyclePacket(packet);
                    return;
                }
                lock (it) EventCollection.Enqueue(() => 
                { 
                    OnDiscoveryHost(packet);
                    client.Exchange.RecyclePacket(packet);
                });
            } else {
                if (OnDiscoveryClient == null)
                {
                    client.Exchange.RecyclePacket(packet);
                    return;
                }
                lock (it) EventCollection.Enqueue(() => 
                { 
                    OnDiscoveryClient(packet);
                    client.Exchange.RecyclePacket(packet);
                });
            }
        }
        public void RaiseEventHostList(Packet packet)
        {
            if (OnHostList == null)
            {
                client.Exchange.RecyclePacket(packet);
                return;
            }            
            lock (it) EventCollection.Enqueue(() => 
            { 
                OnHostList(packet);
                client.Exchange.RecyclePacket(packet);
            });
        }
        public void RaiseEventAddRequest(Packet packet)
        {
            if(OnAddRequest == null)
            {
                client.Exchange.RecyclePacket(packet);
                return;
            }
            lock (it) EventCollection.Enqueue(() =>
             {
                 OnAddRequest(packet);
                 client.Exchange.RecyclePacket(packet);
             });
        }
        public void RaiseEventDeniedRequest(Packet packet)
        {
            if(OnDeniedRequest == null)
            {
                //Remove Peer
                client.Exchange.RecyclePacket(packet);
                return;
            }
            lock (it) EventCollection.Enqueue(() =>
             {
                 //Remove Peer
                 OnDeniedRequest(packet);
                 client.Exchange.RecyclePacket(packet);
             });
        }
        public void Poll()
        {
            for(int i = 0; i < EventCollection.Count; i++)
            {
                lock (it) EventCollection.Dequeue()();
            }
        }
    }
}
