using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(TestGrid))]
[CanEditMultipleObjects]
public class TestGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TestGrid builder = (TestGrid)target;
        if (GUILayout.Button("Create", GUILayout.ExpandWidth(false)))
        {
            builder.CreateGrid();
        }
        if (GUILayout.Button("Destroy", GUILayout.ExpandWidth(false)))
        {
            builder.DestroyGrid();
        }
        
    }
}
