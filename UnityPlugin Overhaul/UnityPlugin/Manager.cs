using RavelTek.Disrupt.Serializers;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using UnityEngine;
using UnityEditor;

namespace RavelTek.Disrupt
{
    [CreateAssetMenu(fileName = "Manager", menuName = "Disrupt/Manager")]
    public class Manager : ScriptableObject
    {
        private static Manager instance;
        public static Manager Instance
        {
            get
            {
                if (ReferenceEquals(instance, null))
                {
                    instance = Resources.Load<Manager>("Manager");
                }
                return instance;
            }
        }
        public string ApplicationName;
        public string RelayAddress;
        public float NetworkTickRate;        
        public Client NetworkClient;
        public EndPoint Host;
        [HideInInspector]
        public int PlayerId = 0;
        [HideInInspector]
        public bool Offline;
        public Dictionary<GameObject, int> PrefabIndexes = new Dictionary<GameObject, int>();
        public Dictionary<int, GameObject> Prefabs = new Dictionary<int, GameObject>();
        public Dictionary<int, ObjectTracker> Trackers = new Dictionary<int, ObjectTracker>();
        [HideInInspector]
        public int TrackerCount;
        private Timer networkTimer;
        private Reader reader = new Reader();

        public int GetPrefabIndex(GameObject Prefab)
        {
            return PrefabIndexes[Prefab];
        }
        public void AddPrefab(GameObject prefab, int index)
        {
            Prefabs.Add(index, prefab);
            PrefabIndexes.Add(prefab, index);
        }
        public void InvokeNetworkLoop(int port = 0)
        {
            if(NetworkClient != null)
            {
                Dispose();
            }
            NetworkClient = new Client(ApplicationName, port, RelayAddress, 35002);
            networkTimer = new Timer();
            networkTimer.Elapsed += Poll;
            networkTimer.Interval = NetworkTickRate * 1000;
            networkTimer.AutoReset = true;
            networkTimer.Enabled = true;
            NetworkClient.OnIncomingMessage += NetworkClient_OnIncomingMessage;
        }
        private void Poll(object sender, ElapsedEventArgs e)
        {
            NetworkClient.Poll();
        }
        private void NetworkClient_OnIncomingMessage(Packet packet)
        {
            var tracker = reader.PullInt(packet);
            var helper = reader.PullByte(packet);
            var method = reader.PullByte(packet);
            Trackers[tracker].GetMethod(helper, method)(packet);
        }
        public void Dispose()
        {
            networkTimer.Dispose();
            NetworkClient.Dispose();
        }
    }

    public class QuickRepopulate : Editor
    {        
        private static int prefabCount;
        [MenuItem("RavelTek/Disrupt/Repopulate")]
        public static void Repopulate()
        {
            prefabCount = 0;
            Manager.Instance.TrackerCount = 0;
            var trackers = FindObjectsOfType(typeof(ObjectTracker), true) as ObjectTracker[];
            for (int i = 0; i < trackers.Length; i++)
            {
                trackers[i].Id = i;
                var helpers = trackers[i].gameObject.GetComponentsInChildren<NetHelper>(true);
                for (byte x = 0; x < helpers.Length; x++)
                {
                    helpers[x].Id = x;
                }
                Manager.Instance.TrackerCount++;
            }
            Manager.Instance.Prefabs.Clear();
            var assets = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabRoot = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                var view = prefabRoot.GetComponent<ObjectTracker>();
                view.Id = Manager.Instance.TrackerCount;
                Manager.Instance.TrackerCount++;
                if (view == null) continue;
                var helpers = view.gameObject.GetComponentsInChildren<NetHelper>(true);
                for (byte x = 0; x < helpers.Length; x++)
                {
                    helpers[x].Id = x;
                }
                Manager.Instance.AddPrefab(prefabRoot, prefabCount);
                prefabCount++;
            }
        }
    }
}
