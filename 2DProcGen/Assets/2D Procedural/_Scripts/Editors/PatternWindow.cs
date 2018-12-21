using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PatternWindow : EditorWindow
{
    [MenuItem("Window/Pattern Window")]
    static void Init()
    {
        PatternWindow window = (PatternWindow) EditorWindow.GetWindow(typeof(PatternWindow));
        window.Show();
    }
}
