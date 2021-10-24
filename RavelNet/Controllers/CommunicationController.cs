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
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RavelNet
{
    public class CommunicationController
    {
        private readonly Writer writer = new Writer();
        private readonly Reader reader = new Reader();
        private readonly Socket socket;
        private readonly SequencedController sequencedController = new SequencedController();
        private readonly ReliableController reliableController = new ReliableController();
        private MethodTracker methodTracker = new MethodTracker();

        public CommunicationController(Socket socket, RavelNetEvents events)
        {
            this.socket = socket;
            reliableController = new ReliableController();
            sequencedController = new SequencedController();
            events.OnSend += Events_OnSend;
            events.OnMethodTracked += Events_OnMethodTracked;
            events.OnReceive += Events_OnReceive;
        }


        private void Events_OnReceive(Packet packet, Peer peer)
        {
            methodTracker.ReleasePacket(packet, peer);
        }
        private void Events_OnMethodTracked(params Action<Packet, Peer>[] methods)
        {
            methodTracker.AddMethods(methods);
        }
        private void Events_OnSend(Packet packet, Protocol protocol, Peer peer, string method)
        {
            var methodId = methodTracker.GetMethodId(method);
            writer.Open(packet).PackMethod(methodId);
            peer.Enqueue(packet, protocol, CollectionType.Outbound);
        }        
        public void Acknowledge(Peer peer)
        {
            var packet = new Packet();
            writer.Open(packet)
                .Add(peer.ReceivedBits);
            packet.Flag = Flags.UPD;
            packet.Protocol = Protocol.Sequenced;
            packet.Address = peer.Address;
            Send(packet);
        }
        public void TrySend(Peer peer)
        {
            TrySendReliable(peer);
            TrySendSequenced(peer);
        }
        private void TrySendReliable(Peer peer)
        {            
            List<Packet> packets = reliableController.TrySend(peer);            
            if (packets == null) return;
            foreach (var packet in packets)
            {
                Send(packet);
            }
        }
        private void TrySendSequenced(Peer peer)
        {
            var packet = sequencedController.TrySend(peer);
            if (packet == null) return;
            Send(packet);
        }
        private void Send(Packet packet)
        {
            CheckFlag(packet);
            socket.SendTo(packet.Payload, packet.CurrentIndex, SocketFlags.None, packet.Address);
        }
        private void CheckFlag(Packet packet)
        {
            if (packet.Flag == Flags.None)
            {
                packet.Flag = Flags.Dat;
            }
        }
        public void Confirmation(Packet packet, Peer peer)
        {
            peer.SendBits = reader.Int(packet);
            UpdateSenderLowerBound(peer);
        }
        void UpdateSenderLowerBound(Peer peer)
        {
            var oldLowerBound = peer.SendLowerBound;
            // If buffer is -1, all pending packets have been received
            var allReceived = peer.SendBits == -1;
            // If ALL received, there is no new lower bound
            if (!allReceived)
                peer.SendLowerBound = FindLowerBound(oldLowerBound, peer.SendBits);

            if (allReceived)
            {
                peer.SendBits = 0;
                for (int i = 0; i < 32; i++)
                {
                    peer.SendBuffer[i] = null;
                    peer.SendFlags[i] = !peer.SendFlags[i];
                }
            }
            else if (oldLowerBound != peer.SendLowerBound)
            {
                for (int i = oldLowerBound; i != peer.SendLowerBound; i = (i + 1) % 31)
                {
                    peer.SendBuffer[i] = null;
                    peer.SendFlags[i] = !peer.SendFlags[i];
                }
            }
        }

        private static int FindLowerBound(int currentLowerBound, int buffer)
        {
            // Loops 32 times, starting from current _lowerBound (IMPORTANT)
            for (int i = 0; i < 32; i++)
            {
                var index = (currentLowerBound + i) % 31;

                if ((buffer & (1 << index)) != 0) continue;
                return (byte)index;
            }
            return currentLowerBound;
        }
    }
}
