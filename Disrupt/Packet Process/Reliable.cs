using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public class Reliable : OperationLoop
    {
        private PeerContainer currentPeer;

        public Reliable(DisruptClient client)
        {
            Initiate(client);
        }

        public override void Initiate(DisruptClient client)
        {
            base.Initiate(client);
            //Register for ACK
            client.Events.OnPacketUpdate += Events_OnPacketUpdate;
        }
        private void Events_OnPacketUpdate(Packet packet)
        {
            Client.Exchange.Peers[packet.Address].PacketUpdate(packet, Client);
        }
        public override void RecieveReady(Packet packet)
        {
            base.RecieveReady(packet);
            //dont tell the client we recieved the packet let the PacketUpdate Transport layer do that
        }
        public override void SendReady(Packet packet)
        {
            base.SendReady(packet);
            currentPeer = Client.Exchange.Peers[packet.Address];
            currentPeer.AwaitingPackets.Enqueue(packet);
            //check the status on the sent packets.. 
        }
    }
}
