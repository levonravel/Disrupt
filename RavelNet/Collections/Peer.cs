/*
 * All rights reserved by RavelTek / Levon Marcus Ravel 2012 - Present
 *
 *Dependencies
 *      Packet
 *
 *Class Information
 *      Creates a storage container, that holds information to sent over the wire
 */
using System;
using System.Collections.Generic;
using System.Net;

namespace RavelNet
{
    public class Peer 
    {
        public EndPoint Address;
        public int ReceivedBits, ReceivedLowerBound;
        private byte sequencedOutIndex, sequencedInIndex, reliableOutIndex;
        private const byte maxIndex = 31;
        public bool IsConnected;        
        public bool[] ReceivedFlags = new bool[maxIndex];
        public Queue<Packet> FragmentPackets = new Queue<Packet>();
        private Dictionary<Protocol, Queue<Packet>> outbound = new Dictionary<Protocol, Queue<Packet>>()
        { { Protocol.Reliable, new Queue<Packet>()}, {Protocol.Sequenced, new Queue<Packet>()}};
        private Dictionary<Protocol, Queue<Packet>> inbound = new Dictionary<Protocol, Queue<Packet>>()
        { { Protocol.Reliable, new Queue<Packet>()}, {Protocol.Sequenced, new Queue<Packet>()}};
        public Packet[] SendBuffer = new Packet[maxIndex];
        public Packet[] ReceiveBuffer = new Packet[maxIndex];
        private static readonly object collectionLock = new object();
        
        public Peer(EndPoint address)
        {
            Address = address;
        }

        public byte ReliableOutIndex
        {
            get
            {
                reliableOutIndex =(byte)(reliableOutIndex % maxIndex);
                return reliableOutIndex;
            }
            set
            {
                reliableOutIndex = value;
            }
        }
        public byte SequencedOutIndex
        {
            get
            {
                return sequencedOutIndex;
            }
            set
            {
                sequencedOutIndex = value;
            }
        }
        public byte SequencedInIndex
        {
            get
            {
                return sequencedInIndex;
            }
            set
            {
                sequencedInIndex = value;
            }
        }


        public void Enqueue(Packet packet, Protocol protocol, TransportLayer layer)
        {
            packet.Protocol = protocol;
            packet.Address = Address;
            lock (collectionLock)
            {
                Console.WriteLine($"Enqueue protocol {protocol} layer {layer} address {Address}");
                GetCollection(layer)[packet.Protocol].Enqueue(packet);
            }
        }
        public Packet Dequeue(Protocol protocol, TransportLayer layer)
        {            
            lock (collectionLock)
            {
                GetCollection(layer).TryGetValue(protocol, out Queue<Packet> packets);
                if (packets.Count > 0)
                {
                    Console.WriteLine($"Dequeue protocol {protocol} layer {layer}");
                    return packets.Dequeue();
                }
            }
            return null;
        }
        public int Peek(Protocol protocol, TransportLayer layer)
        {
            if (GetCollection(layer)[protocol].Count > 0)
            {
                return inbound[protocol].Peek().Id;
            }
            return -1;
        }
        private Dictionary<Protocol, Queue<Packet>> GetCollection(TransportLayer layer)
        {
            return layer == TransportLayer.Inbound ? inbound : outbound;
        }
    }
}
