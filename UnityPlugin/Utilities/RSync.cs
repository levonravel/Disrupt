using System;

namespace RavelTek.Disrupt
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RD : Attribute
    {
        public Protocol Protocol = Protocol.Reliable;
    }
}