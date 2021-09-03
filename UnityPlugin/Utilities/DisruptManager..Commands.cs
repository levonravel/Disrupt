using RavelTek.Disrupt.Serializers;
using System;
using System.Net;
using System.Collections.Generic;

namespace RavelTek.Disrupt
{
    public partial class DisruptManager
    {
        private List<Peer> p2pPeers = new List<Peer>();

        public void FindLanMatches()
        {
            Client.IsServer = false;
            NetType = Network.Lan;
            Client.LanDiscovery(35005, 0);
        }
        public void FindWanMatches()
        {
            Client.IsServer = false;
            NetType = Network.Wan;
            Client.GetHostList();
        }
        public void ConnectToMatch(NatInfo matchInfo)
        {
            if (matchInfo.External.Address.Equals(Client.Address.External.Address))
            {
                Client.Connect(matchInfo.Internal);
                HostIp = matchInfo.Internal;
                return;
            }
            if (NetType == Network.Lan)
                Client.Connect(matchInfo.Internal);
            else
                Client.NatPunchClient(matchInfo.External);
            HostIp = matchInfo.External;
        }
        public void HostWanSimple()
        {
            Client.IsServer = true;
            var match = new NatInfo();
            Client.NatPunchHost(match);

        }
        public void HostWanComplex(NatInfo natInfo)
        {
            Client.IsServer = true;
            Client.NatPunchHost(natInfo);
        }
        public void HostLanSimple()
        {
            Client.IsServer = true;
        }
        public void HostLanComplex(NatInfo natInfo)
        {
            Client.IsServer = true;
        }
        public void CreateClient()
        {
            if (client != null) return;
            client = new Client(DisruptManagement.GetAppId, 0, DisruptManagement.RelayServer, DisruptManagement.RelayPort);
            Peer = client.ManualAddEndPoint(Client.Address.Internal);
            Disrupt.Manager.HostIp = client.Address.Internal;
            client.Connect(DisruptManagement.RelayServer, DisruptManagement.RelayPort);
            client.PingTimeout = DisruptManagement.GetPingTimeout;
            RegisterEvents();
        }
        private void RegisterEvents()
        {
            Client.OnConnected += Events_OnConnected;
            Client.OnDisconnected += Client_OnDisconnected;
            Client.OnIncomingMessage += Events_OnIncomingMessage;
            Client.OnDiscovery += Events_OnDiscovery;
        }

        private void Client_OnDisconnected(EndPoint endPoint)
        {
        }
        private void Events_OnDiscovery(NatInfo natInfo)
        {
            throw new NotImplementedException();
        }
        private void Events_OnConnected(Peer peer)
        {            
        }
        [RD]
        public void ConnectP2P(EndPoint endPoint)
        {
            if (endPoint.Equals(Peer.Address)) return;
            UnityEngine.Debug.Log($"P2P request {endPoint}");
            Client.Connect(endPoint);
        }
    }
}