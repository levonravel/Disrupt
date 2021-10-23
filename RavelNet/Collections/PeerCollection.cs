using System;
using System.Collections.Generic;
using System.Net;

namespace RavelNet
{
    public class PeerCollection
    {
        private Dictionary<EndPoint, Peer> peers = new Dictionary<EndPoint, Peer>();

        public Peer Add(EndPoint address)
        {
            if (peers.ContainsKey(address)) return null;
            peers.Add(address, new Peer());
            return peers[address];
        }
        public void Remove(EndPoint address)
        {
            if (peers.ContainsKey(address)) return;
            peers.Remove(address);
        }
        public Peer GetPeer(EndPoint address)
        {
            DoesExist(address);
            return peers[address];
        }
        public Dictionary<EndPoint, Peer>.ValueCollection GetPeers
        {
            get
            {
                return peers.Values;
            }
        }
        public Dictionary<EndPoint, Peer> GetPeerCollection
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
                throw new Exception($"Your sending before the client has connected to {address}.");
            }
        }
    }
}
