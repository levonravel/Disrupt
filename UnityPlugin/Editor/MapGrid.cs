#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapGrid : Editor
{
    [DrawGizmo (GizmoType.NonSelected)]
    public void DrawGizmo()
    {
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}
#endif
