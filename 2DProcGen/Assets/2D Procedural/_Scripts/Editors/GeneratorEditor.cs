using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Generator))]
[CanEditMultipleObjects]
public class GeneratorEditor : Editor {

    private int iterations = 0, sRX = 0, sRY = 0, sX = 0, sY = 0;
    private Object room;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Generator gen = (Generator)target;
        GUILayout.Label("-------------------------------------------------");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Iterations: "))
        {
            gen.Generate(iterations);
            Debug.Log("Rows: " + gen.map.Rows() + "\n Cols: " + gen.map.Cols());
        }
        iterations = EditorGUILayout.IntField(iterations, GUILayout.Height(20f), GUILayout.Width(20f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Place Start"))
        {
            gen.PlaceStart();
        }
        if(GUILayout.Button("Place Random At Location: "))
        {
            gen.PlaceRandAtLocation(sRX, sRY);
        }
        GUILayout.Label("(");
        sRX = EditorGUILayout.IntField(sRX, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.Label(",");
        sRY = EditorGUILayout.IntField(sRY, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.Label(")");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Place Room At Location:"))
        {
            gen.PlaceAtLocation(sX, sY, room as Room);
        }
        GUILayout.Label("(");
        sX = EditorGUILayout.IntField(sX, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.Label(",");
        sY = EditorGUILayout.IntField(sY, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.Label(")");
        GUILayout.Label("Room:");
        room = EditorGUILayout.ObjectField(room, typeof(Room), true, GUILayout.Height(20f), GUILayout.Width(150f));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Reset"))
        {
            gen.ResetGenerator();
            ConsoleMod.ClearLog();
        }
        if (GUILayout.Button("Debug Active Cells"))
            gen.map.GetActiveCells();
    }
}
#endif