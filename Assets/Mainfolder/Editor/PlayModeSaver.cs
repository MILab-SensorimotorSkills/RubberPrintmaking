/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonoBehaviour), true)]
public class PlayModeSaver : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Save Play Mode Changes"))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                EditorUtility.SetDirty(target);
            }
        }
    }
}
*/