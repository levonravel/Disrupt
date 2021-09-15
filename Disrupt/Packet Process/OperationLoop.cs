using System.Collections.Generic;
using System.Threading;

namespace RavelTek.Disrupt
{
    public class OperationLoop
    {
        public DisruptClient Client;
        public Queue<Packet> RecvPackets = new Queue<Packet>();
        public Queue<Packet> SendPackets = new Queue<Packet>();
        public ManualResetEventSlim RecvTicker = new ManualResetEventSlim(false);
        public ManualResetEventSlim SendTicker = new ManualResetEventSlim(false);
        private static readonly object recv = new object();
        private static readonly object send = new object();

        public void Initiate(DisruptClient client)
        {
            this.Client = client;
            var readThread = new Thread(ReadQueue)
            {
                IsBackground = true
            };
            readThread.Start();
            var sendThread = new Thread(SendQueue)
            {
                IsBackground = true,
            };
            sendThread.Start();
        }
        public void PushRecvPacket(Packet packet)
        {
            lock (recv) RecvPackets.Enqueue(packet);
        }
        public void PushSendPacket(Packet packet)
        {            
            lock (send) SendPackets.Enqueue(packet);
        }
        public void ReadQueue()
        {
            while (Client.Socket != null)
            {
                RecvTicker.Wait();
                for(int i = 0; i < RecvPackets.Count; i++)
                { 
                    lock(recv)RecieveReady(RecvPackets.Dequeue());
                }
                RecvTicker.Reset();
            }
        }
        public void SendQueue()
        {
            while (Client.Socket != null)
            {
                SendTicker.Wait();
                for(int i = 0; i < SendPackets.Count; i++)
                { 
                    lock (send) SendReady(SendPackets.Dequeue());
                }
                SendTicker.Reset();
            }
        }

        public virtual void RecieveReady(Packet packet) { }
        public virtual void SendReady(Packet packet) { }
    }
}
