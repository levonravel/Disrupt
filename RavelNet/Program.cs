using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RavelNet
{
    class Program 
    {
        private static Writer writer = new Writer();
        private static Reader read = new Reader();
        private static Client client;

        static void Main(string[] args)
        {
            //start server
            var server = new Client("test", 35005);
            RegisterEvents(server);
            ServerStartPolling();


            client = new Client("test", 0);
            client.TrackMethods(Response, UserInformation);
            RegisterEvents(client);
            ClientStartPolling();
            client.Connect("127.0.0.1", 35005);

            while (Console.ReadKey().Key != ConsoleKey.Escape) { Thread.Sleep(100);  };
        }
        static void ClientStartPolling()
        {
            Task.Run(async () =>
            {
                while (client.IsAlive)
                {
                    await Task.Delay(100);
                    client.Poll("client");
                }
            });
        }
        static void ServerStartPolling()
        {
            Task.Run(async () =>
            {
                while (client.IsAlive)
                {
                    await Task.Delay(100);
                    client.Poll("server");
                }
            });
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
