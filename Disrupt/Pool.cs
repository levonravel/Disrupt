using System.Collections.Generic;

namespace RavelNet
{
    public class Pool
    {
        private readonly Queue<Packet> pool = new Queue<Packet>();
        private static readonly object poolInstance = new object();
        private Packet packet;
        public Packet GetPacket()
        {
            lock (poolInstance)
            {
                if (pool.Count > 0)
                {
                    packet = pool.Dequeue();
                }
                else
                {
                    packet = new Packet();
                }
                packet.Reset();
                return packet;
            }
        }
        public void ReturnPacket(Packet packet)
        {
            lock (poolInstance)
            {
                pool.Enqueue(packet);
            }
        }
    }
}
