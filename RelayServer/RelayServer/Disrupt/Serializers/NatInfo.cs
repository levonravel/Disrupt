using System;
using System.Net;

namespace RavelTek.Disrupt
{
    [Serializable]
    public class NatInfo
    {
        public IPEndPoint External;
        public IPEndPoint Internal;
        public object CustomData;
        public int CurrentConnections;
        public int MaxConnections;        
    }
}
