using System;
using System.Collections.Generic;
using System.Linq;
using jkGenerator;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(PatternBuilder))]
[CanEditMultipleObjects]
public class PatternBuilderEditor : Editor
{

    private bool _failCheck, grid, loaded;
    private string _patternName;
    private string[] popupOptions;
    private int[,] indexes;
    private List<GameObject> gameAssets;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PatternBuilder builder = (PatternBuilder)target;

        GUILayout.Label("-------------------------------------------------");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Initialize Builder", GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Init", GUILayout.ExpandWidth(false)))
        {
            builder.Init();
            LoadAssets();
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("Add the Canvas and Grid Layout to your \n Scene at worldspace (0,0,0) according to " +
                              "\n set Subdivisions and Size fields: ");
        if (GUILayout.Button("Create/Replace Builder Grid", GUILayout.ExpandWidth(false)))
        {
            if (builder.GetGrid() == null)
            {
                //builder.CreateCanvas();
                builder.CreateGrid();
                indexes = new int[builder.GetGrid().GetIndexes().GetLength(0)
                                , builder.GetGrid().GetIndexes().GetLength(1)];
                grid = true;
            }
            else if (builder.GetGrid() != null && EditorUtility.DisplayDialog("Replace Grid", "Re-create grid using set values?", "Yes", "Cancel"))
            {
                builder.CreateGrid();
                indexes = new int[builder.GetGrid().GetIndexes().GetLength(0)
                                , builder.GetGrid().GetIndexes().GetLength(1)];
                grid = true;
            }
            else
                Debug.Log("Grid already created.");
            Clean();
        }
        /*GUILayout.Label("Fill grid with grid buttons according to \n layout:");
        if (GUILayout.Button("Fill Created Grid", GUILayout.ExpandWidth(false)))
        {
            _failCheck = builder.FillGrid();
            if(_failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            Clean();
        }*/
        GUILayout.Label("Destroy all created grid buttons, including \n those which have already been designated \n as specific spawn points:");
        if (GUILayout.Button("Empty Created Grid", GUILayout.ExpandWidth(false)))
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
        if (GUILayout.Button("Create Pattern", GUILayout.ExpandWidth(false)))
        {
            _failCheck = builder.CreatePattern();
            if (_failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            else
            {
                EditorUtility.DisplayDialog("Success", "Pattern successfully created.", "Continue");
                CreateScriptablePatternAsset(builder.editing, _patternName);
                //foreach(GameObject g in builder.GetButtonGrid())
                //    g.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                builder.EmptyGrid();
            }
            Clean();
        }
        GUILayout.EndHorizontal();
        /*GUILayout.Label("Set all grid cells back to their original state.");
        if (GUILayout.Button("Reset Cells", GUILayout.ExpandWidth(false)))
        {
            _failCheck = builder.GetGrid().RemoveAll();
            if (_failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            Clean();
        }*/
        GUILayout.Label("The following will destroy all room builder \n components and objects created by \n the above buttons:");
        if (GUILayout.Button("Full Reset", GUILayout.ExpandWidth(false)))
        {
            if(EditorUtility.DisplayDialog("Reset All Assets", "Are you sure?", "Yes", "Cancel"))
                builder.ResetBuilder();
            builder.allowDisplay = false;
            grid = false;
            loaded = false;
            Clean();
        }
        //Debug.Log(grid);
        CreateUIGrid(builder);
    }

    void CreateUIGrid(PatternBuilder builder)
    {
        if (!grid) return;
        JKGrid jg = builder.GetGrid();
        GUILayout.Label("Pattern", EditorStyles.boldLabel);
        //indexes[0, 0] = EditorGUILayout.Popup(indexes[0, 0], popupOptions, GUILayout.ExpandWidth(false));
        //Debug.Log(bg.GetLength(0) + ", " + bg.GetLength(1));
        //Debug.Log("x: " + jg.GetIndexes().GetLength(0) + "\n y: " + jg.GetIndexes().GetLength(1));
        //Debug.Log("x: " + indexes.GetLength(0) + "\n y: " + indexes.GetLength(1));
        for (int i = 0; i < jg.GetIndexes().GetLength(0); i++) //rows
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < jg.GetIndexes().GetLength(1); j++) //columns
            {
                EditorGUILayout.BeginVertical();
                //Debug.Log(popupOptions.Length);
                indexes[i, j] = EditorGUILayout.Popup(indexes[i, j], popupOptions);
                //Debug.Log(indexes[i, j]);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        if(GUILayout.Button("Set"))
            PatternByUI(builder);
        EditorGUILayout.Space();
    }

    void LoadAssets()
    {
        string path = "_Prefabs/_Overlay/";
        List<string> pList = new List<string>(){ "None" };
        gameAssets = new List<GameObject>();
        gameAssets.AddRange(Resources.LoadAll(path + "_Enemies").Cast<GameObject>().ToList());
        gameAssets.AddRange(Resources.LoadAll(path + "_Objects").Cast<GameObject>().ToList());
        gameAssets.AddRange(Resources.LoadAll(path + "_Walls").Cast<GameObject>().ToList());
        gameAssets.AddRange(Resources.LoadAll(path + "_Guns").Cast<GameObject>().ToList());
        
        foreach(var asset in gameAssets)    pList.Add(asset.ToString());
 
        popupOptions = pList.ToArray();
        //foreach(var item in popupOptions) Debug.Log(item);
        loaded = true;
    }

    //iterate through indexes, set grid objects where index is not zero
    //TODO: properly integrate spawn chance
    void PatternByUI(PatternBuilder builder)
    {
        for(int i = 0; i < indexes.GetLength(0); i++)
        {
            for (int j = 0; j < indexes.GetLength(1); j++)
            {
                if (indexes[i, j] != 0)
                    builder.SetGridObject((i, j), gameAssets[indexes[i, j]], 100f);
                else
                    builder.SetGridObject((i, j), null, 0f);
            }
        }
    }
    static void CreateScriptablePatternAsset(ScriptableObject so, string name)
    {
        string pathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/2D Procedural/Resources/_ScriptableObjects/Patterns/" + name + ".asset");
        AssetDatabase.CreateAsset(so, pathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    void Clean()
    {
        AssetDatabase.Refresh();
        Repaint();
    }
    private static T LoadAsset<T>(string path) where T : Object{    return AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T; }
}
#endif