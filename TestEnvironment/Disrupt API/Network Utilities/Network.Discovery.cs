using System.Net;

namespace RavelTek.Disrupt.Network_Utilities
{
    public partial class Network
    {
        public void LanDiscovery(int port)
        {
            var packet = client.CreatePacket();
            var Address = new IPEndPoint(IPAddress.Parse("255.255.255.255"), port);
            packet.Flag = Flags.Dscvr;
            writer.Push(client.AppId, packet);
            client.SendTo(packet, Protocol.Sequenced, Address);
        }
        public void AcknowledgeDiscovery(EndPoint destination, object response)
        {
            var packet = client.CreatePacket();
            packet.Flag = Flags.Dscvr;
            packet.Protocol = Protocol.Sequenced;
            writer.Push(client.AppId, packet);

            if(response != null)
                writer.Push(response, packet);

            client.Peers[destination].Send(packet);
        }
    }
}
