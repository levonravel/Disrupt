using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavelNet
{
    public class MethodTracker
    {
        private Reader read = new Reader();
        private Dictionary<string, int> methodGroups = new Dictionary<string, int>();
        private Dictionary<int, Action<Packet, Peer>> actionGroups = new Dictionary<int, Action<Packet, Peer>>();

        public void AddMethods(params Action<Packet, Peer>[] methods)
        {
            for (byte i = 0; i < methods.Length; i++)
            {
                methodGroups.Add(methods[i].Method.Name, i);
                actionGroups.Add(i, methods[i]);
            }
        }
        public void ReleasePacket(Packet packet, Peer peer)
        {
            var methodId = read.Int(packet);
            actionGroups[methodId].Invoke(packet, peer);
        }
        public int GetMethodId(string method)
        {
            return methodGroups[method];
        }
    }
}
