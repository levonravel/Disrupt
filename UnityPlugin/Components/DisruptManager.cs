using Newtonsoft.Json;
using RavelTek.Disrupt.Custom_Serializers;
using RavelTek.Disrupt.Serializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RavelTek.Disrupt
{
    [RequireComponent (typeof(NetBridge))]
    public partial class DisruptManager : NetHelper
    {
        public Client Client
        {
            get
            {
                if (object.ReferenceEquals(client, null))
                    CreateClient();
                return client;
            }
        }

        public Peer Peer;
        private Network NetType = Network.Lan;
        public EndPoint HostIp;
        public bool P2P { get; set; }
        public bool Offline { get; set; }
        private readonly List<NatInfo> DiscoveryList = new List<NatInfo>();
        private Client client;
        private readonly Reader reader = new Reader();
        public List<NetHelper> LoopedComponents { get; set; } = new List<NetHelper>();
        public Dictionary<int, Dictionary<ushort, NetBridge>> ClassInvocations = 
            new Dictionary<int, Dictionary<ushort, NetBridge>>();
        Coroutine _netLoopRoutine;
        WaitForSeconds _netLoopWait;
        public GameObject[] HiddenObjects
        {
            get
            {
                return this.gameObject.scene.GetRootGameObjects();
            }
        }

        private enum Network
        {
            Lan,
            Wan
        }
        protected void Awake()
        {
            JsonSettings.Instance.Add(new Vector3Serializer());
            JsonSettings.Instance.Add(new QuaternionSerializer());
            Disrupt.Offline = Offline;
            CreateClient();
            UnityEngine.Debug.Log($"Client created IP is {Client.Address.Internal}");
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            _netLoopWait = new WaitForSeconds(DisruptManagement.Instance.TickRate);
            _netLoopRoutine = StartCoroutine(NetLoopRoutine());
        }

        void OnDestroy()
        {
            if (_netLoopRoutine != null)
                StopCoroutine(_netLoopRoutine);
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (FindObjectsOfType<DisruptManager>().Length > 1)
            {
                Destroy(this);
                return;
            }
        }
        public void AddClassRef(NetBridge view)
        {
            if(!ClassInvocations.ContainsKey(view.OwnerId))
            {
                ClassInvocations.Add(view.OwnerId, new Dictionary<ushort, NetBridge>());
            }
            if (ClassInvocations[view.OwnerId].ContainsKey(view.ObjectId)) return;
            ClassInvocations[view.OwnerId].Add(view.ObjectId, view);
        }
        public void RemoveView(ushort ownerId, ushort objectId)
        {
            if (!ClassInvocations.ContainsKey(ownerId))
            {
                //Debug.LogError($"Class invocations does not contain {ownerId}");
                return;
            }
            if (!ClassInvocations[ownerId].ContainsKey(objectId))
            {
                //Debug.LogError($"Class invocations {ownerId} does not contain {objectId}");
                return;
            }
            for(int i = LoopedComponents.Count -1; i >= 0; i--)
            {
                var __component = LoopedComponents[i];

                if (__component == null)
                {
                    LoopedComponents.Remove(__component);
                }
                else if (__component.View.OwnerId == ownerId && __component.View.ObjectId == objectId)
                {
                    LoopedComponents.Remove(__component);
                    break;
                }
            }
            Destroy(ClassInvocations[ownerId][objectId].gameObject);
            ClassInvocations[ownerId].Remove(objectId);
        }
        private void Update()
        {
            if (Client == null) return;
            Client.Poll();
        }
        public void Events_OnIncomingMessage(Packet packet)
        {
            var ownerId = reader.PullUShort(packet);
            var objectId = reader.PullUShort(packet);
            var methodPointer = reader.PullInt(packet);
            if (!ClassInvocations.ContainsKey(ownerId))
            {
                Debug.LogError($"ClassInvocations missing Owner. Packet Id {packet.Id} | OwnerId: {ownerId}");
                return;
            }
            if (!ClassInvocations[ownerId].ContainsKey(objectId))
            {
                Debug.LogError($"ClassInvocations missing object. Packet Id {packet.Id} | OwnerID: {ownerId} | ObjectId: {objectId} | Latest: {packet.lastUsage}");
                return;
            }
            ClassInvocations[ownerId][objectId].InvokeClass(packet, methodPointer);
        }
        public void OnApplicationQuit()
        {
#if UNITY_EDITOR
            return;
#endif
            Client.Dispose();
        }
        IEnumerator NetLoopRoutine()
        {
            while (true)
            {
                for (int i = LoopedComponents.Count - 1; i >= 0; i--)
                {
                    var component = LoopedComponents[i];
                    if (component == null)
                    {
                        LoopedComponents.RemoveAt(i);
                        continue;
                    }
                    if (!component.ShouldLoop) continue;
                    component.NetworkingLoop();
                }
                yield return _netLoopWait;
            }
        }
    }
}
