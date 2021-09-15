using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public class ObjectTracker : MonoBehaviour
    {
        public int Id;
        public string OwnerId;
        public bool HostControlled;
        public bool IsMine
        {
            get
            {
                if(HostControlled)
                {
                    return Manager.Instance.IsHost;
                }
                return Manager.Instance.PlayerId == OwnerId;
            }
        }
        private readonly Dictionary<byte, Dictionary<byte, Action<Packet>>> Methods = new Dictionary<byte, Dictionary<byte, Action<Packet>>>();
        public void Awake()
        {
            NetworkManagement.Instance.Trackers.Add(Id, this);
        }
        public void AddMethod(Action<Packet> method, byte methodId, byte helperId)
        {
            if(Methods.TryGetValue(helperId, out Dictionary<byte, Action<Packet>> action))
            {
                action.Add(methodId, method);
            }
            else
            {
                Methods.Add(helperId, new Dictionary<byte, Action<Packet>> { { methodId, method } });
            }
            
        }
        public Action<Packet> GetMethod(byte helperId, byte methodId)
        {
           return Methods[helperId][methodId];
        }
    }
}
