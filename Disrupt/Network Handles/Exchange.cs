using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RavelTek.Disrupt
{
    public class Exchange
    {
        public Pool Pool = new Pool();
        public List<EndPoint> Peers = new List<EndPoint>();
        private Socket socket;
        private Reliable reliable;
        private Sequenced sequenced;

        public Exchange(DisruptClient disrupt)
        {
            this.socket = disrupt.Socket;
            reliable = new Reliable(disrupt);
            sequenced = new Sequenced(disrupt);
        }

        public Packet CreatePacket()
        {
            return Pool.CreateObject();
        }
        public void RecyclePacket(Packet packet)
        {
            Pool.RecyclePacket(packet);
        }
        public List<EndPoint> GetPeers()
        {
            return Peers;
        }
        public void Send(Packet packet, Protocol protocol, string destination, int port)
        {
            packet.Protocol = protocol;
            packet.Address = new IPEndPoint(IPAddress.Parse(destination), port);
            SortPacket(packet);
        }
        public void Send(Packet packet, Protocol protocol, EndPoint destination)
        {
            packet.Protocol = protocol;
            packet.Address = destination;
            SortPacket(packet);
        }
        public void Send(Packet packet, Protocol protocol, EndPoint[] destinations)
        {
            packet.Protocol = protocol;
            foreach(var destination in destinations)
            {
                var hardcopy = packet.Clone(CreatePacket());
                hardcopy.Address = destination;
                SortPacket(hardcopy);
            }
        }
        public void Broadcast(Packet packet, Protocol protocol, EndPoint exclusion = null)
        {
            packet.Protocol = protocol;
            foreach (var destination in Peers)
            {
                if (exclusion != null && exclusion.Equals(destination)) continue;
                var hardcopy = packet.Clone(CreatePacket());
                hardcopy.Address = destination;
                SortPacket(hardcopy);
            }
        }
        private void SortPacket(Packet packet)
        {
            packet.Flag = packet.Flag == Flags.None ? Flags.Dat : packet.Flag;
            switch (packet.Protocol)
            {
                case Protocol.Reliable:
                    reliable.PushSendPacket(packet);
                    reliable.SendTicker.Set();
                    break;
                case Protocol.Sequenced:
                    sequenced.PushSendPacket(packet);
                    sequenced.SendTicker.Set();
                    break;
                default:
                    break;
            }
        }
        public void ReceivePacket(Packet packet)
        {
            if (!CheckAllowed(packet)) return;
            switch (packet.Protocol)
            {
                case Protocol.Reliable:
                    reliable.PushRecvPacket(packet);
                    reliable.RecvTicker.Set();
                    break;
                case Protocol.Sequenced:
                    sequenced.PushRecvPacket(packet);
                    sequenced.RecvTicker.Set();
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// FIX ME
        /// Slow should use dictionary
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private bool CheckAllowed(Packet packet)
        {
            if (!Peers.Contains(packet.Address))
            {
                RecyclePacket(packet);
                return false;
            }
            return true;
        }
    }
}
