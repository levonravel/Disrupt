/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      ReliableController
 *      SequencedController
 *      RavelNetEvents
 *      Packet
 *      Peer
 *
 *Class Information
 *      The main communication between the application and peers over the network
 */
using System.Net.Sockets;
using System.Threading;

namespace RavelNet
{
    public class CommunicationController
    {
        private Socket socket;
        private Client client;
        private SequencedController sequencedController;
        private ReliableController reliableController;
        private Writer writer = new Writer();

        public CommunicationController(Socket socket, Client client)
        {
            this.client = client;
            this.socket = socket;
            reliableController = new ReliableController(this);
            sequencedController = new SequencedController(this);
        }
        public void TrySendSequenced(Peer peer, Packet packet)
        {
            if (!peer.GoodRTT()) return;
            sequencedController.TrySend(peer, packet);
        }
        public bool TrySendReliable(Peer peer)
        {
            if(!peer.GoodRTT()) return false;
            //check if the peer is able to send at this time if the network conditions are good then send if not then dont
            return reliableController.TrySend(peer);
        }
        public void TryReceive(Peer peer, Packet packet)
        {
            if(packet.Protocol == Protocol.Sequenced)
            {
                sequencedController.TryReceive(peer, client, packet);
            }
            else
            {
                reliableController.TryReceive(peer, client, packet);
            }
        }
        public void Send(Packet packet)
        {
            packet.Flag = packet.Flag == Flags.None ? Flags.Dat : packet.Flag;
            socket.SendTo(packet.Payload, packet.CurrentIndex, SocketFlags.None, packet.Address);
        }

        /******************************************************************
         ******                Keep Alive Method                     ******
         ******                Includes Bitfield                     ******
         ******************************************************************/

        public void Ping(Peer peer)
        {
            if (!peer.GoodRTT()) return;
            var packet = client.GetPacket();
            packet.Flag = Flags.UPD;
            packet.Protocol = Protocol.Sequenced;
            packet.Address = peer.Address;
            writer.Open(packet).Add(peer.ReceivedBitfield);
            Send(packet);
        }
    }
}
