using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(PatternBuilder))]
[CanEditMultipleObjects]
public class PatternBuilderEditor : Editor {

    private bool _failCheck;
    [SerializeField] private string _patternName;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PatternBuilder builder = (PatternBuilder)target;
        GUILayout.Label("-------------------------------------------------");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Initialize Builder", GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Init"))
        {
            builder.Init();
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("Add the Canvas and Grid Layout to your \n Scene at worldspace (0,0,0) according to \n set Columns, Rows, and Size fields: ");
        if (GUILayout.Button("Create/Replace Builder Canvas & Grid"))
        {
            if (!builder.GetCanvas())
            {
                builder.CreateCanvas();
                builder.CreateGrid();
            }
            else if (builder.GetCanvas() && EditorUtility.DisplayDialog("Replace Canvas", "Re-create canvas and grid using set values?", "Yes", "Cancel"))
            {
                builder.ClearCanvasAndGrid();
                builder.CreateCanvas();
                builder.CreateGrid();
            }
            else
                Debug.Log("Canvas already created.");
            Clean();
        }
        GUILayout.Label("Fill grid with grid buttons according to \n layout:");
        if (GUILayout.Button("Fill Created Grid"))
        {
            _failCheck = builder.FillGrid();
            if(_failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            Clean();
        }
        GUILayout.Label("Destroy all created grid buttons, including \n those which have already been designated \n as specific spawn points:");
        if (GUILayout.Button("Empty Created Grid"))
        {
            _failCheck = builder.EmptyGrid();
            if (_failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            Clean();
        }
        GUILayout.Label("Create a usable 'Pattern' out of preset \n spawn points and typing, which will be \n allowed placement in the generator:");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Pattern Name: ");
        _patternName = EditorGUILayout.TextField(_patternName, GUILayout.Height(20f), GUILayout.Width(50f));
        if (GUILayout.Button("Create Pattern"))
        {
            _failCheck = builder.CreatePattern();
            if (_failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            else
            {
                EditorUtility.DisplayDialog("Success", "Pattern successfully created.", "Continue");
                CreateScriptablePatternAsset(builder.editing, _patternName);
                foreach(GameObject g in builder.GetButtonGrid())
                    g.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                builder.ResetCells();
            }

            Clean();
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("Set all grid cells back to their original state.");
        if (GUILayout.Button("Reset Cells"))
        {
            _failCheck = builder.ResetCells();
            if (_failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            Clean();
        }
        GUILayout.Label("The following will destroy all room builder \n components and objects created by \n the above buttons:");
        if (GUILayout.Button("Full Reset"))
        {
            if(EditorUtility.DisplayDialog("Reset All Assets", "Are you sure?", "Yes", "Cancel"))
                builder.ResetBuilder();
            Clean();
        }

    }
    public static void CreateScriptablePatternAsset(ScriptableObject so, string name)
    {
        string pathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/2D Procedural/Resources/_ScriptableObjects/Patterns/" + name + ".asset");
        AssetDatabase.CreateAsset(so, pathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    public void Clean()
    {
        AssetDatabase.Refresh();
        Repaint();
    }
    private static T LoadAsset<T>(string path) where T : Object{    return AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T; }
}
#endif