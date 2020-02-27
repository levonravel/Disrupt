using System.Collections.Generic;

namespace RavelTek.Disrupt
{
    public class Pool
    {
        private Client _client;
        public Queue<Packet> pool = new Queue<Packet>();
        public static readonly object poolInstance = new object();
        public Pool(Client client)
        {
            _client = client;
        }
        public Packet CreateObject()
        {
            lock (poolInstance)
            {
                Packet packet;

                if (pool.Count > 0)
                {
                    packet = pool.Dequeue();
                    packet.Client = _client;
                    packet.Reset();                   
                } else {
                    packet = new Packet() { Client = _client };
                }

                return packet;
            }
        }
        public void RecyclePacket(Packet packet)
        {
            lock(poolInstance)
                pool.Enqueue(packet);
        }
    }
}
