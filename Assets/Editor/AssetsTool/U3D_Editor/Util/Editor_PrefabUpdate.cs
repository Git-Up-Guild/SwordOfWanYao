using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PrefabUpdate))]
public class Editor_PrefabUpdate : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Update Prefab"))
        {
            PrefabUpdate spawn = target as PrefabUpdate;
            spawn.UpdatePrefabs();
        }

        if (GUILayout.Button("Find Instances(Reset Instances)"))
        {
            PrefabUpdate spawn = target as PrefabUpdate;
            spawn.FindInstances();
        }

    }
}

