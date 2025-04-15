using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PositionHandle))]
[CanEditMultipleObjects]
public class PositionHandleEditor : Editor
{
    public void OnSceneGUI()
    {
        PositionHandle handle = (target as PositionHandle);

        EditorGUI.BeginChangeCheck();
        Vector3 position = Handles.PositionHandle(handle.position, Quaternion.identity);
        if (EditorGUI.EndChangeCheck() )
        {
            Undo.RecordObject(target, "Moved Point");
            handle.position = position;
            handle.Update();
        }
    }
}

public class PositionHandle : MonoBehaviour
{
    public Vector3 position = Vector3.zero;
    public void Update()
    {
        transform.position = position;
    }
}