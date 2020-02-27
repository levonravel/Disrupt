using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public partial class Client
    {
        public delegate void HostConnected();
        public event HostConnected OnHostConnected;

        public void RaiseHostConnected()
        {
            if (OnHostConnected == null) return;
            OnHostConnected();
        }
    }
}
