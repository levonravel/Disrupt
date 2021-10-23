using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RavelNet
{
    public static class NetworkController
    {
        public static Client CreateClient(string applicationName, int port = 0)
        {
            return new Client(applicationName, port);
        }

    }
}
