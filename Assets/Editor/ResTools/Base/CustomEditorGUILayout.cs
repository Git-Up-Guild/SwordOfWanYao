using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CustomEditorGUILayout
{
    public static List<Object> ObjectListField(string label, List<Object> objList, System.Type objType)
    {
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField(label);
        for (int i = objList.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal("Box");
            objList[i] = EditorGUILayout.ObjectField(objList[i], objType, false);
            if (GUILayout.Button("Remove"))
            {
                objList.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add"))
        {
            objList.Add(null);
        }
        EditorGUILayout.EndVertical();
        return objList;
    }

}
