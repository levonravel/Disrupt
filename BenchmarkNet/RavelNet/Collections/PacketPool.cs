using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavelNet
{
    public class PacketPool
    {
        private Queue<Packet> packets = new Queue<Packet>();
        private readonly object queue = new object();

        public Packet GetPacket()
        {
            lock (queue)
            {
                var packet = packets.Count > 0 ? packets.Dequeue() : new Packet();
                packet.Reset();
                return packet;
            }
        }

        public void PutPacket(Packet packet)
        {
            lock(queue)
            {
                packets.Enqueue(packet);
            }
        }
    }
}
