using RavelTek.Disrupt.Serializers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RavelTek.Disrupt
{
    public class PeerContainer
    {
        private const int maxIndexes = 32;
        private FragmentProcess fragmentProcess;
        private Exchange exchange;
        private EndPoint address;
        public Writer writer = new Writer();
        private Packet[] recvPackets = new Packet[maxIndexes];
        private bool[] recvBufferFlags = new bool[maxIndexes];
        private Reader reader = new Reader();
        private int recvBuffer;
        private byte recvLower;

        public PeerContainer(DisruptClient client, EndPoint address)
        {
            this.address = address;
            fragmentProcess = new FragmentProcess(client);
            exchange = fragmentProcess.Client.Exchange;
        }
        public void EnqueuePacket(Packet packet)
        {
            fragmentProcess.ShouldFragment(packet);
        }
        public void Receive(Packet packet)
        {
            //works
            if(packet.Flag == Flags.Conn)
            {
                fragmentProcess.Client.Events.RaiseEventAddRequest(packet);
                recvBuffer |= 1 << packet.Id;
                return;
            }
            //works
            if (packet.ReliableBufferFlag != recvBufferFlags[packet.Id])
            {
                exchange.RecyclePacket(packet);
                return;
            }
            recvBuffer |= 1 << packet.Id;
            Acknowledge();
            recvPackets[packet.Id] = packet;
            for (int i = recvLower; i < maxIndexes; i++)
            {
                if (recvPackets[i] == null) break;
                fragmentProcess.ConstructPacket(recvPackets[i]);
                recvPackets[i] = null;
                recvBufferFlags[i] = !recvBufferFlags[i];
                recvLower++;
            }
            if (recvBuffer == -1)
            {
                recvBuffer = 0;
                recvLower = 0;
            }
        }
        public void Acknowledge()
        {            
            var ackPacket = exchange.CreatePacket();
            ackPacket.Flag = Flags.PacketUpdate;
            writer.Open(ackPacket)
                .Add(recvBuffer);
            ackPacket.Protocol = Protocol.Reliable;
            ackPacket.Address = address;
            exchange.SendRaw(ackPacket);
            exchange.RecyclePacket(ackPacket);
        }
        //////////////////////////////////////////////////////////////////////////////////////////// SENDING ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private Packet[] sendBuffer = new Packet[maxIndexes];
        private bool[] sendBufferFlags = new bool[maxIndexes];
        private int senderBuffer;
        private byte lowerBound;

        public void TrySend()
        {
            for (byte i = lowerBound; i < maxIndexes; i++)
            {
                if (fragmentProcess.Awaited.Count == 0 || sendBuffer[i] != null) break;
                Packet packet = fragmentProcess.Awaited.Dequeue();
                packet.Id = i;
                packet.ReliableBufferFlag = sendBufferFlags[i];
                sendBufferFlags[i] = !sendBufferFlags[i];
                sendBuffer[i] = packet;
                SendPacket(packet);
                lowerBound++;
            }
        }
        public void Confirmation(Packet packet)
        {
            senderBuffer = reader.PullInt(packet);
            for(int i = 0; i < maxIndexes; i++)
            {
                if (sendBuffer[i] == null) continue;
                if((senderBuffer & (1 << i)) != 0)
                {
                    exchange.RecyclePacket(sendBuffer[i]);
                    sendBuffer[i] = null;                    
                }
                else
                {
                    exchange.SendRaw(sendBuffer[i]);
                }
            }
            if(senderBuffer == -1)
            {
                lowerBound = 0;
            }
        }
        private void SendPacket(Packet packet)
        {
            packet.Address = address;
            exchange.SendRaw(packet);
            sendBuffer[packet.Id] = packet;
        }
    }
}