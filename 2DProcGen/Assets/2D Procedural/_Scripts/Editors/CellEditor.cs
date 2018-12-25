using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(GCVisualComponent))]
[CanEditMultipleObjects]
public class CellEditor : Editor
{
    private GameObject spawn;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GCVisualComponent cellEditor = (GCVisualComponent)target;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set Spawn", GUILayout.ExpandWidth(false)))
        {
            cellEditor.SetSpawn(spawn);
            Debug.Log(cellEditor.GetSpawn());
        }
        spawn = (GameObject)EditorGUILayout.ObjectField(spawn, typeof(GameObject), true, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Reset Spawn", GUILayout.ExpandWidth(false)))
        {
            cellEditor.SetSpawn(null);
            cellEditor.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
    }
}
#endif