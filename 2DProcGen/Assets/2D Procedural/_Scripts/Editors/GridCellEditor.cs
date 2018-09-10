using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(GridCell))]
[CanEditMultipleObjects]
public class GridCellEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridCell cellEditor = (GridCell)target;

        if (GUILayout.Button("Set as Enemy Spawn"))
            cellEditor.SetSpawnType(GridCell.SpawnType.Enemy);
        if (GUILayout.Button("Set as Obstacle Spawn"))
            cellEditor.SetSpawnType(GridCell.SpawnType.Obstacle);
        if (GUILayout.Button("Reset/Remove Spawn"))
            cellEditor.SetSpawnType(GridCell.SpawnType.None);
    }
}
#endif