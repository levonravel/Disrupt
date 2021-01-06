using RavelTek.Disrupt.Serializers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RavelTek.Disrupt
{
    public partial class Peer
    {
        private Packet[] received = new Packet[TotalBufferSize];
        private bool[] receivedBufferFlags = new bool[TotalBufferSize];
        private Queue<Packet> fragments = new Queue<Packet>();
        private Reader recvReader = new Reader();
        private Writer recvWriter = new Writer();
        private int receiverBuffer;
        private byte receiverLowerBound;

        private void PrepareReliable(Packet packet)
        {
            var __expectedFlag = receivedBufferFlags[packet.Id];
            if (packet.ReliableBufferFlag != __expectedFlag)
            {
                client.Recycle(packet);
                return;
            }

            Acknowledge(packet.Id);
            received[packet.Id] = packet;
            UpdateReceiverLowerBound();
        }
        public void Acknowledge(int id)
        {
            receiverBuffer += 1 << id;
            var ackPacket = client.CreatePacket();
            ackPacket.Flag = Flags.Ack;
            recvWriter.Push(receiverBuffer, ackPacket);
            client.Socket.SendTo(ackPacket.PayLoad, ackPacket.CurrentIndex, SocketFlags.None, Address);
            client.Recycle(ackPacket);
        }
        void UpdateReceiverLowerBound()
        {
            var oldLowerBound = receiverLowerBound;
            // If buffer is -1, all pending packets have been received
            var allReceived = receiverBuffer == -1;
            // If ALL received, there is no new lower bound
            if (!allReceived)
                receiverLowerBound = FindLowerBound(receiverLowerBound, receiverBuffer);

            if (allReceived)
            {
                receiverBuffer = 0;
                for (int i = 0; i < TotalBufferSize; i++)
                {
                    var __index = (receiverLowerBound + i) % 32;

                    var packet = received[__index];
                    if (object.ReferenceEquals(packet,null)) continue;
                    received[__index] = null;
                    receivedBufferFlags[__index] = !receivedBufferFlags[__index];
                    var constructed = ShouldConstruct(packet);
                    if (object.ReferenceEquals(constructed,null)) continue;
                    //Auto-recycle
                    ReleasePacket(constructed);
                }
            }
            // If Lower bound hasn't changed, there's no need to free up spaces in the buffer
            else if (oldLowerBound != receiverLowerBound)
            {
                // Updates buffer to fit new lower bound
                for (int i = oldLowerBound; i != receiverLowerBound; i = (i + 1) % TotalBufferSize)
                {
                    receiverBuffer -= 1 << i;
                    var packet = received[i];
                    if (object.ReferenceEquals(packet, null)) continue;
                    received[i] = null;
                    receivedBufferFlags[i] = !receivedBufferFlags[i];
                    var constructed = ShouldConstruct(packet);
                    if (object.ReferenceEquals(constructed, null)) continue;
                    //Auto-recycle
                    ReleasePacket(constructed);
                }
            }
        }
        private Packet ShouldConstruct(Packet packet)
        {
            if (packet.Fragmented == Fragment.Begin)
            {
                fragments.Enqueue(packet);

                return null;
            }
            else
            {
                if (fragments.Count == 0)
                {
                    return packet;
                }
                fragments.Enqueue(packet);
                var outPacket = client.CreatePacket();
                Array.Resize(ref outPacket.PayLoad, outPacket.PayLoad.Length * (fragments.Count));
                int count = 0;
                while (fragments.Count != 0)
                {
                    var frag = fragments.Dequeue();
                    Buffer.BlockCopy(frag.PayLoad, Packet.HeaderSize, outPacket.PayLoad, count == 0 ? Packet.HeaderSize : 254 * count + Packet.HeaderSize, 254);
                    if (frag.Fragmented == Fragment.End)
                    {
                        outPacket.Flag = packet.Flag;
                        outPacket.Protocol = packet.Protocol;
                        return outPacket;
                    }
                    count++;
                    client.Recycle(frag);
                }
            }
            return null;
        }
        private void ReleasePacket(Packet packet)
        {
            switch (packet.Flag)
            {
                case Flags.NatReq:
                    client.NetworkUtilities.NatReq(packet, Address);
                    break;
                case Flags.NatIntro:
                    client.NetworkUtilities.NatIntro(packet);
                    break;
                case Flags.NatHost:
                    Host(packet);
                    break;
                case Flags.Dat:
                    client.RaiseEventData(packet);
                    break;
                case Flags.Conn:
                    Connect(packet);
                    break;
                case Flags.HostList:
                    HostList(packet, Address);
                    break;
                default:
                    break;
            }
        }
        private void Connect(Packet packet)
        {
            var peer = client.Peers[Address];
            if (peer.HandshakeComplete)
            {
                client.Recycle(packet);
                return;
            }            
            peer.CaclulateRTT();
            peer.HandshakeComplete = true;
            client.Connect(packet.Address);
            client.Id = recvReader.PullInt(packet);
            client.RaiseEventConnect(packet, peer);
        }
        private void HostList(Packet packet, EndPoint Address)
        {
            if (Address.Equals(client.RelayAddress))
            {
                client.RaiseEventHostList(packet);
                return;
            }
            var appId = "";
            appId = recvReader.PullString(packet);
            var hosts = client.HostManager.ServerOnlyHostList(appId);
            if (object.ReferenceEquals(hosts, null))
            {
                client.Recycle(packet);
                return;
            }
            var outPacket = client.CreatePacket();
            outPacket.Flag = Flags.HostList;
            recvWriter.Push(hosts, outPacket);
            client.SendTo(outPacket, Protocol.Reliable, Address);
        }
        private void Host(Packet packet)
        {
            var appId = "";
            NatInfo hostInfo = null;
            appId = recvReader.PullString(packet);
            hostInfo = recvReader.PullObject<NatInfo>(packet);
            hostInfo.External = (IPEndPoint)Address;
            if (!client.HostManager.ServerOnlyAddHost(hostInfo, appId))
            {
                Console.WriteLine("Failed To Create Match");
            }
            client.Recycle(packet);
        }
    }
}
