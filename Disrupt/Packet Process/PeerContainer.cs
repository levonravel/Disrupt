using RavelTek.Disrupt.Serializers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RavelTek.Disrupt
{
    public class PeerContainer : FragmentProcess
    {
        private DisruptClient client;
        private Packet[] sentPackets = new Packet[32];
        private Packet[] recvPackets = new Packet[32];
        private Reader reader = new Reader();
        private byte sentIndex;
        private int recvBits;
        private bool isLooping;

        public int ReceivedBits
        {
            get { return recvBits; }  set { recvBits = value; }
        }

        public void SendLoop()
        {
            if (isLooping) return;
            Task.Run(() =>
            {
                while(Awaited.Count > 0)
                {
                    TrySend();
                    Thread.Sleep(1);
                }
            });
            isLooping = false;
        }
        public PeerContainer(DisruptClient client)
        {
            this.client = client;
        }
        public Packet Receive(Packet packet)
        {
            ReceivedBits |= 1 << packet.Id;
            return ConstructPacket(packet);
        }
        public void EnqueuePacket(Packet packet)
        {
            ShouldFragment(packet);
        }
        public void PacketUpdate(Packet packet)
        {
            var bits = reader.PullInt(packet);
            for(int i = 0; i < 32; i++)
            {
                if ((bits & (1 << i)) != 0)
                {
                    client.Exchange.RecyclePacket(sentPackets[i]);
                    sentPackets[i] = null;
                }
                else
                {
                    client.Exchange.SendRaw(sentPackets[i]);
                }                
            }
        }
        public void TrySend()
        {
            if (!CanSend()) return;
            Send(Awaited.Dequeue());
        }
        private void Send(Packet packet)
        {
            sentIndex = (byte)(sentIndex % 31);
            sentPackets[sentIndex] = packet;
            packet.Id = sentIndex;
            client.Socket.SendTo(packet.Payload, packet.CurrentIndex, System.Net.Sockets.SocketFlags.None, packet.Address);
            sentIndex++;
        }
        private bool CanSend()
        {
            sentIndex = (byte)(sentIndex % 31);
            return sentPackets[sentIndex] == null ? true : false;
        }
    }
}
