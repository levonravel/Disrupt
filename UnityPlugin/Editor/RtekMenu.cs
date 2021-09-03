#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RavelTek.Disrupt
{
    public class RtekMenu
    {
        [MenuItem("RavelTek/Disrupt/Management")]
        public static void OpenInspector()
        {
            var asset = Resources.Load<DisruptManagement>("DisruptManagement");
            Selection.activeObject = asset;
        }
        [MenuItem("RavelTek/Disrupt/Compile Network")]
        public static void CompileNetwork()
        {
            Reflected.AssignIds();
        }
        [MenuItem("RavelTek/Disrupt/Create Manager")]
        public static void CreateManager()
        {
            var __manager = new GameObject("Disrupt Manager", new System.Type[] { typeof(DisruptManager) }).GetComponent<DisruptManager>();
            __manager.View.Ownership = OwnerType.SharedView;
        }
    }
}
#endif
