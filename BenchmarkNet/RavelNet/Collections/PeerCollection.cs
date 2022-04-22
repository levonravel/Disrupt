using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace RavelNet
{
    public class PeerCollection
    {
        private ConcurrentDictionary<EndPoint, Peer> peers = new ConcurrentDictionary<EndPoint, Peer>();
        private static readonly object collection = new object();

        public Peer Add(EndPoint address)
        {
            if (peers.ContainsKey(address)) return null;
            peers.TryAdd(address, new Peer(address));
            return peers[address];
        }
        public void Remove(EndPoint address)
        {
            if (peers.ContainsKey(address)) return;
            peers.TryRemove(address, out Peer peer);
        }
        public Peer GetPeer(EndPoint address)
        {
            DoesExist(address);
            return peers[address];
        }
        public ICollection<Peer> GetPeers
        {
            get
            {
                return peers.Values;
            }
        }
        public ConcurrentDictionary<EndPoint, Peer> GetPeerCollection
        {
            get
            {
                return peers;
            }
        }
        private void DoesExist(EndPoint address)
        {
            if (!peers.ContainsKey(address))
            {
                throw new Exception($"Your sending before the client has been connected to {address}.");
            }
        }
    }
}
