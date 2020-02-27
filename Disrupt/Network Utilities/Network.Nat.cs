using System;
using System.Net;

namespace RavelTek.Disrupt.Network_Utilities
{
    public partial class Network
    {
        public void NatReq(Packet packet, EndPoint Address)
        {
            Console.WriteLine("Got Nat Request");
            //host
            var clientEp = Address;
            //client
            var hostEp = reader.PullObject<IPEndPoint>(packet);//send client             
            client.Recycle(packet);
            Send(hostEp, clientEp);
            Send(clientEp, hostEp);
        }
        public void NatIntro(Packet packet)
        {
            var destination = reader.PullObject<IPEndPoint>(packet);
            client.Recycle(packet);
            client.Connect(destination);
            if (!client.IsServer)
                client.Disconnect(client.RelayAddress);
        }
        private void Send(EndPoint destination, EndPoint connection)
        {
            var packet = client.CreatePacket();
            packet.Flag = Flags.NatIntro;
            packet.Protocol = Protocol.Reliable;
            writer.Push(connection, packet);
            client.Peers[destination].Send(packet);
        }
    }
}
