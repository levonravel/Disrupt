using Newtonsoft.Json;
using RavelTek.Disrupt.Custom_Serializers;
using RavelTek.Disrupt.Network_Utilities;
using RavelTek.Disrupt.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RavelTek.Disrupt
{
    public partial class Client
    {
        public int Id;
        public Socket Socket;
        public bool IsServer;
        public int PingTimeout = 3;
        public ClientAddress Address = new ClientAddress();
        public ConcurrentDictionary<EndPoint, Peer> Peers = new ConcurrentDictionary<EndPoint, Peer>();
        public Network NetworkUtilities;
        public EndPoint RelayAddress;
        public string AppId;
        public HostManagement HostManager = new HostManagement();
        public float PacketLossPercentage = 0;
        public readonly Queue<Packet> SendQueue = new Queue<Packet>();
        private IPEndPoint localAddress = new IPEndPoint(IPAddress.Any, 0);
        private readonly Pool pool;
        private readonly Writer writer = new Writer();
        private Queue<Packet> fragments = new Queue<Packet>();
        private readonly Queue<Packet> inbound = new Queue<Packet>();
        private readonly object sendLock = new object();
        
        public ICollection<Peer> Connections
        {
            get
            {
                return Peers.Values;
            }
        }
        public Client(string appId, int port = 0, string relayAddress = null, int relayPort = 0)
        {
            JsonSettings.Instance.Add(new AddressConverter());
            JsonSettings.Instance.Add(new IPConverter());
            JsonSettings.Instance.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            CreateSocket(appId, port, relayAddress, relayPort);
            NetworkUtilities = new Network(this);
            pool = new Pool(this);
        }
    }
}
