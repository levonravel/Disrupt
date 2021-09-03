using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RavelTek.Disrupt
{
    [Serializable]
    public class MethodInformation
    {
        public string Name;
        public SerializableMethodInfo Method;
        public Action<NetHelper, object[]> Action;
        public NetHelper Class;
        public Protocol Protocol = Protocol.Reliable;
        public void InitializeAction()
		{
			Action = DelegateBuilder.BuildDelegate<Action<NetHelper, object[]>>(Method.methodInfo); 
		}
    }
}
