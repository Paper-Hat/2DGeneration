using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
public static class UsefulFunctions {
    public static void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    public static void PrintList<T>(List<T> typedCollection)
    {
        Debug.Log("Printed List: ");
        foreach(var item in typedCollection)
            Debug.Log(item.ToString());
        Debug.Log("End of List.");
    }
}
