using System;
using UnityEngine;
using RavelTek.Disrupt.Serializers;
using System.Net;
using System.Collections.Generic;

namespace RavelTek.Disrupt
{
    [Serializable]
    public class NetHelper : MonoBehaviour
    {
        private ObjectTracker tracker;
        public ObjectTracker Tracker
        {
            get
            {
                if (ReferenceEquals(tracker, null))
                {
                    tracker = GetComponentInParent<ObjectTracker>();
                }
                if(tracker == null)
                {
                    throw new Exception("Cannot find an object tracker on the root of the gameobject, this will cause networking issues.");
                }
                return tracker;
            }
        }
        public Writer Writer = new Writer();
        public Reader Reader = new Reader();
        public Dictionary<string, byte> methodGroups = new Dictionary<string, byte>();
        public byte Id;

        public void AddMethodToTicker(Action method)
        {
            NetworkManagement.Instance.TickerMethods.Add(method);
        }
        public void AddMethods(params Action<Packet>[] methods)
        {
            for(byte i = 0; i < methods.Length; i++)
            {
                Tracker.AddMethod(methods[i], i, Id);
                methodGroups.Add(methods[i].Method.Name, i);
            }
            Tracker.AddMethod(RecvInstantiate, (byte)methods.Length, Id);
            methodGroups.Add("RecvInstantiate", (byte)methods.Length);
        }
        public Packet CreatePacket()
        {
            var packet = Manager.Instance.NetworkClient.Exchange.CreatePacket();
            Writer.Open(packet)
                .Add(Tracker.Id)
                .Add(Id);
            packet.CurrentIndex += 1; //accounts for the helper id set later in send method
            return packet;
        }
        public GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, Enums.SendType destination)
        {
            Manager.Instance.TrackerCount++;
            prefab.GetComponent<ObjectTracker>().Id = Manager.Instance.TrackerCount;
            var packet = CreatePacket();
            Writer.Open(packet)
                .Add(Manager.Instance.GetPrefabIndex(prefab))
                .Add(position)
                .Add(rotation);
            Send(nameof(RecvInstantiate), destination, packet);
            return Instantiate(prefab, position, rotation);
        }
        public GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, EndPoint destination)
        {
            Manager.Instance.TrackerCount++;
            prefab.GetComponent<ObjectTracker>().Id = Manager.Instance.TrackerCount;
            var packet = CreatePacket();
            Writer.Open(packet)
                .Add(Tracker.OwnerId)
                .Add(Manager.Instance.GetPrefabIndex(prefab))
                .Add(position)
                .Add(rotation);
            Send(nameof(RecvInstantiate), destination, packet);
            return Instantiate(prefab, position, rotation);
        }
        public GameObject NetInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, EndPoint[] destination)
        {
            Manager.Instance.TrackerCount++;
            prefab.GetComponent<ObjectTracker>().Id = Manager.Instance.TrackerCount;
            var packet = CreatePacket();
            Writer.Open(packet)
                .Add(Manager.Instance.GetPrefabIndex(prefab))
                .Add(position)
                .Add(rotation);
            Send(nameof(RecvInstantiate), destination, packet);
            return Instantiate(prefab, position, rotation);
        }
        /// <summary>
        /// Do not use this method its for interal receives
        /// If you need to instantiate use NetInstantiate
        /// </summary>
        /// <param name="packet"></param>
        public void RecvInstantiate(Packet packet)
        {
            var ownerId = Reader.PullString(packet);
            var prefab = Manager.Instance.Prefabs[Reader.PullInt(packet)];
            var position = Reader.PullObject<Vector3>(packet);
            var rotation = Reader.PullObject<Quaternion>(packet);
            var go = Instantiate(prefab, position, rotation);
            go.GetComponent<ObjectTracker>().OwnerId = ownerId;
        }
        public void Send(string method, Enums.SendType destination, Packet packet, Protocol protocol = Protocol.Reliable)
        {
            packet.Payload[8] = methodGroups[method];
            switch (destination)
            {
                case Enums.SendType.All:
                    Manager.Instance.NetworkClient.Exchange.Broadcast(packet, protocol, null);
                    break;
                case Enums.SendType.Others:
                    Manager.Instance.NetworkClient.Exchange.Broadcast(packet, protocol, Manager.Instance.NetworkClient.InternalAddress);
                    break;
                case Enums.SendType.Host:
                    Manager.Instance.NetworkClient.Exchange.Send(packet, protocol, Manager.Instance.Host);
                    break;
                default:
                    break;
            }
        }
        public void Send(string method, EndPoint destination, Packet packet, Protocol protocol = Protocol.Reliable)
        {
            packet.Payload[8] = methodGroups[method];
            Manager.Instance.NetworkClient.Exchange.Send(packet, protocol, destination);
        }
        public void Send(string method, EndPoint[] destination, Packet packet, Protocol protocol = Protocol.Reliable)
        {
            packet.Payload[8] = methodGroups[method];
            Manager.Instance.NetworkClient.Exchange.Send(packet, protocol, destination);
        }
        public void RequestConnection(string destination, int port)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(destination), port);
            Manager.Instance.NetworkClient.RequestAdd(endpoint);
        }
        public void RequestConnection(EndPoint destination)
        {
            Manager.Instance.NetworkClient.RequestAdd(destination);
        }
        public void AddRequest(EndPoint destination)
        {
            Manager.Instance.NetworkClient.AddConnection(destination);
        }
        public void DenyRequest()
        {

        }
        public DisruptClient CreateClient(bool isHost)
        {
            return NetworkManagement.Instance.InvokeNetworkLoop(isHost);
        }
    }
}
