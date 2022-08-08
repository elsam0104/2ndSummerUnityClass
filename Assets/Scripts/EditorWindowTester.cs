using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class EditorWindowTester : EditorWindow
{
    string temp = "Go ahead";
    [MenuItem("TestEditer/EditorTest")]
    public static void init()
    {
        GetWindow<EditorWindowTester>().titleContent = new GUIContent("Editor");
    }
    public void OnGUI()
    {
        GUILayout.Label("Hellow world");
        EditorGUILayout.TextField("MSG", temp);
        if (GUILayout.Button("Test"))
        {
            foreach(var tObj in Selection.gameObjects)
            tObj.transform.rotation = Quaternion.Euler(
                Random.Range(-360f, 360f),
                Random.Range(-360f, 360f),
                Random.Range(-360f, 360f)
                );
        }
    }

}
