using RavelTek.Disrupt.Serializers;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace RavelTek.Disrupt {
  [Serializable]
  public class NetHelper : MonoBehaviour {
    private NetBridge NetBridge;
    [HideInInspector] //keep public, unity removes k/v on start if private
    public StringIntDictionary methodPointerStore = StringIntDictionary.New<StringIntDictionary>();
    public Dictionary<string, int> MethodPointers { get { return methodPointerStore.dictionary; } }
    public NetBridge View {
      get {
        if (object.ReferenceEquals(NetBridge, null)) {
          NetBridge = transform.root.GetComponent<NetBridge>();
#if UNITY_EDITOR
          if (object.ReferenceEquals(NetBridge, null)) {
            Debug.LogError($"Missing a NetBridge for object {gameObject.name}. Please place a NetBridge on {transform.root.name}.");
          }
#endif
        }
        return NetBridge;
      }
      set {
        NetBridge = value;
      }
    }
    string _method = "";
    readonly Queue<Data> _d = new Queue<Data>();

    public enum DataType {
        Boolean,
        BooleanA,
        SByte,
        SByteA,
        Byte,
        ByteA,
        Char,
        CharA,
        UInt16,
        UInt16A,
        Int16,
        Int16A,
        UInt32,
        UInt32A,
        Int32,
        Int32A,
        Single,
        SingleA,
        UInt64,
        UInt64A,
        Int64,
        Int64A,
        Decimal,
        DecimalA,
        Double,
        DoubleA,
        String,
        StringA,
        Object,
        ObjectA,
        Vector3,
        Vector3A,
        Quaternion,
        QuaternionA
      }

    public struct Data 
    {
      public DataType dataType;
      public Ts val;
    }
    public struct Ts 
    {
      public bool Boolean;
      public bool[] BooleanA;
      public sbyte SByte;
      public sbyte[] SByteA;
      public byte Byte;
      public byte[] ByteA;
      public char Char;
      public char[] CharA;
      public ushort UInt16;
      public ushort[] UInt16A;
      public short Int16;
      public short[] Int16A;
      public uint UInt32;
      public uint[] UInt32A;
      public int Int32;
      public int[] Int32A;
      public float Single;
      public float[] SingleA;
      public ulong UInt64;
      public ulong[] UInt64A;
      public long Int64;
      public long[] Int64A;
      public decimal Decimal;
      public decimal[] DecimalA;
      public double Double;
      public double[] DoubleA;
      public string String;
      public string[] StringA;
      public object Object;
      public object[] ObjectA;
      public Vector3 Vector3;
      public Vector3[] Vector3A;
      public Quaternion Quaternion;
      public Quaternion[] QuaternionA;
    }

    public NetHelper Sync(string method) {
      if (!View.IsMine)
        throw new Exception($"Invalid Sync on {name}! Check Ownership settings in it's NetBridge");

      _method = method;
      _d.Clear();
      return this;
    }

    public NetHelper Add(bool val) {
      Data _data = default(Data);
      _data.dataType = DataType.Boolean;
      _data.val.Boolean = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(bool[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.BooleanA;
      _data.val.BooleanA = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(sbyte val) {
      Data _data = default(Data);
      _data.dataType = DataType.SByte;
      _data.val.SByte = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(sbyte[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.SByteA;
      _data.val.SByteA = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(byte val) {
      Data _data = default(Data);
      _data.dataType = DataType.Byte;
      _data.val.Byte = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(byte[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.ByteA;
      _data.val.ByteA = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(char val) {
      Data _data = default(Data);
      _data.dataType = DataType.Char;
      _data.val.Char = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(char[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.CharA;
      _data.val.CharA = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(ushort val) {
      Data _data = default(Data);
      _data.dataType = DataType.UInt16;
      _data.val.UInt16 = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(ushort[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.UInt16A;
      _data.val.UInt16A = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(short val) {
      Data _data = default(Data);
      _data.dataType = DataType.Int16;
      _data.val.Int16 = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(short[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.Int16A;
      _data.val.Int16A = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(uint val) {
      Data _data = default(Data);
      _data.dataType = DataType.UInt32;
      _data.val.UInt32 = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(uint[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.UInt32A;
      _data.val.UInt32A = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(int val) {
      Data _data = default(Data);
      _data.dataType = DataType.Int32;
      _data.val.Int32 = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(int[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.Int32A;
      _data.val.Int32A = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(float val) {
      Data _data = default(Data);
      _data.dataType = DataType.Single;
      _data.val.Single = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(float[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.SingleA;
      _data.val.SingleA = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(ulong val) {
      Data _data = default(Data);
      _data.dataType = DataType.UInt64;
      _data.val.UInt64 = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(ulong[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.UInt64A;
      _data.val.UInt64A = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(long val) {
      Data _data = default(Data);
      _data.dataType = DataType.Int64;
      _data.val.Int64 = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(long[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.Int64A;
      _data.val.Int64A = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(decimal val) {
      Data _data = default(Data);
      _data.dataType = DataType.Decimal;
      _data.val.Decimal = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(decimal[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.DecimalA;
      _data.val.DecimalA = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(double val) {
      Data _data = default(Data);
      _data.dataType = DataType.Double;
      _data.val.Double = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(double[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.DoubleA;
      _data.val.DoubleA = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(string val) {
      Data _data = default(Data);
      _data.dataType = DataType.String;
      _data.val.String = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(string[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.StringA;
      _data.val.StringA = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(object val) {
      Data _data = default(Data);
      _data.dataType = DataType.Object;
      _data.val.Object = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(object[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.ObjectA;
      _data.val.ObjectA = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(Vector3 val) {
      Data _data = default(Data);
      _data.dataType = DataType.Vector3;
      _data.val.Vector3 = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(Vector3[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.Vector3A;
      _data.val.Vector3A = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(Quaternion val) {
      Data _data = default(Data);
      _data.dataType = DataType.Quaternion;
      _data.val.Quaternion = val;
      _d.Enqueue(_data);
      return this;
    }

    public NetHelper Add(Quaternion[] val) {
      Data _data = default(Data);
      _data.dataType = DataType.QuaternionA;
      _data.val.QuaternionA = val;
      _d.Enqueue(_data);
      return this;
    }

    public void Send(SendTo receivers) {
      Disrupt.Sync(this, _method, receivers, _d);
      _method = "";
    }

    public void Send(Peer peer) {
      Disrupt.Sync(this, _method, peer, _d);
      _method = "";
    }

    public void Send(EndPoint endpoint) {
      Disrupt.Sync(this, _method, endpoint, _d);
      _method = "";
    }
    public void Send(Peer[] peers) {
      Disrupt.Sync(this, _method, peers, _d);
      _method = "";
    }

    public virtual void NetworkingLoop() { }
    public virtual bool ShouldLoop { get { return false; } }
    public void AddComponentToNetLoop() {
      Disrupt.Manager.LoopedComponents.Add(this);
    }
  }
}
