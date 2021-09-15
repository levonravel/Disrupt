using RavelTek.Disrupt.Serializers;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public class Hot
    {
        public IPEndPoint ExternalAddress;
        public IPEndPoint InternalAddress;
        public Writer writer = new Writer();
        public Exchange Exchange;
        public bool IsHost;
        public Events Events;

        public void RequestAdd(EndPoint destination, DisruptClient client)
        {
            Exchange.Peers.Add(destination, new PeerContainer(client)); //remove if add was denied
            var packet = Exchange.CreatePacket();
            packet.Flag = Flags.Conn;
            Exchange.Send(packet, Protocol.Sequenced, destination);
        }
        public void RequestAdd(string address, int port, DisruptClient client)
        {
            var destination = new IPEndPoint(IPAddress.Parse(address), port);
            Exchange.Peers.Add(destination, new PeerContainer(client)); //Remove if add was denied
            var packet = Exchange.CreatePacket();
            packet.Flag = Flags.Conn;
            Exchange.Send(packet, Protocol.Sequenced, destination);
        }
        public void DenyRequest() { }
        public void AddConnection(EndPoint destination, DisruptClient client)
        {
            Exchange.Peers.Add(destination, new PeerContainer(client));
        }
        public void FindLanMatches() 
        {
            var packet = Exchange.CreatePacket();
            var address = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 35005);            
            packet.Flag = Flags.Dscvr;
            Exchange.Send(packet, Protocol.Sequenced, address);
        }
        public void FindWanMatches()
        {
            
        }
        public void HostWanMatch(Packet packet)
        { 

        }
        public void DiscoveryResponse(Packet packet, EndPoint destination)
        {
            packet.Flag = Flags.Dscvr;
            Exchange.Send(packet, Protocol.Sequenced, destination);
        }
    }
}
