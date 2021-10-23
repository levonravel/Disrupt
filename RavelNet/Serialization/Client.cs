/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      RavelNetEvents
 *      CommunicationController
 *
 *Class Information
 *      Creates a socket that pulls data from the wire and uses the CommunicationController to publish packets
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RavelNet
{
    public class Client : RavelNetEvents
    {
        public IPEndPoint Address;
        private Socket socket;
        private ProcessPacketController packetProcessor;

        public Client(string applicationName, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Address = new IPEndPoint(IPAddress.Any, port);
            socket.ReceiveBufferSize = int.MaxValue;
            socket.SendBufferSize = int.MaxValue;
            socket.DontFragment = true;
            socket.EnableBroadcast = true;
            socket.Bind(Address);
            packetProcessor = new ProcessPacketController(socket, new CommunicationController(socket, this), new PeerCollection());
            var listenerThread = new Thread(Listen)
            {
                IsBackground = true
            };
            listenerThread.Start();
        }
        private void Listen()
        {
            while (socket != null)
            {
                try
                {
                    var packet = new Packet();
                    packet.Length = socket.ReceiveFrom(packet.Payload, 0, 512, SocketFlags.None, ref packet.Address);
                    packetProcessor.PreprocessPacket(packet);
                }
                catch (Exception e)
                {
                    throw new Exception(e.ToString() + " line 40 in method Listener, class Listener.cs");
                }
            };
        }
        public void Dispose()
        {
            socket.Dispose();
            socket = null;
        }
    }
}
