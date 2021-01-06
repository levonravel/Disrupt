using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RavelTek.Disrupt;
using RavelTek.Disrupt.Serializers;

namespace DisruptUsage
{
    class Program
    {
        static int clientCount;
        static Client host;
        static List<Client> clients = new List<Client>();
        static Reader reader = new Reader();
        static Writer writer = new Writer();

        static void Main(string[] args)
        {
            Console.WriteLine("Press escape to exit at any time, How many clients should be created? (1-10000)");
            while (clientCount == 0)
            {
                var suggestedClients = int.TryParse(Console.ReadLine(), out clientCount);
                if(!suggestedClients)
                {
                    Console.Clear();
                    Console.WriteLine("Press escape to exit at any time, How many clients should be created? (1-10000)");
                }
            }
            for(int i = 0; i < clientCount; i++)
            {
                var client = new Client("test app");
                clients.Add(client);
            }
            Console.Clear();
            Console.WriteLine($"{clientCount} client(s) created.");
            host = new Client("test app", 35005);
            Console.WriteLine("Host created on port 35005");
            RegisterEvents();
            Console.WriteLine("Type as many messages you like, press (enter) send to the host.");
            do
            {                
                var message = Console.ReadLine();
                foreach (var client in clients)
                {
                    var packet = client.CreatePacket();
                    /*
                        this can be done as many times with any reference
                        writer.Push(SomeClass, packet)
                        writer.Push(12345, packet);
                    */
                    writer.Push(message, packet);
                    //if your sending outside LAN should use host.Address.External
                    client.SendTo(packet, Protocol.Reliable, host.Address.Internal);
                }
            } while (Console.ReadKey().Key != ConsoleKey.Escape);
            host.Dispose();
            foreach(var client in clients)
            {
                client.Dispose();
            }
        }
        static void RegisterEvents()
        {
            host.OnConnected += Host_OnConnected;
            host.OnDisconnected += Host_OnDisconnected;
            host.OnIncomingMessage += Host_OnIncomingMessage;
            //only call poll after registering events
            Poll(host);
            foreach(var client in clients)
            {
                client.OnConnected += Client_OnConnected;
                client.OnDisconnected += Client_OnDisconnected;
                client.OnIncomingMessage += Client_OnIncomingMessage;
                Poll(client);
                //Register hosts connection or it will throw an issue when looking for it when sending
                client.Connect(host.Address.Internal);
            }
            Console.WriteLine("Events registered.");
        }

        private static void Client_OnIncomingMessage(Packet packet)
        {
            var message = reader.PullString(packet);
            Console.WriteLine($"{message}, from {packet.Address}");
        }

        private static void Client_OnDisconnected(System.Net.EndPoint endPoint)
        {
            Console.WriteLine($"Client disconnected from {endPoint}");
        }

        private static void Client_OnConnected(Peer peer)
        {
            Console.WriteLine($"Client connected to Host {peer.Address}");
        }

        private static void Host_OnIncomingMessage(Packet packet)
        {
            var message = reader.PullString(packet);
            Console.WriteLine($"{message}, from {packet.Address}");
        }

        private static void Host_OnDisconnected(System.Net.EndPoint endPoint)
        {
            Console.WriteLine($"Host Disconnected from {endPoint}");
        }

        private static void Host_OnConnected(Peer peer)
        {
            Console.WriteLine($"The host received a connection {peer.Address}");
        }
        /// <summary>
        /// You have to poll for your messages 
        /// if you do not then nothing will register in the OnIncomingEvent
        /// the reason for this is the client is threaded and not on the main thread
        /// </summary>
        private static void Poll(Client client)
        {
            Task.Run(async() =>
            {
                while(client.Socket != null)
                {
                    await Task.Delay(1);
                    client.Poll();
                }
            });
        }
    }
}
