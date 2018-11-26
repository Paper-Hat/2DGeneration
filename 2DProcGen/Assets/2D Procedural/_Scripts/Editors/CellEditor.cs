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
    private Cell.SpawnType spawnType;
    private Cell.WallType wallType;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GCVisualComponent cellEditor = (GCVisualComponent)target;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set Spawn", GUILayout.ExpandWidth(false)))
        {
            cellEditor.SetSpawnType(spawnType);
            Debug.Log(cellEditor.GetSpawnType());
        }
        spawnType = (Cell.SpawnType)EditorGUILayout.EnumPopup(spawnType, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set Wall Spawn", GUILayout.ExpandWidth(false)))
        {
            cellEditor.SetWallType(wallType);
        }
        wallType = (Cell.WallType)EditorGUILayout.EnumPopup(wallType, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Reset Spawn", GUILayout.ExpandWidth(false)))
        {
            cellEditor.SetSpawnType(Cell.SpawnType.None);
            cellEditor.SetWallType(Cell.WallType.None);
            cellEditor.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
    }
}
#endif