using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RavelNet
{
    public class Listener : Events
    {
        private Socket socket;
        public IPEndPoint address;
        public bool IsActive;
        public Dictionary<EndPoint, Peer> Peers = new Dictionary<EndPoint, Peer>();
        private int sendInterval;
        public Listener(string applicationName, int port, int sendInterval)
        {
            IsActive = true;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            address = new IPEndPoint(IPAddress.Any, port);
            socket.ReceiveBufferSize = int.MaxValue;
            socket.SendBufferSize = int.MaxValue;
            socket.DontFragment = true;
            socket.EnableBroadcast = true;
            socket.Bind(address);
            var listenerThread = new Thread(Listen)
            {
                IsBackground = true
            };
            listenerThread.Start();
            OnInternalConnect += Listener_OnInternalConnect;
            this.sendInterval = sendInterval;
        }

        private void Listener_OnInternalConnect(Packet packet)
        {
            AddPeer(packet.Address);
            var peer = Peers[packet.Address];
            if (peer.IsConnected) return;
            peer.IsConnected = true;
            RaiseConnect(packet);
        }

        private void Listen()
        {
            while (IsActive)
            {
                try
                {
                    var packet = new Packet();
                    packet.Length = socket.ReceiveFrom(packet.Payload, 0, 512, SocketFlags.None, ref packet.Address);
                    AwaitPacket(packet);
                }
                catch
                {
                }
            };
        }
        public void Connect(string ip, int port)
        {
            var destination = new IPEndPoint(IPAddress.Parse(ip), port);
            Connect(destination);
        }
        public void Connect(EndPoint destination)
        {
            AddPeer(destination);
            var packet = new Packet();
            packet.Flag = Flags.Con;
            packet.Address = destination;
            packet.Protocol = Protocol.Reliable;
            RaiseInternalSend(packet);
        }
        public void Send(Packet packet, Protocol protocol, EndPoint address)
        {
            if (packet.Flag == Flags.None) packet.Flag = Flags.Dat;
            packet.Address = address;
            packet.Protocol = protocol;
            RaiseInternalSend(packet);
        }
        private void AddPeer(EndPoint address)
        {
            if (Peers.ContainsKey(address)) return;
            Peers.Add(address, new Peer(address, socket, this, sendInterval));
            return;
        }
        public void Dispose()
        {
            IsActive = false;
            socket.Dispose();
        }
    }
}
