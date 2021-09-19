using RavelTek.Disrupt.Serializers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RavelTek.Disrupt
{
    public class PeerContainer
    {
        private const int maxIndexes = 32;
        private FragmentProcess fragmentProcess;
        private EndPoint address;
        public Writer writer = new Writer();        
        private Packet[] sentPackets = new Packet[maxIndexes];
        private Packet[] recvPackets = new Packet[maxIndexes];
        private Reader reader = new Reader();
        private byte sentIndex;
        private int recvBits;
        private int confBits;
        private int recvLower = 0;

        public PeerContainer(DisruptClient client, EndPoint address)
        {
            this.address = address;
            fragmentProcess = new FragmentProcess(client);
        }
        public int ReceivedBits
        {
            get 
            { 
                return recvBits; 
            }  
            set
            { 
                recvBits = value; 
            }
        }

        public void Receive(Packet packet)
        {
            if(packet.Id == 69)
            {
                ReceivedBits = 0;
                return;
            }
            if (ReceivedBits == -1 || recvPackets[packet.Id] != null) return;
            ReceivedBits |= 1 << packet.Id;
            recvPackets[packet.Id] = packet;
            while(true)
            { 
                if (recvPackets[recvLower] == null) return;             
                fragmentProcess.ConstructPacket(recvPackets[recvLower]);
                recvPackets[recvLower] = null;
                recvLower++;
                recvLower %= maxIndexes;
            }
        }
        public void SendUpdate()
        {            
            var packet = fragmentProcess.Client.Exchange.CreatePacket();
            writer.Open(packet)
                .Add(recvBits);
            packet.Flag = Flags.PacketUpdate;
            packet.Protocol = Protocol.Reliable;
            packet.Address = address;
            fragmentProcess.Client.Exchange.SendRaw(packet);
        }
        public void EnqueuePacket(Packet packet)
        {
            fragmentProcess.ShouldFragment(packet);
        }
        public void PacketUpdate(Packet packet)
        {
            var bits = reader.PullInt(packet);
            if (bits == -1)
            {
                //reset sender counter
                sentIndex = 0;
                var confirmation = fragmentProcess.Client.Exchange.CreatePacket();
                confirmation.Flag = Flags.Dat;
                confirmation.Protocol = Protocol.Reliable;
                confirmation.Id = 69;
                confirmation.Address = address;
                fragmentProcess.Client.Exchange.SendRaw(confirmation);
                if (sentPackets[maxIndexes - 1] != null)
                {
                    fragmentProcess.Client.Exchange.RecyclePacket(sentPackets[maxIndexes - 1]);
                    sentPackets[maxIndexes - 1] = null;
                }
                return;
            }
            for (int i = 0; i < maxIndexes; i++)
            {
                if (sentPackets[i] == null) continue;
                if ((bits & (1 << i)) != 0)
                {
                    fragmentProcess.Client.Exchange.RecyclePacket(sentPackets[i]);
                    sentPackets[i] = null;
                }
                else
                {
                    fragmentProcess.Client.Exchange.SendRaw(sentPackets[i]);
                }
            }
        }
        public void TrySend()
        {
            while (CanSend() && fragmentProcess.Awaited.Count > 0)
            {
                Send(fragmentProcess.Awaited.Dequeue());
            }
        }
        private void Send(Packet packet)
        {
            sentPackets[sentIndex] = packet;
            packet.Id = sentIndex;
            packet.Address = address;
            fragmentProcess.Client.Socket.SendTo(packet.Payload, packet.CurrentIndex, System.Net.Sockets.SocketFlags.None, packet.Address);
            sentIndex++;
        }
        private bool CanSend()
        {
            if (sentIndex == maxIndexes) return false;
            return sentPackets[sentIndex] == null;
        }
    }
}
