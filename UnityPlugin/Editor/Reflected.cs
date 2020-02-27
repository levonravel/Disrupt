
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RavelTek.Disrupt
{
    public class Reflected
    {
        public static int Id;
        private static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        
        public static void AssignIds()
        {
            Id = 0;
            DisruptManagement.ObjectId = 0;
            PopulatePrefabs();
            foreach (var prefab in DisruptManagement.Instance.Prefabs)
            {
                var view = prefab.GetComponent<NetBridge>();
                AssignId(view);
            }
            OpenAllActiveBuildScenes();
            var bridges = FindAllOfTypeInOpenScenes<NetBridge>(true);
            foreach(var bridge in bridges)
            {
                AssignId(bridge);
            }
            Debug.Log("Assigned " + DisruptManagement.ObjectId + " IDS!");
            CloseAllScenesExceptCurrent();
            EditorUtility.SetDirty(DisruptManagement.Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Network setup complete.");
        }
        private static void AssignId(NetBridge view)
        {
            var __isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(view);
            // Only assign ID if not a prefab ASSET (aka is an object in a scene)
            if (!__isPrefabAsset)
            {
                view.ObjectId = DisruptManagement.ObjectId;
                DisruptManagement.ObjectId++;
                //Is part of a prefab 
                if (PrefabUtility.IsPartOfPrefabInstance(view))
                {
                    Debug.Log($"Setting a prefab property to obj {view.name}. ID: {view.ObjectId}");
                    RevertToPrefabState(view);
                    return;
                }
            }
            // At this point the View isn't a prefab instance. It's either an asset, or a non prefab object
            view.Methods.Clear();
            var netHelpers = GetHelpers(view.gameObject);
            foreach (var helper in netHelpers)
            {
                helper.MethodPointers.Clear();
                var type = helper.GetType();
                var allMethods = helper.GetType().GetMethods(flags);
                var methods = helper.GetType().GetMethods(flags).Where(methodInfo => methodInfo.GetCustomAttributes(typeof(RD), true).Length > 0).ToList();
                if (methods.Count == 0) continue;
                foreach (var methodInfo in methods)
                {
                    var RD = (RD)methodInfo.GetCustomAttribute(typeof(RD));
                    var classInfo = new MethodInformation();
                    var parms = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                    classInfo.Method = new SerializableMethodInfo(methodInfo);
                    classInfo.Class = helper;
                    classInfo.Protocol = RD.Protocol;
                    classInfo.Name = methodInfo.Name;
                    view.Methods.Add(Id, classInfo);
                    helper.MethodPointers.Add(classInfo.Name, Id);
                    Id++;
                }
                if (__isPrefabAsset) continue;
                // Important:
                if (PrefabUtility.IsPartOfPrefabInstance(helper))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(helper);
                }
            }            
        }
        private static List<NetHelper> GetHelpers(GameObject root)
        {
            var componentList = new List<NetHelper>();
            var netHelpers = root.GetComponentsInChildren(typeof(NetHelper), true);
            if (netHelpers != null)
            {
                foreach (NetHelper component in netHelpers)
                {
                    componentList.Add(component);
                }
            }
            return componentList;
        }
        private static void OpenAllActiveBuildScenes()
        {
            var currentScenePath = EditorSceneManager.GetActiveScene().path;
            List<string> scenes = new List<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled || currentScenePath == scene.path) continue;
                scenes.Add(scene.path);
            }

            foreach (var scene in scenes)
            {
                var __scene = EditorSceneManager.OpenScene(scene, OpenSceneMode.Additive);
            }
        }
        private static void CloseAllScenesExceptCurrent()
        {
            var currentScene = SceneManager.GetActiveScene();

            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    //UnityEngine.Debug.LogError("Couldn't save scene: " + scene);

                if (scene == currentScene) continue;

                EditorSceneManager.CloseScene(scene, true);
            }
        }
        public static T[] FindAllOfTypeInOpenScenes<T>(bool includeInactive) where T : MonoBehaviour
        {
            var results = new List<T>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var roots = scene.GetRootGameObjects();
                for (var j = 0; j < roots.Length; j++)
                    results.AddRange(roots[j].GetComponentsInChildren<T>(includeInactive));
            }

            return results.ToArray();
        }
        private static void PopulatePrefabs()
        {
            DisruptManagement.Instance.ClearPrefabs();
            var assets = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabRoot = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                var view = prefabRoot.GetComponent<NetBridge>();
                if (view == null) continue;
                DisruptManagement.Instance.InsertPrefab(prefabRoot);
            }
        }
        static void RevertToPrefabState(NetBridge bridge)
        {
            var __serializedObject = new SerializedObject(bridge);
            var __methodInfo = __serializedObject.FindProperty("methodInfoStore");
            PrefabUtility.RevertPropertyOverride(__methodInfo, InteractionMode.AutomatedAction);

            var __helpers = GetHelpers(bridge.gameObject);
            foreach (var helper in __helpers)
            {
                var __serializedHelper = new SerializedObject(helper);
                var __methodPointerStore = __serializedHelper.FindProperty("methodPointerStore");
                PrefabUtility.RevertPropertyOverride(__methodPointerStore, InteractionMode.AutomatedAction);
                __serializedHelper.Dispose();
            }

            __serializedObject.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(bridge);
            __serializedObject.Dispose();
        }
    }
}
#endif