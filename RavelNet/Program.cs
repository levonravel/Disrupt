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
                    server.Poll("server");
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

            var infoPacket = new Packet();            
            var userInfo = new UserInformation();
            userInfo.AdditionalInformation = new List<string>() { "Blue", "Blonde", "Italian Food" };
            userInfo.Age = 37;
            userInfo.BirthDate = "5-5-1984";
            userInfo.Country = "USA";
            userInfo.Height = 5.11F;
            userInfo.Name = "SilentCoder";

            writer.Open(infoPacket)
                .Add(userInfo)
                .Add("Top of the day")
                .Add(12345)
                .Add(0.5185F);
            client.Send(infoPacket, Protocol.Sequenced, peer, nameof(UserInformation));
        }

        static public void Response(Packet packet, Peer peer)
        {
            var response = read.String(packet);
            Console.WriteLine(response);
        }

        static public void UserInformation(Packet packet, Peer peer)
        {
            var userInfo = read.Object<UserInformation>(packet);
            var dailyMessage = read.String(packet);
            var randomInt = read.Int(packet);
            var randomFloat = read.Float(packet);

            foreach(var addition in userInfo.AdditionalInformation)
            {
                Console.WriteLine(addition);
            }
            Console.WriteLine($"{userInfo.Age} {userInfo.BirthDate} {userInfo.Country} {userInfo.Height} {userInfo.Name}");
            Console.WriteLine($"{dailyMessage} {randomInt} {randomFloat}");
        }
    }
}