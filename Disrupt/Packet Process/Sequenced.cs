using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public class Sequenced : OperationLoop
    {
        private byte sendId;
        private byte recvId;
        //Initiate OperationLoop
        public Sequenced(DisruptClient client)
        {
            Initiate(client);
        }
        public override void RecieveReady(Packet packet)
        {
            base.RecieveReady(packet);
            switch (packet.Flag)
            {
                case Flags.PacketUpdate:
                    Client.Events.RaiseEventPacketUpdate(packet);
                    break;
                case Flags.Dat:
                    if (LatestPacket(recvId, packet.Id))
                    {
                        recvId = packet.Id;
                        Client.Events.RaiseEventData(packet);
                    }
                    else
                    {
                        Client.Exchange.RecyclePacket(packet);
                    }
                    break;
                case Flags.Dscvr:
                    Client.Events.RaiseEventDiscovery(packet);
                    break;
                case Flags.HostList:
                    break;
                default:
                    Client.Exchange.RecyclePacket(packet);
                    break;
            }
        }
        public override void SendReady(Packet packet)
        {
            base.SendReady(packet);
            try
            {
                sendId++;
                packet.Id = sendId;
                Client.Socket.SendTo(packet.Payload, packet.CurrentIndex, SocketFlags.None, packet.Address);
                Client.Exchange.RecyclePacket(packet);
            }
            catch
            {

            }
        }
        private bool LatestPacket(int s2, int s1)
        {
            return ((s1 > s2) && (s1 - s2 <= 128)) || ((s1 < s2) && (s2 - s1 > 128));
        }
    }
}
