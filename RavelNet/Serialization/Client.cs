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
        private readonly CommunicationController communicationController;
        private readonly SequencedController sequencedLayer = new SequencedController();
        private readonly ReliableController reliableLayer = new ReliableController();
        private readonly PeerCollection peerCollection = new PeerCollection();

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
            communicationController = new CommunicationController(socket, this);
            var listenerThread = new Thread(Listen)
            {
                IsBackground = true
            };
            listenerThread.Start();
            var outgoingThread = new Thread(OutgoingLoop)
            {
                IsBackground = true
            };
            outgoingThread.Start();
        }
        private void Listen()
        {
            OnOutboundConnection += Events_OnOutboundConnection;
            OnDisconnect += Events_OnDisconnect;
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
                    PreprocessPacket(packet);
                }
                catch (Exception e)
                {
                    throw new Exception(e.ToString() + " line 55 in method Listener, class Listener.cs");
                }
            };
        }
        public void PreprocessPacket(Packet packet)
        {
            if (packet.Flag == Flags.Con)
            {
                TryAddPeer(packet.Address);
            }
            Peer peer = peerCollection.GetPeer(packet.Address);
            peer.Enqueue(packet, packet.Protocol, TransportLayer.Inbound);
        }
        public void Poll(string clientName)
        {
            foreach (var peer in peerCollection.GetPeers)
            {
                var result = reliableLayer.TryReceive(peer);
                if (result != null)
                {
                    communicationController.Acknowledge(peer);
                    //update peers receive buffer
                    reliableLayer.UpdateReceiverLowerBound(peer);
                }
                Receive(result, peer);
                result = sequencedLayer.TryReceive(peer);
                //this is a UPD packet contains Acked information
                if (result == null) continue;
                if (result.Flag == Flags.UPD)
                {
                    communicationController.Confirmation(result, peer);
                    continue;
                }
                Receive(result, peer);
            }
        }
        private void OutgoingLoop()
        {
            Task.Run(async () =>
            {
                while (IsAlive)
                {
                    await Task.Delay(1);
                    try
                    {
                        foreach (var peer in peerCollection.GetPeers)
                        {
                            communicationController.TrySend(peer);
                        }
                    }catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }
        private void Events_OnDisconnect(EndPoint address)
        {
            peerCollection.Remove(address);
        }
        private void Events_OnOutboundConnection(string address, int port)
        {
            var destination = new IPEndPoint(IPAddress.Parse(address), port);
            var peer = peerCollection.Add(destination);
            var packet = new Packet();
            packet.Flag = Flags.Con;
            packet.Address = destination;
            peer.Enqueue(packet, Protocol.Sequenced, TransportLayer.Outbound);
        }
        private void TryAddPeer(EndPoint address)
        {
            peerCollection.Add(address);
        }
        public void Dispose()
        {
            socket.Dispose();
            socket = null;
            IsAlive = false;
        }
    }
}
