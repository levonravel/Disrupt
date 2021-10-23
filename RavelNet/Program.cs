using System;
using System.Net;

namespace RavelNet
{
    class Program 
    {
        private static Writer writer = new Writer();
        private static Reader read = new Reader();
        private static Client client;

        static void Main(string[] args)
        {
            client = new Client("test", 0);
            client.TrackMethods(Response, UserInformation);
            RegisterEvents(client);
            client.Connect("127.0.0.1", 35005);    
        }

        static void RegisterEvents(RavelNetEvents events)
        {
            events.OnInboundConnection += Events_OnInboundConnection;
            events.OnDisconnect += Events_OnDisconnect;
        }

        private static void Events_OnDisconnect(EndPoint address)
        {
            throw new NotImplementedException();
        }

        private static void Events_OnInboundConnection(Peer peer)
        {
            var packet = new Packet();
            writer.Open(packet).Add("Hello World");
            client.Send(packet, Protocol.Sequenced, peer, nameof(Response));
        }

        static public void Response(Packet packet, Peer peer)
        {
            var response = read.String(packet);
            Console.WriteLine(response);
        }

        static public void UserInformation(Packet packet, Peer peer)
        {

        }
    }
}
