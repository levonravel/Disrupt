using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using RavelTek.Disrupt;

namespace RelayServer
{
    class Program
    {
        private static Client client = new Client("DRRS", 35002);
        private static int priorWidth;
        private static int width;
        private static string spacement;
        static void Main(string[] args)
        {
            width = (Console.WindowWidth /2) - 26;
            Console.OutputEncoding = Encoding.Unicode;
            StartRelayServer();
            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();
            do
            {
                keyInfo = Console.ReadKey();
            } while (keyInfo == null);
        }
        static void StartRelayServer()
        {
            client.PingTimeout = 3;
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;
            Timer pollTimer = new Timer(1);
            pollTimer.Elapsed += Poll;
            pollTimer.Start();
        }

        private static void Poll(object sender, ElapsedEventArgs e)
        {
            client.Poll();
            width = (Console.WindowWidth / 2) - 26;
            if (priorWidth != width)
            {   
                priorWidth = width;
                Console.Clear();
                Console.SetCursorPosition(0, Console.CursorTop);                
                spacement = "";
                for (int i = 0; i < width; i++)
                {
                    spacement += " ";
                }
                Console.WriteLine(
$"{spacement} ________   __                          __       \n" +
$"{spacement} \\       \\ |__|___________ __ _________/  |_     \n" +
$"{spacement}  |   |\\  \\|  /  ___/  __ \\  |  \\   _ \\   __|    \n" +
$"{spacement}  |   |_\\  \\  \\___ \\|  | \\/  |  /  |_| |  |      \n" +
$"{spacement} /_________/__/____/|__|  |____/|   __/|__| ♣ v20\n" +
$"{spacement}                                |__|");
                Console.WriteLine($"\n           {spacement}External {client.Address.External}");
                Console.WriteLine($"           {spacement}Internal {client.Address.Internal}\n");
            }
        }

        private static void Client_OnDisconnected(System.Net.EndPoint endPoint)
        {
            Console.WriteLine($"Peer disconnected {endPoint}");
            if (client.HostManager.ServerOnlyRemoveHost(endPoint))
            {
                Console.WriteLine($"Host was removed {endPoint}");
            }
        }

        private static void Client_OnConnected(Peer peer)
        {
            Console.WriteLine($"Peer connected {peer.Address}");
        }
    }
}
