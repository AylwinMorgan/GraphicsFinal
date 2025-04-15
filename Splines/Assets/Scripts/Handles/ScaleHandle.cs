using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScaleHandle))]
[CanEditMultipleObjects]
public class ScaleHandleEditor : Editor
{
    public void OnSceneGUI()
    {
        ScaleHandle handle = (target as ScaleHandle);

        EditorGUI.BeginChangeCheck();
        Vector3 scale = Handles.ScaleHandle(handle.scale, Vector3.zero, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Scaled point");
            handle.scale = scale;
            handle.Update();
        }
    }
}

public class ScaleHandle : MonoBehaviour
{
    public Vector3 scale = Vector3.one;
    public void Update()
    {
        transform.localScale = scale;
    }
}