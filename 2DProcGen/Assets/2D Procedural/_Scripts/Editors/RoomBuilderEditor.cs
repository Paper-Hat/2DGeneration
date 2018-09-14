using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(RoomBuilder))]
[CanEditMultipleObjects]
public class RoomBuilderEditor : Editor {

    private bool failCheck;
    private string roomName = "";
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        RoomBuilder builder = (RoomBuilder)target;
        GUILayout.Label("-------------------------------------------------");
        if (GUILayout.Button("Initialize")){
            failCheck = builder.Init();
            if(failCheck)
                EditorUtility.DisplayDialog("Failed", builder.GetReason(), "Continue");
        }
        GUILayout.Label("Create Exit: ");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Up")){
            failCheck = builder.CreateExit(Exit.Orientation.Up);
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", builder.GetReason(), "Continue");
        }
        if (GUILayout.Button("Down")){
            failCheck = builder.CreateExit(Exit.Orientation.Down);
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", builder.GetReason(), "Continue");
        }
        if (GUILayout.Button("Left")){
            failCheck = builder.CreateExit(Exit.Orientation.Left);
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", builder.GetReason(), "Continue");
        }
        if (GUILayout.Button("Right")){
            failCheck = builder.CreateExit(Exit.Orientation.Right);
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", builder.GetReason(), "Continue");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Clear Exits:");
        if (GUILayout.Button("Clear")){
            if(builder.exits.Count > 0)
                builder.exits.Clear();
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("-------------------------------------------------");

        GUILayout.Label("Add Collider Point: ");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
            builder.AddColliderPoint();
        GUILayout.EndHorizontal();
        GUILayout.Label("Complete room's collider:");
        if (GUILayout.Button("Complete"))
        {
            failCheck = builder.CompleteCollider();
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", builder.GetReason(), "Continue");
        }
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Clear Points"))
        {
            failCheck = builder.ClearPoints();
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", builder.GetReason(), "Continue");
        }
        if(GUILayout.Button("Clear Dict"))
        {
            failCheck = builder.ClearDict();
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", builder.GetReason(), "Continue");
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("-------------------------------------------------");
        GUILayout.Label("Create room asset using settings: ");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Room Name: ");
        roomName = EditorGUILayout.TextField(roomName, GUILayout.Height(20f), GUILayout.Width(50f));
        if (GUILayout.Button("Create Room")){
            failCheck = builder.CreateRoom();
            if (failCheck)
                EditorUtility.DisplayDialog("Failed", builder.GetReason(), "Continue");
            else{
                if (!string.IsNullOrEmpty(roomName))
                    CreateScriptableRoomAsset(builder.editing, roomName);
                else
                    EditorUtility.DisplayDialog("Failed", "Room field must be entered.", "Continue");
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Debugging:");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Debug List"))
            builder.DebugList();
        if (GUILayout.Button("Debug Dict"))
            builder.DebugDict();
        GUILayout.EndHorizontal();
    }
    public static void CreateScriptableRoomAsset(ScriptableObject so, string name)
    {
        string pathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/2D Procedural/Resources/_ScriptableObjects/Rooms/" + name + ".asset");
        AssetDatabase.CreateAsset(so, pathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif