using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RotationHandle))]
[CanEditMultipleObjects]
public class RotationHandleEditor : Editor
{
    public void OnSceneGUI()
    {
        RotationHandle handle = (target as RotationHandle);

        EditorGUI.BeginChangeCheck();
        Quaternion rot = Handles.RotationHandle(handle.rot, Vector3.zero);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target,"Rotated Point");
            handle.rot = rot;
            handle.Update();
        }
    }
}

public class RotationHandle : MonoBehaviour
{
    public Quaternion rot = Quaternion.identity;
    public void Update()
    {
        transform.rotation = rot;
    }
}