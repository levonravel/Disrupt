using System.Collections.Generic;

namespace RavelTek.Disrupt
{
    public class Pool
    {
        private readonly Queue<Packet> pool = new Queue<Packet>();
        private static readonly object poolInstance = new object();
        private Packet packet;
        public Packet CreateObject()
        {
            lock (poolInstance)
            {                
                if (pool.Count > 0)
                {
                    packet = pool.Dequeue();
                    packet.Reset();
                }
                else
                {
                    packet = new Packet();
                }
                return packet;
            }
        }
        public void RecyclePacket(Packet packet)
        {
            lock (poolInstance)
            {
                pool.Enqueue(packet);
            }
        }
    }
}
