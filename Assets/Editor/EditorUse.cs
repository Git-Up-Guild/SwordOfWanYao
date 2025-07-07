using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorUse : MonoBehaviour
{ 
    [MenuItem("Tools/Clean Missing Components")]
    public static void CleanMissingComponents()
    {
        // 获取场景中的所有游戏对象，包括预制体实例
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);

        foreach (GameObject obj in allObjects)
        {
            // 检查并清除缺失的组件
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            // 如果对象是预制体实例，确保更新预制体
            if (PrefabUtility.IsPartOfPrefabInstance(obj))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
            }
        }

        Debug.Log("清除完毕：所有缺失组件已被删除。");
    }
}
