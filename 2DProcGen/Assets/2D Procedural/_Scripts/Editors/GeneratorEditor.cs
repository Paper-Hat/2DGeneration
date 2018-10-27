using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Generator))]
[CanEditMultipleObjects]
public class GeneratorEditor : Editor {
    
    [SerializeField] private int _rooms, _boundsX1 = -4, _boundsX2 = 4, _boundsY1 = -4, _boundsY2 = 4, _sRx, _sRy, _sX, _sY;
    private Vector2 _start;
    private Object room;
    [SerializeField] private Generator.Constraints.Style _style;
    [SerializeField] private Generator.Constraints.Types _constraints;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Generator gen = (Generator) target;
     
        GUILayout.Label("--------------------------", GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Init", GUILayout.ExpandWidth(false)))
            gen.Init();
        GUILayout.Label("Set Constraints For Generation: ", GUILayout.ExpandWidth(false));
        GUILayout.BeginHorizontal();
        GUILayout.Label("Style:", GUILayout.ExpandWidth(false));
        _style = (Generator.Constraints.Style)EditorGUILayout.EnumPopup("Gen. Style: ", _style);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Constraint Types:", GUILayout.ExpandWidth(false));
        _constraints = (Generator.Constraints.Types)EditorGUILayout.EnumFlagsField(_constraints, GUILayout.Height(20f), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Number of rooms: ", GUILayout.ExpandWidth(false));
        _rooms = EditorGUILayout.IntField(_rooms, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Boundary range on X-axis: ", GUILayout.ExpandWidth(false));
        _boundsX1 = EditorGUILayout.IntField(_boundsX1, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.Label("-", GUILayout.ExpandWidth(false));
        _boundsX2 = EditorGUILayout.IntField(_boundsX2, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Boundary range on Y-Axis: ", GUILayout.ExpandWidth(false));
        _boundsY1 = EditorGUILayout.IntField(_boundsY1, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.Label("-", GUILayout.ExpandWidth(false));
        _boundsY2 = EditorGUILayout.IntField(_boundsY2, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Set Starting Position: ", GUILayout.ExpandWidth(false));
        _start = EditorGUILayout.Vector2Field("", _start);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Set constraints with above values: ");
        if(GUILayout.Button("Set"))
            Generator.SetGenConstraints(_style, _constraints, _rooms, (_boundsX1, _boundsX2), (_boundsY1, _boundsY2), _start);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Generate with above constraints: ", GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Generate", GUILayout.Height(20f), GUILayout.ExpandWidth(false)))
        {
            gen.Generate(/*_constraints, _style*/);
            Debug.Log("Rows: " + gen.Map.Rows() + "\n Cols: " + gen.Map.Cols());
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("--------------------------");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Place Random At Location: ", GUILayout.Height(20f), GUILayout.Width(200f)))
        {
            gen.PlaceRandAtLocation(_sRx, _sRy);
        }
        GUILayout.Label("(", GUILayout.ExpandWidth(false));
        _sRx = EditorGUILayout.IntField(_sRx, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.Label(",", GUILayout.ExpandWidth(false));
        _sRy = EditorGUILayout.IntField(_sRy, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.Label(")", GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Place Room At Location:"))
        {
            gen.PlaceAtLocation(_sX, _sY, room as Room);
        }
        GUILayout.Label("(", GUILayout.ExpandWidth(false));
        _sX = EditorGUILayout.IntField(_sX, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.Label(",", GUILayout.ExpandWidth(false));
        _sY = EditorGUILayout.IntField(_sY, GUILayout.Height(20f), GUILayout.Width(30f));
        GUILayout.Label(")", GUILayout.ExpandWidth(false));
        GUILayout.Label("Room:", GUILayout.ExpandWidth(false));
        room = EditorGUILayout.ObjectField(room, typeof(Room), true, GUILayout.Height(20f), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
        {
            gen.ResetGenerator();
            UsefulFunctions.ClearLog();
        }

        if (GUILayout.Button("Debug Active Cells", GUILayout.ExpandWidth(false)))
            gen.Map.GetActiveCells();
    }
}
#endif