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
using System.Threading.Tasks;

namespace RavelNet
{
    public class Client : RavelNetEvents
    {
        public bool IsAlive;
        private IPEndPoint address;
        private Socket socket;
        private readonly ProcessingController processingController;

        public Client(string applicationName, int port)
        {
            IsAlive = true;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            address = new IPEndPoint(IPAddress.Any, port);
            socket.ReceiveBufferSize = int.MaxValue;
            socket.SendBufferSize = int.MaxValue;
            socket.DontFragment = true;
            socket.EnableBroadcast = true;
            socket.Bind(address);            
            processingController = new ProcessingController(this, socket);
            var listenerThread = new Thread(Listen)
            {
                IsBackground = true
            };
            listenerThread.Start();
        }
        private void Listen()
        {            
            while (IsAlive)
            {
                try
                {
                    var packet = new Packet();
                    packet.Length = socket.ReceiveFrom(packet.Payload, 0, 512, SocketFlags.None, ref packet.Address);
                    if(packet.Flag != Flags.UPD)
                    {
                        //Adjust for method packed 
                        packet.CurrentIndex = 3;
                    }
                    processingController.PreprocessPacket(packet);
                }
                catch (Exception e)
                {
                    throw new Exception(e.ToString() + " line 55 in method Listener, class Listener.cs");
                }
            };
        }
        public void Poll()
        {
            processingController.Poll();
        }
        public void Dispose()
        {
            socket.Dispose();
            socket = null;
            IsAlive = false;
        }
    }
}
