using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RavelTek.Disrupt.Serializers;

namespace RavelTek.Disrupt.Network_Utilities
{
    public partial class Network
    {
        public Client client;
        private readonly Reader reader = new Reader();
        private readonly Writer writer = new Writer();

        public Network(Client netClient)
        {
            client = netClient;
        }
    }
}
