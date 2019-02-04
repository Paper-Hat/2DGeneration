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
    private static string[] _popupOptions;
    private static int[,] indexes;
    private List<GameObject> gameAssets;
    private JKGrid jg;
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
        GUILayout.Label("Add the Grid Layout to your \n Scene at worldspace (0,0,0) according to " +
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
        GUILayout.Label("Destroy all created grid buttons, including \n those which have already been designated \n as specific spawn points:");
        if (GUILayout.Button("Empty Created Grid", GUILayout.ExpandWidth(false)))
        {
            _failCheck = builder.EmptyGrid();
            if (_failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            ResetIndexes();
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
                ResetIndexes();
                builder.EmptyGrid();
            }
            Clean();
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("The following will destroy all room builder \n components and objects created by \n the above buttons:");
        if (GUILayout.Button("Full Reset", GUILayout.ExpandWidth(false)))
        {
            if(EditorUtility.DisplayDialog("Reset All Assets", "Are you sure?", "Yes", "Cancel"))
                builder.ResetBuilder();
            ResetIndexes();
            builder.allowDisplay = false;
            grid = false;
            loaded = false;
            Clean();
        }
        CreateUIGrid(builder);
    }

    void CreateUIGrid(PatternBuilder builder)
    {
        //Debug.Log("Creating UI Grid...");
        jg = builder.GetGrid();
        /*if (grid)
        {
            foreach(var item in popupOptions)
                Debug.Log(item);
            foreach (var item in indexes)
                Debug.Log(item.ToString());
        }*/

        
        Repaint();
        //Debug.Log(jg.ToString());
        GUILayout.Label("Pattern", EditorStyles.boldLabel);
        /*if(jg == null || popupOptions == null || indexes == null)
        {
            if(jg == null)
                Debug.Log("Grid is null.");
            if (popupOptions == null)
                Debug.Log("Popup Options are null.");
            if(indexes == null)
                Debug.Log("Indexes are null.");
        }*/
        if (jg != null && _popupOptions != null && indexes != null)
        {
            for (int i = 0; i < jg.GetIndexes().GetLength(0); i++) //rows
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < jg.GetIndexes().GetLength(1); j++) //columns
                {
                    EditorGUILayout.BeginVertical();
                    indexes[i, j] = EditorGUILayout.Popup(indexes[i, j], _popupOptions);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndHorizontal();
            }
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

        foreach (var asset in gameAssets) pList.Add(asset.ToString());
        _popupOptions = pList.ToArray();
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
                //SetGridObject with inverse indexes because of how UI is set up
                if (indexes[i, j] != 0)
                    builder.SetGridObject((j, i), gameAssets[indexes[i, j]-1], 100f);
                else
                    builder.SetGridObject((j, i), null, 0f);
            }
        }
    }

    void ResetIndexes()
    {
        if (indexes != null){
            for (int i = 0; i < indexes.GetLength(0); i++){
                for (int j = 0; j < indexes.GetLength(1); j++){
                    indexes[i, j] = 0;
                }
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