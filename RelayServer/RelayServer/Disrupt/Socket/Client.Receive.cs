using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RavelTek.Disrupt
{
    public partial class Client
    {
        public void Process(Packet packet)
        {
            lock (inbound) inbound.Enqueue(packet);
        }
        private void Process()
        {
            Task.Run(async () =>
            {
                while (Socket != null)
                {
                    await Task.Delay(1);/* ProcessAsync(() => inbound.Count > 0);*/
                    Packet packet = null;
                    if (inbound.Count > 0)
                    {
                        while (inbound.Count != 0)
                        {
                            lock (inbound) packet = inbound.Dequeue();
                            try
                            {
                                Peers[packet.Address].Receive(packet);
                            }
                            catch
                            {
                                Recycle(packet);
                            }
                        }
                    }
                    //Process Send Packets
                    while (SendQueue.Count > 0)
                    {
                        try
                        {
                            lock (sendLock) packet = SendQueue.Dequeue();
                            if (packet.SingleSend)
                            {
                                SendTo(packet);
                            }
                            else
                            {
                                Broadcast(packet);
                            }
                        }catch(Exception e)
                        {
                            Console.WriteLine("Did fail at line 52 of Client.Recieve..");
                            Console.WriteLine(e);
                        }
                    }
                    //Send Packets
                    try
                    {
                        foreach (var peer in Peers.Values)
                        {
                            if (DateTime.Now.Subtract(peer.LastCheckedRTT).TotalMilliseconds > peer.RTT)
                            {
                                peer.LastCheckedRTT = DateTime.Now;
                                peer.TrySend();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }
        private async Task ProcessAsync(Func<bool> func)
        {
            while (!func())
            {
                await Task.Delay(1);
            }
        }
    }
}
