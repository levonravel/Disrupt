using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public class ObjectTracker : MonoBehaviour
    {
        public int Id, OwnerId;
        [HideInInspector]
        public bool IsMine;
        private readonly Dictionary<byte, Dictionary<byte, Action<Packet>>> Methods = new Dictionary<byte, Dictionary<byte, Action<Packet>>>();

        public void AddMethod(Action<Packet> method, byte methodId, byte helperId)
        {
            Methods[helperId].Add(methodId, method);
        }
        public Action<Packet> GetMethod(byte helperId, byte methodId)
        {
            return Methods[helperId][methodId];
        }
    }
}
