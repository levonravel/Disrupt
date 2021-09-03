using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace RavelTek.Disrupt
{
    [CreateAssetMenu(fileName = "DisruptManagement", menuName = "Rtek/Disrupt/Resources")]
    public class DisruptManagement : ScriptableObject
    {
        private static DisruptManagement instance;
        public string AppId;
        public float TickRate = .25f;
        public int PingTimeout = 3;
        [HideInInspector][SerializeField]
        public ushort objectId;
        public string relayServer;
        public const int RelayPort = 35002;
        [HideInInspector][SerializeField]
        public List<GameObject> Prefabs = new List<GameObject>();
        public static string GetAppId => Instance.AppId;
        public static string RelayServer
        {
            get
            {
                return Instance.relayServer;
            }
            set
            {
                if (Instance.relayServer == value) return;               
                var __client = Disrupt.Client;
                __client.Disconnect(__client.RelayAddress);
                Instance.relayServer = value;
                var __relayEndpoint = new IPEndPoint(IPAddress.Parse(value), RelayPort);
                __client.RelayAddress = __relayEndpoint;
                __client.Connect(value, RelayPort);
            }
        }
        public static int GetPingTimeout => Instance.PingTimeout;
        public static ushort ObjectId
        {
            get
            {
                return Instance.objectId;
            }
            set
            {
                Instance.objectId = value;
            }
        }
        public List<GameObject> GetPrefabs
        {
            get
            {
                return Prefabs;
            }
        }
        public void InsertPrefab(GameObject prefab)
        {
            Prefabs.Add(prefab);
        }
        public GameObject GetPrefab(int index)
        {
            return Prefabs[index];
        }
        public void ClearPrefabs()
        {
            Prefabs.Clear();
        }
        public static DisruptManagement Instance
        {
            get
            {
                if(instance == null)
                    instance = Resources.Load<DisruptManagement>("DisruptManagement");

                return instance;
            }
        }
    }
}