using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(PatternBuilder))]
[CanEditMultipleObjects]
public class PatternBuilderEditor : Editor {

    private bool failCheck;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PatternBuilder builder = (PatternBuilder)target;
        GUILayout.Label("-------------------------------------------------");
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
            failCheck = builder.FillGrid();
            if(failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            Clean();
        }
        GUILayout.Label("Destroy all created grid buttons, including \n those which have already been designated \n as specific spawn points:");
        if (GUILayout.Button("Empty Created Grid"))
        {
            failCheck = builder.EmptyGrid();
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            Clean();
        }
        GUILayout.Label("Create a usable 'Pattern' out of preset \n spawn points and typing, which will be \n allowed placement in the generator:");
        if (GUILayout.Button("Create Pattern"))
        {
            failCheck = builder.CreatePattern();
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            else
                EditorUtility.DisplayDialog("Success", "Pattern successfully created.", "Continue");
            Clean();
        }
        GUILayout.Label("Save all patterns created to a prefab:");
        if (GUILayout.Button("Save Patterns"))
        {
            failCheck = builder.SavePatterns();
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            else
            {
                EditorUtility.DisplayDialog("Success", builder.patternList.Count
                    + " pattern(s) saved.", "Continue");
                CreateNewPattern(builder.GetContainer(), builder.GetFilePath());
            }
            Clean();
        }
        GUILayout.Label("Clear all stored patterns:");
        if(GUILayout.Button("Clear Patterns"))
        {
            failCheck = builder.SavePatterns();
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", "Reason: " + builder.GetReason(), "Continue");
            else
                EditorUtility.DisplayDialog("Success", "All Patterns cleared.", "Continue");
            Clean();
        }
        GUILayout.Label("Set all grid cells back to their original state.");
        if (GUILayout.Button("Reset Cells"))
        {
            failCheck = builder.ResetCells();
            if (failCheck)
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
    static void CreateNewPattern(GameObject toPrefab, string filePath)
    {
        string prefabPath = "_Prefabs/PatternContainerPrefabs/PatternContainer";
        CreateNew(toPrefab, prefabPath);
    }
    //combine list from old prefab and new - insert objects from prev prefab at proper locations in new list
    static void CreateNew(GameObject g, string path)
    {
        //I give up. this is a terrible solution, but will suffice until another is found
        string prefabPath = "Assets/2D Procedural/Resources/" + path + ".prefab";

        List<PatternBuilder.Pattern> newContainerList = g.GetComponent<PatternContainer>().patterns;
        if (Resources.Load(path) != null)
        {
            //Debug.Log("Prefab found in resources folder.");
            GameObject prevGO = Instantiate(LoadAsset<GameObject>(prefabPath));
            AssetDatabase.StartAssetEditing();
            g.GetComponent<PatternContainer>().patterns = CombinePrefabLists(prevGO, newContainerList);
            AssetDatabase.StopAssetEditing();
            DestroyImmediate(prevGO, false);
            //DestroyImmediate(prevGO, false);
        }
        PrefabUtility.ReplacePrefab(g, PrefabUtility.CreatePrefab(prefabPath, g, ReplacePrefabOptions.ConnectToPrefab), ReplacePrefabOptions.ConnectToPrefab);
    }
    static List<PatternBuilder.Pattern> CombinePrefabLists(GameObject prevPrefab, List<PatternBuilder.Pattern> newList)
    {

        List<PatternBuilder.Pattern> prevContainerList = prevPrefab.GetComponent<PatternContainer>().patterns;
        bool fault = false;
        List<int> failPositions = new List<int>();
        for (int i = 0; i < prevContainerList.Count; i++)
        {
            //Debug.Log("Reaches this function " + (i + 1) + " times.");
            foreach (PatternBuilder.Pattern p in newList)
            {
                if (p.Equals(prevContainerList[i]))
                {
                    fault = true;
                    failPositions.Add(i);
                }
            }
            if (!fault)
                newList.Insert(i, prevContainerList[i]);
            fault = false;
        }
        if (failPositions.Count > 0)
            Debug.Log("Duplicate patterns at position(s): " + failPositions + " not copied.");
        
        return newList;
    }
    public void Clean()
    {
        AssetDatabase.Refresh();
        Repaint();
    }
    private static T LoadAsset<T>(string path) where T : Object{    return AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T; }
}
#endif