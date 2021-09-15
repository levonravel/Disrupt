using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RavelTek.Disrupt;
using RavelTek.Disrupt.Serializers;
using System;
using UnityEditor;

public class NetworkManagement : MonoBehaviour
{
    private static NetworkManagement instance;
    public static NetworkManagement Instance
    {
        get
        {
            if (ReferenceEquals(instance, null))
            {
                instance = FindObjectOfType<NetworkManagement>();
            }
            return instance;
        }
    }
    public Dictionary<int, ObjectTracker> Trackers = new Dictionary<int, ObjectTracker>();
    public List<Action> TickerMethods = new List<Action>();
    private float currentTickRate;
    private Reader reader = new Reader();

    public DisruptClient InvokeNetworkLoop(bool isHost)
    {
        Manager.Instance.IsHost = isHost;
        if (Manager.Instance.NetworkClient != null)
        {
            Dispose();
        }
        Manager.Instance.NetworkClient = new DisruptClient(Manager.Instance.ApplicationName, isHost);
        Manager.Instance.NetworkClient.Events.OnIncomingMessage += NetworkClient_OnIncomingMessage;
        return Manager.Instance.NetworkClient;
    }

    public void Update()
    {
        if (Manager.Instance.NetworkClient == null) return;
        Manager.Instance.NetworkClient.Events.Poll();
        currentTickRate += Time.deltaTime * 1;
        if(currentTickRate >= Manager.Instance.NetworkTickRate)
        {
            currentTickRate = 0;            
            for(int i = 0; i < TickerMethods.Count; i++)
            {
                TickerMethods[i]();
            }
        }
    }
    private void NetworkClient_OnIncomingMessage(Packet packet)
    {
        var tracker = reader.PullInt(packet);
        var helper = reader.PullByte(packet);
        var method = reader.PullByte(packet);
        var action = Trackers[tracker].GetMethod(helper, method);
        action(packet);
    }
    public void Dispose()
    {       
        Manager.Instance.NetworkClient.Dispose();
    }
}

#if UNITY_EDITOR
public class QuickRepopulate : Editor
{
    private static int prefabCount;
    [MenuItem("RavelTek/Disrupt/Repopulate")]
    public static void Repopulate()
    {
        Manager.Instance.PlayerId = Manager.Instance.GetPlayerId();
        prefabCount = 0;
        Manager.Instance.TrackerCount = 0;
        var trackers = FindObjectsOfType(typeof(ObjectTracker), true) as ObjectTracker[];
        for (int i = 0; i < trackers.Length; i++)
        {
            trackers[i].Id = i;
            trackers[i].OwnerId = Manager.Instance.PlayerId;
            var helpers = trackers[i].gameObject.GetComponentsInChildren<NetHelper>(true);
            for (byte x = 0; x < helpers.Length; x++)
            {
                helpers[x].Id = x;
            }
            Manager.Instance.TrackerCount++;
        }
        Manager.Instance.Prefabs.Clear();
        Manager.Instance.PrefabIndexes.Clear();
        var assets = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in assets)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            var view = prefabRoot.GetComponent<ObjectTracker>();
            view.Id = Manager.Instance.TrackerCount;
            view.OwnerId = Manager.Instance.PlayerId;
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
#endif