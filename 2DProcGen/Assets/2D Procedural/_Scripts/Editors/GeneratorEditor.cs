using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Generator))]
[CanEditMultipleObjects]
public class GeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Generator gen = (Generator)target;
        GUILayout.Label("-------------------------------------------------");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
        {
            gen.Generate();
            Debug.Log("Rows: " + gen.map.Rows() + "\n Cols: " + gen.map.Cols());
        }
        if (GUILayout.Button("Reset"))
            gen.ResetGenerator();
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Debug Active Cells"))
            gen.map.GetActiveCells();
    }
}
#endif