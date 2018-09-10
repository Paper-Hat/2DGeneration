using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(PatternContainer))]
[CanEditMultipleObjects]
public class PatternContainerEditor : Editor {

    private bool up, failCheck;
    private int index = 0, trackCleared;
    private static int indexToRemove;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PatternContainer container = (PatternContainer)target;

        GUILayout.Label("------------------------------------------------");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Remove Pattern At Index:");
        indexToRemove = EditorGUILayout.IntField(index, GUILayout.Height(20f), GUILayout.Width(20f));

        if (GUILayout.Button("Remove Pattern")){
            failCheck = container.RemovePattern(indexToRemove);
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + container.GetReason(), "Continue");
            else
                EditorUtility.DisplayDialog("Success", "Pattern at index [" + indexToRemove + "] successfully removed.", "Continue");
            Clean();
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Clear/delete all patterns:");
        if (GUILayout.Button("Clear")){
            trackCleared = container.patterns.Count;
            failCheck = container.ClearPatterns();
            if (EditorUtility.DisplayDialog("Confirm Deletion", "Are you sure? This cannot be undone.", "Yes", "No")){
                if (failCheck)
                    EditorUtility.DisplayDialog("Failed", "Reason: " + container.GetReason(), "Continue");
                else
                    EditorUtility.DisplayDialog("Success", trackCleared + " pattern(s) successfully deleted.", "Continue");
            }
            Clean();
        }       
    }
    public void Clean()
    {
        AssetDatabase.Refresh();
        Repaint();
    }
}
#endif