using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RavelTek.Disrupt
{
    public class DisruptClient : Hot
    {
        public EndPoint RelayAddress;
        public string AppId;
        public Socket Socket;
        private const int ReceivePollingTime = 500000;

        public DisruptClient(string appId, bool isHost)
        {
            IsHost = isHost;
            AppId = appId;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            InternalAddress = isHost == true ? new IPEndPoint(IPAddress.Any, 35005) : new IPEndPoint(IPAddress.Any, 0);
            ExternalAddress = GetExternalAddress();
            Socket.ReceiveBufferSize = int.MaxValue;
            Socket.SendBufferSize = int.MaxValue;
            Socket.DontFragment = true;
            Socket.EnableBroadcast = true;
            Socket.Bind(InternalAddress);
            ExternalAddress = ((IPEndPoint)Socket.LocalEndPoint);
            Events = new Events(this);
            Exchange = new Exchange(this);            
            var recvThread = new Thread(Receive);
            recvThread.IsBackground = true;
            recvThread.Start();
        }
        private void Receive()
        {
            Ping();
            while (Socket != null)
            {                
                try
                {
                    if (Socket.Available == 0 && !Socket.Poll(ReceivePollingTime, SelectMode.SelectRead)) continue;
                    var packet = Exchange.CreatePacket();
                    packet.Length = Socket.ReceiveFrom(packet.Payload, 0, 512, SocketFlags.None, ref packet.Address);
                    if (packet.Flag == Flags.Conn)
                    {
                        Exchange.Peers.Add(packet.Address, new PeerContainer(this, packet.Address));
                    }
                    Exchange.ReceivePacket(packet);
                }
                catch
                {
                }
            };
        }
        private void Ping()
        {
            Task.Run(async () =>
            {
                while (Socket != null)
                {
                    try
                    {
                        foreach (var client in Exchange.Peers.Values)
                        {
                            client.Acknowledge();
                        }                        
                    }catch(Exception e)
                    {
                        UnityEngine.Debug.Log(e);
                    }
                    await Task.Delay(500);
                }
            });            
        }
        public void Dispose()
        {
            Socket.Dispose();
            Exchange = null;
            Events = null;
            Socket = null;
        }
        public IPEndPoint GetExternalAddress()
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    var externalIp = webClient.DownloadString("http://ipv4.icanhazip.com").Replace("\n", "");
                    return new IPEndPoint(IPAddress.Parse(externalIp), InternalAddress.Port);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
