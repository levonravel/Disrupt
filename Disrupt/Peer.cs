using System.Net;
using System.Net.Sockets;

namespace RavelNet
{
    public class Peer 
    {
        public bool IsConnected;
        private Reliable reliable = new Reliable();
        private Sequenced sequenced = new Sequenced();
        private SendingPipe sendingPipe;
        private Listener listener;
        private Socket socket;
        private EndPoint address;
        private Fragmenter fragmenter = new Fragmenter();

        public Peer(EndPoint address, Socket socket, Listener listener, int sendInterval)
        {
            this.address = address;
            this.socket = socket;
            this.listener = listener;
            sendingPipe = new SendingPipe(socket, address, sendInterval);
            listener.OnInternalReceive += Listener_OnInternalReceive;
            listener.OnPing += Listener_OnPing;
            listener.OnInternalSend += Listener_OnInternalSend;
        }
        private void Listener_OnPing(Packet packet)
        {
            if (!packet.Address.Equals(address)) return;
            //var resendList = reliable.Confirmation(packet);
            sendingPipe.CalculateRtt();
        }
        private void Listener_OnInternalReceive(Packet packet)
        {
            if (!packet.Address.Equals(address)) return;
            switch (packet.Protocol)
            {
                case Protocol.Reliable:
                    var eventPacket = reliable.ReceiveReliable(packet);
                    if (eventPacket == null) return;
                    break;
                case Protocol.Sequenced:
                    eventPacket = sequenced.ReceiveSequenced(packet);
                    if (eventPacket == null) return;
                    break;
            }
            listener.RaiseReceive(packet);
        }

        private void Listener_OnInternalSend(Packet packet)
        {
            if (!packet.Address.Equals(address)) return;
            Packet sendPacket;
            switch (packet.Protocol)
            {
                case Protocol.Reliable:
                    sendPacket = fragmenter.ShouldFragment(packet);
                    if (sendPacket == null) return;
                    break;
                case Protocol.Sequenced:
                    sendPacket = sequenced.PrepSequenced(packet);
                    if (sendPacket == null) return;
                    break;
                default:
                    break;
            }
            sendingPipe.AwaitPacket(packet);
        }
    }
}
