using System.Collections.Generic;

namespace RavelNet
{
    public class Reliable
    {
        private const int maxIndexes = 32;
        public Writer writer = new Writer();
        private Fragmenter fragmenter = new Fragmenter();
        private readonly List<Packet> resend = new List<Packet>();
        private readonly Packet[] recvPackets = new Packet[maxIndexes];
        private readonly bool[] recvBufferFlags = new bool[maxIndexes];
        private readonly Packet[] sendBuffer = new Packet[maxIndexes];
        private readonly Reader reader = new Reader();
        private byte recvLower;
        private int recvBuffer, senderBuffer;

        public Packet ReceiveReliable(Packet packet)
        {
            if (packet.Flag == Flags.Con)
            {
                recvBuffer |= 1 << packet.Id;
                return null;
            }
            if (packet.ReliableBufferFlag != recvBufferFlags[packet.Id]) return null;
            recvBuffer |= 1 << packet.Id;
            Acknowledge();
            recvPackets[packet.Id] = packet;
            for (int i = recvLower; i < maxIndexes; i++)
            {
                if (recvPackets[i] == null) break;
                var constructed = fragmenter.ConstructPacket(recvPackets[i]);
                if (constructed == null) return null;
                recvPackets[i] = null;
                recvBufferFlags[i] = !recvBufferFlags[i];
                recvLower++;
                return packet;
            }
            if (recvBuffer == -1)
            {
                recvBuffer = 0;
                recvLower = 0;
            }
            return null;
        }
        public Packet Acknowledge()
        {
            var ackPacket = new Packet
            {
                Flag = Flags.UPD
            };
            writer.Open(ackPacket)
                .Add(recvBuffer);
            ackPacket.Protocol = Protocol.Reliable;
            return ackPacket;
        }
        public List<Packet> Confirmation(Packet packet)
        {
            resend.Clear();
            senderBuffer = reader.Int(packet);
            for (int i = 0; i < maxIndexes; i++)
            {
                if (sendBuffer[i] == null) continue;
                if ((senderBuffer & (1 << i)) != 0)
                {
                    sendBuffer[i] = null;
                }
                else
                {
                    resend.Add(sendBuffer[i]);
                }
            }
            return resend.Count > 0 ? resend : null;
        }
    }
}
