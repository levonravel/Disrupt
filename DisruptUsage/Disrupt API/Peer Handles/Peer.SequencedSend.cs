using System.Net.Sockets;

namespace RavelTek.Disrupt
{
    public partial class Peer
    {
        private byte sentSeq;

        public void SendSequenced(Packet packet)
        {
            sentSeq++;
            packet.Id = sentSeq;            
            client.Socket.SendTo(packet.Payload, packet.CurrentIndex, SocketFlags.None, Address);
            client.Recycle(packet);
        }
    }
}
