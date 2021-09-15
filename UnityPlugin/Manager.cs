using RavelTek.Disrupt.Serializers;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using UnityEngine;
using UnityEditor;
using System;

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
        public DisruptClient NetworkClient;
        public EndPoint Host;
        [HideInInspector]
        public string PlayerId;
        [HideInInspector]
        public bool Offline, IsHost;
        public SerializableDictionary<GameObject, int> PrefabIndexes = new SerializableDictionary<GameObject, int>();
        public SerializableDictionary<int, GameObject> Prefabs = new SerializableDictionary<int, GameObject>();
        [HideInInspector]
        public int TrackerCount;

        public int GetPrefabIndex(GameObject Prefab)
        {
            return PrefabIndexes[Prefab];
        }
        public void AddPrefab(GameObject prefab, int index)
        {
            Prefabs.Add(index, prefab);
            PrefabIndexes.Add(prefab, index);
        }
        public string GetPlayerId()
        {
            var ticks = new DateTime(2016, 1, 1).Ticks;
            var ans = DateTime.Now.Ticks - ticks;
            return ans.ToString("x");
        }
    }
}

