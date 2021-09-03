using UnityEngine;
using System.Net;
using RavelTek.Disrupt.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RavelTek.Disrupt {
  public static class Disrupt {
    private static DisruptManager manager;
    public static bool IsHost => Client.IsServer;
    public static bool IsReady;
    private static readonly Writer writer = new Writer();
    private static bool offline = false;
    public static DisruptManager Manager {
      get {
        if (object.ReferenceEquals(manager, null)) {
          manager = GameObject.FindObjectOfType<DisruptManager>();
          if (object.ReferenceEquals(manager, null))
            throw new System.Exception("No DisruptManager was found in the scene! Create one by navigating RavelTek/Disrupt/Create Manager");
        }
        return manager;
      }
    }
    public static Client Client {
      get {
        if (object.ReferenceEquals(manager, null)) {
          manager = GameObject.FindObjectOfType<DisruptManager>();
          if (object.ReferenceEquals(manager, null)) {
            throw new System.Exception("No DisruptManager was found in the scene! Create one by navigating RavelTek/Disrupt/Create Manager");
          }
        }
        return manager.Client;
      }
    }
    public static bool Offline {
      get {
        return offline;
      }
      set {
        if (object.ReferenceEquals(manager, null)) {
          manager = GameObject.FindObjectOfType<DisruptManager>();
        }
        manager.Offline = value;
      }
    }
    #region Default Instantiates
    public static GameObject Instantiate(GameObject item) {
      return ProcessInstantiate(item.transform.position, item.transform.rotation, item);
    }
    public static GameObject Instantiate(GameObject item, Vector3 position, Quaternion rotation) {
      return ProcessInstantiate(position, rotation, item);
    }
    #endregion
    #region Specialized Instantiates
    public static GameObject Instantiate(GameObject item, params object[] data) {
      return ProcessInstantiate(item.transform.position, item.transform.rotation, item, null, data);
    }
    public static GameObject Instantiate(GameObject item, Vector3 position, Quaternion rotation, params object[] data) {
      return ProcessInstantiate(position, rotation, item, null, data);
    }
    public static T Instantiate<T>(T item, params object[] data) where T : Component {
      var outItem = ProcessInstantiate(item.transform.position, item.transform.rotation, item.gameObject, null, data);
      return outItem.GetComponent(typeof(T)) as T;
    }
    public static T Instantiate<T>(T item, Vector3 position, Quaternion rotation, params object[] data) where T : Component {
      var outItem = ProcessInstantiate(position, rotation, item.gameObject, null, data);
      return outItem.GetComponent(typeof(T)) as T;
    }
    #endregion
    #region Default Single Send Instantiates
    public static GameObject Instantiate(GameObject item, Peer peer) {
      return ProcessInstantiate(item.transform.position, item.transform.rotation, item, peer, null);
    }
    public static GameObject Instantiate(GameObject item, Vector3 position, Quaternion rotation, Peer peer) {
      return ProcessInstantiate(position, rotation, item, peer, null);
    }
    #endregion
    #region Default Multi Send Instantiates
    public static GameObject Instantiate(GameObject item, Peer[] peers) {
      return ProcessMultiInstantiate(item.transform.position, item.transform.rotation, item, peers, null);
    }
    public static GameObject Instantiate(GameObject item, Vector3 position, Quaternion rotation, Peer[] peers) {
      return ProcessMultiInstantiate(position, rotation, item, peers, null);
    }
    #endregion
    #region Specialized Single Send Instantiates
    public static GameObject Instantiate(GameObject item, Peer peer, params object[] data) {
      return ProcessInstantiate(item.transform.position, item.transform.rotation, item, peer, data);
    }
    public static GameObject Instantiate(GameObject item, Vector3 position, Quaternion rotation, Peer peer, params object[] data) {
      return ProcessInstantiate(position, rotation, item, peer, data);
    }
    #endregion
    #region Specialized MultiSelect Send Instantiates
    public static GameObject Instantiate(GameObject item, Peer[] peers, params object[] data) {
      return ProcessMultiInstantiate(item.transform.position, item.transform.rotation, item, peers, data);
    }
    public static GameObject Instantiate(GameObject item, Vector3 position, Quaternion rotation, Peer[] peers, params object[] data) {
      return ProcessMultiInstantiate(position, rotation, item, peers, data);
    }
    #endregion
    public static void Destroy(NetBridge view, SendTo receivers) {
      Manager.SendDestroy(view, receivers);
    }
    public static void Destroy(NetBridge view, params Peer[] peers) {
      Manager.SendDestroy(view, peers);
    }
    public static void Destroy(GameObject go, params Peer[] peers) {
      var view = go.GetComponent<NetBridge>();
      if (view == null) {
        Debug.LogError($"Cannot call destroy without NetBridge {go.name}");
        return;
      }
      Manager.SendDestroy(view, peers);
    }
    public static void Destroy(GameObject go, SendTo receivers) {
      var view = go.GetComponent<NetBridge>();
      if (view == null) {
        Debug.LogError($"Cannot call destroy without NetBridge {go.name}");
        return;
      }
      Manager.SendDestroy(view, receivers);
    }
    public static void Sync(NetHelper netHelper, string method, SendTo receivers, Queue<NetHelper.Data> data) 
    {
      int pointer;
      if (netHelper.MethodPointers.TryGetValue(method, out pointer)) {
        MethodInformation classInfo;
        if (netHelper.View.Methods.TryGetValue(pointer, out classInfo)) {
          var packet = Client.CreatePacket();
          writer.Push((ushort)netHelper.View.OwnerId, packet);
          writer.Push(netHelper.View.ObjectId, packet);
          writer.Push(pointer, packet);
          packet.lastUsage = method;
          ObjectHelper.PushData(packet, data);
          switch (receivers) {
            case SendTo.All:
              Client.Broadcast(packet, classInfo.Protocol, null);
              break;
            case SendTo.Others:
              Client.Broadcast(packet, classInfo.Protocol, Client.Address.Internal);
              break;
            case SendTo.Server:
              packet.Address = Manager.HostIp;
              Client.SendTo(packet, classInfo.Protocol, Manager.HostIp);
              break;
            default:
              Debug.Log("Disrupt.cs Sync SendTo Failure");
              break;
          }
        }
      }
    }
    public static void Sync(NetHelper netHelper, string method, Peer peer, Queue<NetHelper.Data> data) {
      int pointer;
      if (netHelper.MethodPointers.TryGetValue(method, out pointer)) {
        MethodInformation classInfo;
        if (netHelper.View.Methods.TryGetValue(pointer, out classInfo)) {
          var packet = Client.CreatePacket();
          packet.lastUsage = method;
          writer.Push((ushort)netHelper.View.OwnerId, packet);
          writer.Push(netHelper.View.ObjectId, packet);
          writer.Push(pointer, packet);
          ObjectHelper.PushData(packet, data);
          packet.lastUsage = method;
          Client.SendTo(packet, classInfo.Protocol, peer.Address);
        }
      }
    }

    //push this to a thread via object include parameters.
    public static void Sync(NetHelper netHelper, string method, EndPoint peer, Queue<NetHelper.Data> data) {
      int pointer;
      if (netHelper.MethodPointers.TryGetValue(method, out pointer)) {
        MethodInformation classInfo;
        if (netHelper.View.Methods.TryGetValue(pointer, out classInfo)) {
          var packet = Client.CreatePacket();
          writer.Push((ushort)netHelper.View.OwnerId, packet);
          writer.Push(netHelper.View.ObjectId, packet);
          writer.Push(pointer, packet);
          ObjectHelper.PushData(packet, data);
          packet.lastUsage = method;
          Client.SendTo(packet, classInfo.Protocol, peer);
        }
      }
    }
    public static void Sync(NetHelper netHelper, string method, Peer[] peers, Queue<NetHelper.Data> data) {
      int pointer;
      if (netHelper.MethodPointers.TryGetValue(method, out pointer)) {
        MethodInformation classInfo;
        if (netHelper.View.Methods.TryGetValue(pointer, out classInfo)) {
          foreach (var peer in peers) {
            var packet = Client.CreatePacket();
            writer.Push((ushort)netHelper.View.OwnerId, packet);
            writer.Push(netHelper.View.ObjectId, packet);
            writer.Push(pointer, packet);
            ObjectHelper.PushData(packet, data);
            packet.lastUsage = method;
            Client.SendTo(packet, classInfo.Protocol, peer.Address);
          }
        }
      }
    }
    private static GameObject ProcessInstantiate(Vector3 position, Quaternion rotation, GameObject obj) {
      obj.SetActive(false);
      var igo = GameObject.Instantiate(obj, position, rotation);
      var view = igo.GetComponent<NetBridge>();
      view.OwnerId = Client.Id;
      view.ObjectId = DisruptManagement.ObjectId;
      Manager.AddClassRef(view);
      var prefabId = GetPrefabId(obj);
      Manager.SendInstantiate(position, rotation.eulerAngles, null, prefabId, (ushort)Client.Id);
      obj.SetActive(true);
      igo.SetActive(true);
      return igo;
    }
    private static GameObject ProcessInstantiate(Vector3 position, Quaternion rotation, GameObject obj, Peer peer, params object[] data) {
      if (peer != null) {
        if (peer.Address == Client.Address.Internal) {
          var selfId = GetPrefabId(obj);
          Manager.SendInstantiate(position, rotation.eulerAngles, data, peer, selfId, (ushort)Client.Id);
          return null;
        }
      }
      obj.SetActive(false);
      var igo = GameObject.Instantiate(obj, position, rotation);
      var view = igo.GetComponent<NetBridge>();
      view.SetData(data);
      view.OwnerId = Client.Id;
      view.ObjectId = DisruptManagement.ObjectId;
      Manager.AddClassRef(view);
      var prefabId = GetPrefabId(obj);
      Manager.SendInstantiate(position, rotation.eulerAngles, data, peer, prefabId, (ushort)Client.Id);
      obj.SetActive(true);
      igo.SetActive(true);
      return igo;
    }
    private static GameObject ProcessMultiInstantiate(Vector3 position, Quaternion rotation, GameObject obj, Peer[] peers, params object[] data) {
      var prefabId = GetPrefabId(obj);
      var objectId = DisruptManagement.ObjectId;
      var clientId = Client.Id;
      foreach (var peer in peers) {
        Manager.SendInstantiate(position, rotation.eulerAngles, data, peer, prefabId, (ushort)clientId);
      }
      obj.SetActive(false);
      var igo = GameObject.Instantiate(obj, position, rotation);
      var view = igo.GetComponent<NetBridge>();
      var s_data = JsonConvert.SerializeObject(data);
      view.SetData(data);
      view.OwnerId = clientId;
      view.ObjectId = DisruptManagement.ObjectId;
      Manager.AddClassRef(view);
      obj.SetActive(true);
      igo.SetActive(true);
      return igo;
    }
    private static int GetPrefabId(GameObject obj) {
      int prefabId = -1;
      for (int i = 0; i < DisruptManagement.Instance.GetPrefabs.Count; i++) {
        if (obj == DisruptManagement.Instance.GetPrefab(i)) {
          prefabId = i;
          break;
        }
      }
      if (prefabId == -1) {
        Debug.LogError($"Trying to instantiate {obj.name} but your networked prefabs does not contain it. Please ensure you press Populate Prefabs before pressing Play or compiling.");
      }
      return prefabId;
    }
  }
}
