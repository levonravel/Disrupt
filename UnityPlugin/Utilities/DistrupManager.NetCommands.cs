using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RavelTek.Disrupt.Custom_Serializers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RavelTek.Disrupt
{
    public partial class DisruptManager
    {
        public void SendInstantiate(Vector3 position, Vector3 rotation, object[] data, Peer peer, int prefabId, ushort ownerId)
        {
            if (peer == null)
                Sync("NetInstantiate")
                .Add(position)
                .Add(rotation)
                .Add(data)
                .Add(prefabId)
                .Add(ownerId)
                .Add(DisruptManagement.ObjectId)
                .Send(SendTo.Others);
            else
                Sync("NetInstantiate")
                .Add(position)
                .Add(rotation)
                .Add(data)
                .Add(prefabId)
                .Add(ownerId)
                .Add(DisruptManagement.ObjectId)
                .Send(peer.Address);
            DisruptManagement.ObjectId++;
        }
        public void SendInstantiate(Vector3 position, Vector3 rotation, Peer peer, int prefabId, ushort ownerId)
        {
            if (peer == null)
                Sync("NetInstantiate")
                .Add(position)
                .Add(rotation)
                .Add(new object[0])
                .Add(prefabId)
                .Add(ownerId)
                .Add(DisruptManagement.ObjectId)
                .Send(SendTo.Others);
            else
                Sync("NetInstantiate")
                .Add(position)
                .Add(rotation)
                .Add(new object[0])
                .Add(prefabId)
                .Add(ownerId)
                .Add(DisruptManagement.ObjectId)
                .Send(peer.Address);
            DisruptManagement.ObjectId++;
        }
        public void SendDestroy(NetBridge view, SendTo peers)
        {
            Sync("NetDestroy")
            .Add((ushort)view.OwnerId)
            .Add(view.ObjectId)
            .Send(peers);
        }
        public void SendDestroy(NetBridge view, params Peer[] peers)
        {
            Sync("NetDestroy")
            .Add((ushort)view.OwnerId)
            .Add(view.ObjectId)
            .Send(peers);
        }
        public void SendLocalDiscovery()
        {

        }
        [RD]
        public void NetLocalMatches()
        {

        }
        [RD]
        public void NetInstantiate(Vector3 position, Vector3 rotation, object[] data, int prefabId, ushort ownerId, ushort objectId)
        {           
            var prefab = DisruptManagement.Instance.GetPrefab(prefabId);
            prefab.SetActive(false);
            var igo = Instantiate(prefab, position, Quaternion.Euler(rotation));
            var netView = igo.GetComponent<NetBridge>();
            netView.OwnerId = ownerId;
            netView.ObjectId = objectId;
            netView.SetData (data);
            Disrupt.Manager.AddClassRef(netView);
            igo.SetActive(true);
            prefab.SetActive(true);
        }
        [RD]
        public void NetDestroy(ushort ownerId, ushort objectId)
        {
            Disrupt.Manager.RemoveView(ownerId, objectId);
        }
    }
}
