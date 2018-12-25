using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
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
        if (!loaded)
        {
            LoadAssets();
        }

        GUILayout.Label("-------------------------------------------------");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Initialize Builder", GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Init", GUILayout.ExpandWidth(false)))
        {
            builder.Init();
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("Add the Canvas and Grid Layout to your \n Scene at worldspace (0,0,0) according to \n set Columns, Rows, and Size fields: ");
        if (GUILayout.Button("Create/Replace Builder Canvas & Grid", GUILayout.ExpandWidth(false)))
        {
            if (!builder.GetCanvas())
            {
                builder.CreateCanvas();
                builder.CreateGrid();
                indexes = new int[builder.GetButtonGrid().GetLength(0)
                                , builder.GetButtonGrid().GetLength(1)];
                grid = true;
            }
            else if (builder.GetCanvas() && EditorUtility.DisplayDialog("Replace Canvas", "Re-create canvas and grid using set values?", "Yes", "Cancel"))
            {
                builder.ClearCanvasAndGrid();
                builder.CreateCanvas();
                builder.CreateGrid();
                grid = true;
            }
            else
                Debug.Log("Canvas already created.");
            Clean();
        }
        GUILayout.Label("Fill grid with grid buttons according to \n layout:");
        if (GUILayout.Button("Fill Created Grid", GUILayout.ExpandWidth(false)))
        {
            _failCheck = builder.FillGrid();
            if(_failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            Clean();
        }
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
                foreach(GameObject g in builder.GetButtonGrid())
                    g.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                builder.ResetCells();
            }
            Clean();
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("Set all grid cells back to their original state.");
        if (GUILayout.Button("Reset Cells", GUILayout.ExpandWidth(false)))
        {
            _failCheck = builder.ResetCells();
            if (_failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            Clean();
        }
        GUILayout.Label("The following will destroy all room builder \n components and objects created by \n the above buttons:");
        if (GUILayout.Button("Full Reset", GUILayout.ExpandWidth(false)))
        {
            if(EditorUtility.DisplayDialog("Reset All Assets", "Are you sure?", "Yes", "Cancel"))
                builder.ResetBuilder();
            grid = false;
            loaded = false;
            Clean();
        }
        CreateUIGrid(builder, grid);
    }

    void CreateUIGrid(PatternBuilder builder, bool gridExists)
    {
        if (!gridExists) return;
        GameObject[,] bg = builder.GetButtonGrid();
        GUILayout.Label("Pattern", EditorStyles.boldLabel);
        //indexes[0, 0] = EditorGUILayout.Popup(indexes[0, 0], popupOptions, GUILayout.ExpandWidth(false));
        //Debug.Log(bg.GetLength(0) + ", " + bg.GetLength(1));
        for (int i = 0; i < bg.GetLength(0); i++) //rows
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < bg.GetLength(1); j++) //columns
            {
                EditorGUILayout.BeginVertical();
                indexes[i, j] = EditorGUILayout.Popup(indexes[i, j], popupOptions);
                //Debug.Log(indexes[i, j]);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
            //EditorGUI.DrawRect(new Rect(rect.position, cRect.sizeDelta/10), Color.red);
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
    void PatternByUI(PatternBuilder builder)
    {
        for(int i = 0; i < indexes.GetLength(0); i++)
        {
            for (int j = 0; j < indexes.GetLength(1); j++)
            {
                if (indexes[i, j] != 0)
                {
                    builder.SetGridObject((i, j), gameAssets[indexes[i, j]]);
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