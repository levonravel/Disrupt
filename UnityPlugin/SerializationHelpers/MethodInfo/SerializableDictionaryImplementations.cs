using RavelTek.Disrupt;
using System;
using System.Collections.Generic;
using UnityEngine;
 
// ---------------
//  String => Int
// ---------------
[Serializable]
public class StringIntDictionary : SerializableDictionary<string, int> {}
 
// ---------------
//  GameObject => Float
// ---------------
[Serializable]
public class GameObjectFloatDictionary : SerializableDictionary<GameObject, float> {}
[Serializable]
public class IntMethodInfoDictionary : SerializableDictionary<int, MethodInformation> { }
[Serializable]
public class StringInttDictionary : SerializableDictionary<string, int> { }
[Serializable]
public class UshortNetBridgeDictionary : SerializableDictionary<ushort, NetBridge> { }
[Serializable]
public class NetBridgeDictionary : SerializableDictionary<int, UshortNetBridgeDictionary> { }
