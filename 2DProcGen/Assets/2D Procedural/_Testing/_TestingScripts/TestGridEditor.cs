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
        if (GUILayout.Button("Default", GUILayout.ExpandWidth(false)))
        {
            builder.GridDefault();
        }
        if (GUILayout.Button("Image", GUILayout.ExpandWidth(false)))
        {
            builder.GridByImage();
            builder.SetSize();
        }
        if (GUILayout.Button("Sprite", GUILayout.ExpandWidth(false)))
        {
            builder.GridBySprite();
            builder.SetSize();
        }
        if (GUILayout.Button("Destroy", GUILayout.ExpandWidth(false)))
        {
            builder.DestroyGrid();
        }
        
    }
}
