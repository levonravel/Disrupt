using System;
using System.Collections.Generic;
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
        private static Client server;

        static void Main(string[] args)
        {
            //start server
            server = new Client("test", 35005);
            RegisterEvents(server);
            ServerStartPolling();

            //start a client
            client = new Client("test", 0);
            client.TrackMethods(Response);
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
                    await Task.Delay(1);
                    client.Poll();
                }
            });
        }
        static void ServerStartPolling()
        {
            Task.Run(async () =>
            {
                while (client.IsAlive)
                {
                    await Task.Delay(1);
                    server.Poll();
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
            for (int i = 0; i < 1000; i++)
            {
                var packet = new Packet();
                writer.Open(packet).Add(i);
                client.Send(packet, Protocol.Reliable, peer, nameof(Response));
            }
        }

        static public void Response(Packet packet, Peer peer)
        {
            var response = read.Int(packet);
            Console.WriteLine(response);
        }
    }
}