using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public class Reliable : OperationLoop
    {
        //initiate OperationLoop
        public Reliable(DisruptClient client)
        {
            Initiate(client);
        }
        public override void RecieveReady(Packet packet)
        {
            base.RecieveReady(packet);
        }
        public override void SendReady(Packet packet)
        {
            base.SendReady(packet);
        }
    }
}
