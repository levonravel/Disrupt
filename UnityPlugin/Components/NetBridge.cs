using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using RavelTek.Disrupt.Custom_Serializers;
using RavelTek.Disrupt.Serializers;

namespace RavelTek.Disrupt
{
    [System.Serializable]
    public class NetBridge : MonoBehaviour
    {
        public OwnerType Ownership = OwnerType.SharedView;
        [HideInInspector]
        public int OwnerId;
        [HideInInspector]
        public ushort ObjectId;
        private object[] data;
        [HideInInspector][SerializeField]
        private IntMethodInfoDictionary methodInfoStore = IntMethodInfoDictionary.New<IntMethodInfoDictionary>();
        public Dictionary<int, MethodInformation> Methods { get { return methodInfoStore.dictionary; } }
        bool isDestroyed;

        public void Awake()
        {
            Disrupt.Manager.AddClassRef(this);
			foreach(var method in Methods)
			{
				method.Value.InitializeAction();
			}
        }
        void OnDestroy()
        {
            isDestroyed = true;
        }
        public bool IsMine
        {
            get
            {
                switch (Ownership)
                {
                    case OwnerType.Instantiator:
                        return OwnerId == Disrupt.Client.Id;
                    case OwnerType.Host:
                        return Disrupt.IsHost;
                    case OwnerType.SharedView:
                        return true;
                    default:
                        return false;
                }
            }
        }
        public T GetData<T>(int index)
        {
            try
            {
                return (T)data[index];
            }
            catch
            {
                Debug.LogError($"Invalid Cast {gameObject.name}, OwnerId:{OwnerId} ObjectId:{ObjectId} " +
                    $"View.GetData:{index} DataType:{data[index].GetType()}");
                return default;
            }
        }
        public void SetData(object[] value)
        {
            data = value;
        }
        public void InvokeClass(Packet packet, int id)
        {
            if (isDestroyed) return;
            object[] data = null;
            try
            {
                data = ObjectHelper.PullData(packet);
                Methods[id].Action(Methods[id].Class, data);
                }
            catch (Exception e)
            {
                if(!Methods.ContainsKey(id))
                {
                    Debug.LogError($"No method found with id: {id} in bridge: {name}. Exception thrown: {e}");
                    return;
                }
                
                Debug.LogError($"An exception was thrown while invoking method: {Methods[id].Class} | ID {id} | Exception: {e}");
                var __log = "Data: ";
                for (int i = 0; i < data.Length; i++)
                    __log += (data[i] == null ? "null" : data[i].ToString()) + " | ";

                Debug.LogError(__log);
            }
        }
    }
}
