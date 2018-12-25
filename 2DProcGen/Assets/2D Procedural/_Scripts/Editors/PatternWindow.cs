using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
[ExecuteAlways]
public class PatternWindow : EditorWindow
{
    private int xySize;
    private Vector2 sizeDelta;
    
    [MenuItem("Window/Pattern Window")]
    static void Init()
    {
        PatternWindow window = (PatternWindow) EditorWindow.GetWindow(typeof(PatternWindow));
        window.Show();
    }

    void OnGUI()
    {           
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        xySize = EditorGUILayout.IntField("Size (X/Y): ", xySize, GUILayout.ExpandWidth(false));
        if(GUILayout.Button("Build", GUILayout.ExpandWidth(false)))
            buildWindowGrid();
        //GUI.BeginGroup (new Rect (Screen.width * 0.25f, Screen.height * 0.25f, 400, 400));

        //GUI.EndGroup();
        
    }

    void buildWindowGrid()
    {
        bool[,] buttonArr = new bool[xySize, xySize];
        float offX = 5f, offY = 5f;
        Vector2 pos = Vector2.zero;
        GUI.BeginGroup (new Rect (Screen.width * 0.15f, Screen.height * 0.15f, 400, 400));
        GUI.Box (new Rect (0,0 , 300,300), "Pattern");
        if (xySize > 0)
        {
            Debug.Log("Attempted to create UI with size: " + xySize );
            for (int rows = 0; rows < xySize; rows++){
                for (int cols = 0; cols < xySize; cols++)
                {
                    /*buttonArr[rows, cols] =*/ GUI.Button(new Rect(pos.x + offX, pos.y + offY, 15, 15), "O");
                    Debug.Log("Button pos: " + pos);
                    pos = new Vector2(pos.x + 20f, pos.y + 20f);
                }
            }
        }
        GUI.EndGroup();
    }
    
}
