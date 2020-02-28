using RavelTek.Disrupt.Network_Utilities;
using RavelTek.Disrupt.Serializers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RavelTek.Disrupt
{
    public partial class Client
    {
        public Peer ManualAddEndPoint(EndPoint endpoint)
        {
            var peer = new Peer((IPEndPoint)endpoint, this);
            Peers.TryAdd(endpoint, peer);
            peer.HandshakeComplete = true;
            return peer;
        }
        public void ChangePort(int port)
        {
            if (Socket != null)
            {
                Socket.Bind(new IPEndPoint(IPAddress.Any, port));
            }
        }
        public Packet CreatePacket()
        {
            return pool.CreateObject();
        }
        public void SendTo(Packet packet, Protocol protocol, EndPoint destination)
        {
            if (packet.Flag == 0) packet.Flag = Flags.Dat;
            packet.Protocol = protocol;
            packet.SingleSend = true;
            lock (sendLock) SendQueue.Enqueue(packet);
        }
        public void SendTo(Packet packet, Protocol protocol, string address, int port)
        {
            if (packet.Flag == 0) packet.Flag = Flags.Dat;
            var destination = new IPEndPoint(IPAddress.Parse(address), port);
            packet.Protocol = protocol;
            packet.SingleSend = true;
            packet.Address = destination;
            lock (sendLock) SendQueue.Enqueue(packet);
        }
        private void SendTo(Packet packet)
        {
            FragmentCheck(packet);
            while (fragments.Count > 0)
            {
                packet = fragments.Dequeue();
                Peers[packet.Address].Send(packet);
            }
        }
        public void Broadcast(Packet packet, Protocol protocol, EndPoint exclude = null)
        {
            if (packet.Flag == 0) packet.Flag = Flags.Dat;
            packet.Protocol = protocol;
            packet.Address = exclude;
            lock (sendLock) SendQueue.Enqueue(packet);
        }
        public void Broadcast(Packet packet, Protocol protocol, string excludeAddress = "", int excludePort = 0)
        {
            if (packet.Flag == 0) packet.Flag = Flags.Dat;
            packet.Protocol = protocol;
            if (excludePort != 0)
            {
                packet.Address = new IPEndPoint(IPAddress.Parse(excludeAddress), excludePort);
            }
            else
            {
                packet.Address = null;
            }
            lock (sendLock) SendQueue.Enqueue(packet);
        }
        private void Broadcast(Packet packet)
        {
            FragmentCheck(packet);
            while (fragments.Count > 0)
            {
                packet = fragments.Dequeue();
                foreach (var peer in Peers)
                {
                    if (peer.Value.Address.Equals(RelayAddress)) continue;
                    if (packet.Address != null && packet.Address.Equals(peer.Key)) continue;
                    var hardcopy = packet.Clone();
                    peer.Value.Send(hardcopy);
                }
            }
            //Recycle(packet);
            packet.RecyclePacketFast(packet);
        }
        public void LanDiscovery(int port, float autoConnectSeconds = 0)
        {
            NetworkUtilities.LanDiscovery(port);
        }
        public void AcknowledgeDiscovery(EndPoint endPoint, object message)
        {
            NetworkUtilities.AcknowledgeDiscovery(endPoint, message);
        }
        public void NatPunchClient(IPEndPoint host)
        {
            try
            {
                if (!CheckConnection(RelayAddress)) Connect(RelayAddress);
                var packet = CreatePacket();
                packet.Flag = Flags.NatReq;
                packet.Protocol = Protocol.Reliable;
                writer.Push(host, packet);
                Peers[RelayAddress].Send(packet);
            }
            catch
            {
            }
        }
        public void NatPunchClient(string address, int port)
        {
            if (!CheckConnection(RelayAddress)) Connect(RelayAddress);
            var host = new IPEndPoint(IPAddress.Parse(address), port);
            var packet = CreatePacket();
            packet.Flag = Flags.NatReq;
            packet.Protocol = Protocol.Reliable;
            writer.Push(host, packet);
            Peers[RelayAddress].Send(packet);
        }
        public void NatPunchHost(NatInfo hostInfo)
        {
            if (!CheckConnection(RelayAddress))
                Connect(RelayAddress);
            hostInfo.Internal = Address.Internal;
            var packet = CreatePacket();
            packet.Flag = Flags.NatHost;
            packet.Protocol = Protocol.Reliable;
            writer.Push(AppId, packet);
            writer.Push(hostInfo, packet);
            Peers[RelayAddress].Send(packet);
        }
        public void GetHostList()
        {
            if (!CheckConnection(RelayAddress)) Connect(RelayAddress);
            var packet = CreatePacket();
            packet.Flag = Flags.HostList;
            writer.Push(AppId, packet);
            packet.Protocol = Protocol.Reliable;
            Peers[RelayAddress].Send(packet);
        }
        public void Connect(EndPoint endPoint, object data = null)
        {
            if (!Peers.TryGetValue(endPoint, out Peer peer))
            {
                Peers.TryAdd(endPoint, new Peer((IPEndPoint)endPoint, this));
            }
            var packet = CreatePacket();
            packet.Flag = Flags.Conn;
            packet.Protocol = Protocol.Reliable;
            if (IsServer && !endPoint.Equals(RelayAddress))
            {
                HostManager.ConnectionIdCount += 1;
                writer.Push(HostManager.ConnectionIdCount, packet);
            }
            Peers[endPoint].Send(packet);
        }
        public void Connect(string address, int port, object data = null)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(address), port);
            if (!Peers.TryGetValue(endPoint, out Peer peer))
            {
                Peers.TryAdd(endPoint, new Peer(endPoint, this));
            }
            var packet = CreatePacket();
            packet.Flag = Flags.Conn;
            packet.Protocol = Protocol.Reliable;
            if (IsServer && !endPoint.Equals(RelayAddress))
            {
                HostManager.ConnectionIdCount += 1;
                writer.Push(HostManager.ConnectionIdCount, packet);
            }
            Peers[endPoint].Send(packet);
        }
        public void Disconnect(EndPoint endPoint)
        {
            Peers.TryGetValue(endPoint, out Peer peer);
            if (peer == null) return;
            Peers.TryRemove(endPoint, out peer);
        }
        public void Disconnect(string address, int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Peers.TryGetValue(endPoint, out Peer peer);
            if (peer == null) return;
            Peers.TryRemove(endPoint, out peer);
        }
        public void Poll()
        {
            while (EventCollection.Count > 0)
            {
                Action action;
                lock (EventCollection) action = EventCollection.Dequeue();
                action.Invoke();
            }
        }
        public void Dispose()
        {
            Socket.Close();
            Socket = null;
        }
        private bool CheckConnection(EndPoint endpoint)
        {
            if (!Peers.TryGetValue(endpoint, out Peer peer))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void Recycle(Packet packet)
        {
            pool.RecyclePacket(packet);
        }
        public void ClientAddress()
        {
            Address.Internal = new IPEndPoint(IPAddress.Parse(GetInternalAddress()), ((IPEndPoint)Socket.LocalEndPoint).Port);
            try
            {
                var externalIp = new WebClient().DownloadString("http://ipv4.icanhazip.com").Replace("\n", "");
                Address.External = new IPEndPoint(IPAddress.Parse(externalIp), localAddress.Port);
            }
            catch
            {

            }
        }
        private string GetInternalAddress()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }
        public void FragmentCheck(Packet packet)
        {
            if (packet.CurrentIndex <= 1256 || packet.Protocol == Protocol.Sequenced)
            {
                fragments.Enqueue(packet);
                return;
            }
            var needed = (int)Math.Ceiling((double)(packet.PayLoad.Length - Packet.HeaderSize) / 254);
            var totalPayload = (packet.PayLoad.Length - Packet.HeaderSize);
            var endAmount = totalPayload - ((needed - 1) * 254);
            for (int i = 0; i < needed; i++)
            {
                var lastFrag = i + 1 == needed;
                var frag = CreatePacket();
                frag.Protocol = packet.Protocol;
                frag.Flag = packet.Flag;
                var copyStartIndex = (i * 254 + Packet.HeaderSize);
                var copyLength = lastFrag ? endAmount : 254;
                frag.CurrentIndex = lastFrag ? copyLength + Packet.HeaderSize : 256;
                try
                {
                    Buffer.BlockCopy(packet.PayLoad, copyStartIndex, frag.PayLoad, Packet.HeaderSize, copyLength);
                }
                catch (Exception e)
                {
                }
                frag.Fragmented = lastFrag ? Fragment.End : Fragment.Begin;
                fragments.Enqueue(frag);
            }
        }
    }
}
