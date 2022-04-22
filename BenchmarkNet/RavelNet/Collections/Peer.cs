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
        public byte RecievedReliableId = 0;
        public int SentReliableId;
        public int ReceivedBitfield;
        public bool IsConnected;
        public bool ResetSentId;
        public Packet[] SentPackets = new Packet[32];
        public Queue<Packet> FragmentPackets = new Queue<Packet>();
        private byte sequencedOutIndex, sequencedInIndex, reliableOutIndex;
        private const byte maxIndex = 32;
        private Queue<Packet> awaitingOutbound = new Queue<Packet>();
        public DateTime LastSeenPing = DateTime.UtcNow;
        private double testScale = 1;

        public bool GoodRTT()
        {
            var lastPingTime = (DateTime.UtcNow - LastSeenPing).TotalMinutes;
            return lastPingTime <= testScale;
        }

        //TODO create a method to check if the network is in good condition if its bad dont send or send less.
        public Peer(EndPoint address)
        {
            Address = address;
        }

        public int AwaitingPackets
        {
            get
            {
                return awaitingOutbound.Count;
            }            
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
        public Packet Dequeue()
        {
            return awaitingOutbound.Count > 0 ? awaitingOutbound.Dequeue() : null;
        }
        public void Enqueue(Packet packet)
        {
            awaitingOutbound.Enqueue(packet);
        }
    }
}
