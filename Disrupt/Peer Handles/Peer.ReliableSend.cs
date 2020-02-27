using RavelTek.Disrupt.Serializers;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace RavelTek.Disrupt
{
    public partial class Peer
    {
        private Queue<Packet> waitingQueue = new Queue<Packet>();
        private readonly object sendLock = new object();
        private Packet[] sendBuffer = new Packet[TotalBufferSize];
        private bool[] sendBufferFlags = new bool[TotalBufferSize];
        private byte senderLowerBound;
        private int senderBuffer;
        private Writer sndWriter = new Writer();

        int ackCountTotal;

        private bool IsAcked(Packet packet)
        {
            return ((1 << packet.Id) & senderBuffer) != 0;
        }
        private void SendReliable(Packet packet)
        {
            lock (sendLock) waitingQueue.Enqueue(packet);            
        }
        void UpdateBuffer()
        {
            for (int i = 0; i < TotalBufferSize; i++)
            {
                if (waitingQueue.Count == 0) break;
                var index = (senderLowerBound + i) % TotalBufferSize;
                if (sendBuffer[index] != null) continue;
                Packet packet = null;
                lock (sendLock) packet = waitingQueue.Dequeue();
                packet.Id = (byte)index;
                packet.ReliableBufferFlag = sendBufferFlags[index];
                sendBuffer[index] = packet;
            }
        }
        public void TrySend()
        {
            UpdateBuffer();
            var sent = 0;
            for (int i = 0; i < TotalBufferSize; i++)
            {
                var index = (senderLowerBound + i) % TotalBufferSize;
                if (sendBuffer[index] == null) continue;
                //If were not completely connected dont allow any packets through the wire
                if (!HandshakeComplete)
                {
                    if (sendBuffer[index].Flag != Flags.Conn) return;
                }
                var packet = sendBuffer[index];
                if (packet == null || IsAcked(packet)) continue;
                sent++;
                SendPacket(packet);
            }
        }
        private void Confirmation(Packet packet)
        {
            senderBuffer = recvReader.PullInt(packet);
            ackCountTotal++;
            UpdateSenderLowerBound();
        }
        void UpdateSenderLowerBound()
        {
            var oldLowerBound = senderLowerBound;
            // If buffer is -1, all pending packets have been received
            var allReceived = senderBuffer == -1;
            // If ALL received, there is no new lower bound
            if (!allReceived)
                senderLowerBound = FindLowerBound(oldLowerBound, senderBuffer);

            //UnityEngine.Debug.Log("Lower bound: " +oldLowerBound);
            if (allReceived)
            {
                senderBuffer = 0;
                for (int i = 0; i < TotalBufferSize; i++)
                {
                    sendBuffer[i] = null;
                    sendBufferFlags[i] = !sendBufferFlags[i];
                }
            }
            else if (oldLowerBound != senderLowerBound)
            {
                for (int i = oldLowerBound; i != senderLowerBound; i = (i + 1) % TotalBufferSize)
                {
                    sendBuffer[i] = null;
                    sendBufferFlags[i] = !sendBufferFlags[i];
                }
            }
            UpdateBuffer();
        }
        private void SendPacket(Packet packet)
        {
            //UnityEngine.Debug.Log($"Packet sent: {packet.Id} | Index {packet.CurrentIndex} | ObjectID? {System.BitConverter.ToUInt16(new[] { packet.PayLoad[4], packet.PayLoad[5] }, 0)}");
            client.Socket.SendTo(packet.PayLoad, 0, packet.CurrentIndex, SocketFlags.None, Address);
            sendBuffer[packet.Id] = packet;
        }
    }
}