using RavelTek.Disrupt.Serializers;
using System;
using System.Collections.Generic;
using System.Net;

namespace RavelTek.Disrupt
{
    public partial class Client
    {
        public delegate void Connected(Peer peer);
        public delegate void Disconnected(EndPoint endPoint);
        public delegate void Message(Packet packet);
        public delegate void NatSuccess(Packet packet);
        public delegate void Discovery(NatInfo natInfo);
        public delegate void HostList(NatInfo[] hosts);
        public delegate void HostSuccess();
        public event Connected OnConnected;
        public event Disconnected OnDisconnected;
        public event Message OnIncomingMessage;
        public event Discovery OnDiscovery;
        public event HostList OnHostList;
        public event HostSuccess OnHostSuccess;
        private static readonly Queue<Action> EventCollection = new Queue<Action>();
        private readonly Reader reader = new Reader();

        public void RaiseEventHostSuccess(Packet packet)
        {
            Recycle(packet);
            if (OnHostSuccess == null) return;
            OnHostSuccess();
        }
        public void RaiseEventConnect(Packet packet, Peer peer)
        {
            if (OnConnected == null)
            {
                Recycle(packet);
                return;
            }
            if (packet.Address.Equals(RelayAddress))
            {
                if (OnHostSuccess == null) return;
                OnHostSuccess();
                Recycle(packet);
                return;
            }
            Recycle(packet);
            lock (EventCollection) EventCollection.Enqueue(() => { OnConnected( peer);});
        }
        public void RaiseEventDisconnect(EndPoint endPoint)
        {
            if (OnDisconnected == null) return;
            lock (EventCollection) EventCollection.Enqueue(() => { OnDisconnected(endPoint); });
        }
        public void RaiseEventData(Packet packet)
        {
            if (OnIncomingMessage == null)
            {
                Recycle(packet);
                return;
            }
            lock (EventCollection) EventCollection.Enqueue(() => 
            {               
                OnIncomingMessage(packet); 
                Recycle(packet); 
            });
        }
        public void RaiseEventDiscovery(Packet packet)
        {
            if (OnDiscovery == null)
            {
                Recycle(packet);
                return;
            }
            var appId = reader.PullString(packet);
            NatInfo natInfo;
            try
            {
                natInfo = reader.PullObject<NatInfo>(packet);
            }
            catch
            {
                natInfo = new NatInfo();
            }
            natInfo.External = (IPEndPoint)packet.Address;
            if (AppId != appId)
            {
                Recycle(packet);
                return;
            }
            Recycle(packet);
            lock(EventCollection) EventCollection.Enqueue(() => { OnDiscovery(natInfo); });
        }
        public void RaiseEventHostList(Packet packet)
        {
            if (OnHostList == null)
            {
                Recycle(packet);
                return;
            }
            NatInfo[] hosts = reader.PullObject<NatInfo[]>(packet);
            Recycle(packet);
            lock(EventCollection) EventCollection.Enqueue(() => { OnHostList(hosts); });
        }
    }
}
