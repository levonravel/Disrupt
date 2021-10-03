using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RavelNet
{
    public class SendingPipe
    {
        private Socket socket;
        private EndPoint address;
        private int waitTime, packetRate;
        private double currentRtt;
        private DateTime pingTime = DateTime.UtcNow;
        private Queue<Packet> awaited = new Queue<Packet>();
        private static readonly object queue = new object();

        public SendingPipe(Socket socket, EndPoint address, int packetRate)
        {
            this.socket = socket;
            this.address = address;
            this.packetRate = packetRate;
            TrySend();
            Ping();
        }
        public void CalculateRtt()
        {
            currentRtt = (DateTime.UtcNow - pingTime).TotalMilliseconds;
            waitTime = (int)currentRtt / packetRate;
        }
        public void AwaitPacket(Packet packet)
        {
            lock(queue)awaited.Enqueue(packet);
        }
        public void TrySend()
        {
            Task.Run(async() =>
            {
                while (socket != null)
                {
                    while (awaited.Count != 0)
                    {
                        lock (queue) Send(awaited.Dequeue());
                    }
                    await Task.Delay(1);
                }
            });
        }
        public void Ping()
        {
            Task.Run(async () =>
            {
                while (socket != null)
                {
                    await Task.Delay(1000);
                    var ping = new Packet();
                    ping.Flag = Flags.UPD;
                    Send(ping);                    
                }
            });
        }
        private void Send(Packet packet)
        {
            socket.SendTo(packet.Payload, packet.CurrentIndex, SocketFlags.None, address);
        }
    }
}
