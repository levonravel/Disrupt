
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;
using UnityEditor.SceneManagement;

namespace RavelTek.Disrupt
{
    [CustomEditor(typeof(DisruptManagement), true, isFallback = true)]
    public class RavelResourceEditor : Editor
    {
        private GUIStyle toggleButtonStyleNormal = null;
        private GUIStyle toggleButtonStyleToggled = null;
        private int currentToggle;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (toggleButtonStyleNormal == null)
            {
                toggleButtonStyleNormal = "Button";
                toggleButtonStyleToggled = new GUIStyle(toggleButtonStyleNormal);
                toggleButtonStyleToggled.normal.background = toggleButtonStyleToggled.active.background;
            }
            GUILayout.Space(20);
            GUI.backgroundColor = new Color32(180, 180, 180, 255);
            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label("NETWORKED PREFABS", new GUIStyle()
            {
                alignment = TextAnchor.LowerCenter,
                fontStyle = FontStyle.Bold,
            });
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("GroupBox");
            for(int i = DisruptManagement.Instance.Prefabs.Count - 1; i >= 0; i--)
            {
                if(DisruptManagement.Instance.Prefabs[i] == null)
                {
                    DisruptManagement.Instance.Prefabs.RemoveAt(i);
                    continue;
                }
                GUILayout.Label(DisruptManagement.Instance.Prefabs[i].name, new GUIStyle() { fontStyle = FontStyle.Italic });
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            if (GUILayout.Button("Populate Prefabs"))
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
                    //AssetDatabase.Refresh();
                }
                EditorUtility.SetDirty(DisruptManagement.Instance);
                AssetDatabase.SaveAssets();
            }
            GUILayout.FlexibleSpace();
            GUILayout.Space(20);
            GUI.backgroundColor = new Color32(180, 180, 180, 255);
            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Need Help?", new GUIStyle()
            {
                alignment = TextAnchor.LowerCenter,
                fontStyle = FontStyle.Bold,
            });
            GUILayout.Space(10);
            if (GUILayout.Button("Discord"))
            {
                Application.OpenURL("https://discord.gg/9UmAqan");
            }
            GUILayout.EndVertical();
        }
        
        public void Toggle(ref bool isSet, string text, ref int id)
        {
            var style = id == currentToggle ? toggleButtonStyleToggled : toggleButtonStyleNormal;
            if (GUILayout.Button(text, style))
            {
                isSet = !isSet;
                currentToggle = id;
            }
            id++;
        }
        private string GetFileChecksum(string file, HashAlgorithm algorithm)
        {
            string result = string.Empty;
            using (FileStream fs = File.OpenRead(file))
            {
                result = System.BitConverter.ToString(algorithm.ComputeHash(fs)).ToLower().Replace("-", "");
            }
            return result;
        }
    }
}
#endif