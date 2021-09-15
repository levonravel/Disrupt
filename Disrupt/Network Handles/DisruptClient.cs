using System.Net;
using System.Net.Sockets;
using System.Threading;

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
            Exchange = new Exchange(this);
            Events = new Events(this);
            var recvThread = new Thread(Receive);
            recvThread.IsBackground = true;
            recvThread.Start();
        }
        private void Receive()
        {
            while (Socket != null)
            {                
                try
                {
                    if (Socket.Available == 0 && !Socket.Poll(ReceivePollingTime, SelectMode.SelectRead)) continue;
                    var packet = Exchange.CreatePacket();
                    packet.Length = Socket.ReceiveFrom(packet.Payload, 0, 1256, SocketFlags.None, ref packet.Address);
                    if (packet.Flag == Flags.Conn)
                    {                        
                        Events.RaiseEventAddRequest(packet);
                        continue;
                    }
                    Exchange.ReceivePacket(packet);
                }
                catch
                {
                }
            };
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
