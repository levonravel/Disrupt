using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RavelTek.Disrupt
{
    public partial class Client
    {
        private void CreateSocket(string appId, int port, string relayAddress = null, int relayPort = 0)
        {
            AppId = appId;
            if (relayAddress != null)
            {
                var relayEndpoint = new IPEndPoint(IPAddress.Parse(relayAddress), relayPort);
                RelayAddress = relayEndpoint;
            }
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            localAddress = new IPEndPoint(IPAddress.Any, port);
            Socket.ReceiveBufferSize = int.MaxValue;
            Socket.SendBufferSize = int.MaxValue;
            Socket.DontFragment = true;
            Socket.EnableBroadcast = true;
            ICMPIgnore();
            Socket.Bind(localAddress);
            ClientAddress();
            Thread networkingThread = new Thread(RunTasks);
            networkingThread.IsBackground = true;
            networkingThread.Start();
        }
        private void RunTasks()
        {
            Receive();
            Ping();
            Process();
        }
        private void Receive()
        {
            Task.Run(() =>
            {
                while (Socket != null)
                {
                    try
                    {
                        var packet = CreatePacket();
                        packet.PayloadLength = Socket.ReceiveFrom(packet.PayLoad, 0, 1256, SocketFlags.None, ref packet.Address);
                        if (packet.PayLoad[0] == 70)
                        {
                            Peers.TryAdd(packet.Address, new Peer((IPEndPoint)packet.Address, this));
                            Process(packet);
                        }
                        else
                        {
                            Process(packet);
                        }
                    }
                    catch
                    {
                    }
                }
            });
        }
        private void Ping()
        {
            var timeout = (PingTimeout * 1000) / 2;
            Task.Run(async () =>
            {
                while (Socket != null)
                {
                    await Task.Delay(timeout);
                    foreach (var peer in Peers.Values)
                    {
                        if (peer.Address.Equals(Address.Internal)) continue;
                        if (!peer.IsConnected)
                        {
                            Peers.TryRemove(peer.Address, out Peer value);
                            RaiseEventDisconnect(peer.Address);
                            continue;
                        }
                        peer.Ping();
                    }
                }
            });
        }
        private void ICMPIgnore()
        {
            try
            {
                var iocIn = 0x80000000;
                var iocVendor = 0x18000000;
#pragma warning disable CS0675
                var sioUdpConnreset = iocIn | iocVendor | 12;
#pragma warning restore CS0675
                Socket.IOControl((int)sioUdpConnreset, new[] { Convert.ToByte(false) }, null);
            }
            catch
            {
                Console.WriteLine("ICMP Request fix not applicable with System");
            }
        }
    }
}
