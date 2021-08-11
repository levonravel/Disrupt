using System;
using System.Net;

namespace RavelTek.Disrupt
{
    public partial class Peer
    {
        /// <summary>
        /// Returns the next lower bound inside a buffer, starting from the current bound
        /// </summary>
        private static byte FindLowerBound(byte currentLowerBound, int buffer)
        {
            // Loops 32 times, starting from current _lowerBound (IMPORTANT)
            for (int i = 0; i < TotalBufferSize; i++)
            {
                var __index = (currentLowerBound + i) % TotalBufferSize;

                if ((buffer & (1 << __index)) != 0) continue;
                return (byte)__index;
            }
            return currentLowerBound;
        }

        const int TotalBufferSize = 32;

        public EndPoint Address;
        public int Id;
        public double RTT;
        public DateTime LastCheckedRTT = DateTime.Now;
        public bool HandshakeComplete;
        private DateTime lastSeen = DateTime.Now;
        private readonly float pingTimeout = 3F;
        private readonly Client client;
        private DateTime startRTT;

        public void CaclulateRTT()
        {
            RTT = DateTime.Now.Subtract(startRTT).Milliseconds;
        }
        public Peer(IPEndPoint endpoint, Client netClient)
        {
            if ((endpoint.Address.Equals(netClient.Address.External.Address)))
            {
                Address = new IPEndPoint(netClient.Address.Internal.Address, endpoint.Port);
            }
            else
            {
                Address = endpoint;
            }
            startRTT = DateTime.Now;
            pingTimeout = netClient.PingTimeout;
            client = netClient;
        }
        public bool IsConnected
        {
            get
            {
                var dcDiff = DateTime.Now.Subtract(lastSeen).Seconds;
                if (dcDiff > pingTimeout)
                    return false;
                else
                    return true;
            }
        }
        public void Send(Packet packet)
        {
            switch (packet.Protocol)
            {
                case Protocol.Reliable:
                    SendReliable(packet);
                    break;
                case Protocol.Sequenced:
                    SendSequenced(packet);
                    break;
                default:
                    client.Recycle(packet);
                    break;
            }
        }
        public void Receive(Packet packet)
        {
            if (packet.Flag == Flags.Ping)
            {
                ProcessPing(packet);
                return;
            }
            if (packet.Flag == Flags.Ack)
            {
                Confirmation(packet);
                client.Recycle(packet);
                return;
            }
            switch (packet.Protocol)
            {
                case Protocol.Reliable:
                    PrepareReliable(packet);
                    break;
                case Protocol.Sequenced:
                    ProcessSequenced(packet);
                    break;
                default:
                    client.Recycle(packet);
                    break;
            }
        }
        public void Ping()
        {
            var packet = client.CreatePacket();
            packet.Flag = Flags.Ping;
            client.Socket.SendTo(packet.Payload, packet.CurrentIndex, System.Net.Sockets.SocketFlags.None, Address);
            client.Recycle(packet);
        }
        private void ProcessPing(Packet packet)
        {
            lastSeen = DateTime.Now;
            client.Recycle(packet);
        }
    }
}
