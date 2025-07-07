/*******************************************************************
** 文件名: PrefabUpdate.cs
** 版  权:    (C) 深圳冰川网络技术有限公司 
** 创建人:     郑秀程
** 日  期:    2016/8/30
** 版  本:    1.0
** 描  述:    Prefab更新辅助类
**            
** 应  用:    用于解决NestPrefab在预设体修改之后无法应用到实例的情况     
**
**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

public class PrefabUpdate : MonoBehaviour
{
    //#if UNITY_EDITOR
    [System.Serializable]
    public class PrefabBind
    {
        public GameObject prefab;
        public List<GameObject> instances;
    }

    public List<PrefabBind> prefabBinds;
    private Dictionary<Object, SerializedProperty> referenceInstanceIDs;
    private List<SerializedProperty> references;


    public void FindInstances()
    {

        Transform[] gos = gameObject.GetComponentsInChildren<Transform>();
        foreach (var bind in prefabBinds)
        {
            bind.instances.Clear();
            foreach (var go in gos)
            {
                if (go.name.StartsWith(bind.prefab.name))
                {
                    bind.instances.Add(go.gameObject);
                }
            }
        }


    }

    public void UpdatePrefabs()
    {
        InitGameObjectReferenceMap();

        GameObject tmpObj;
        List<GameObject> tmpDelInstances = new List<GameObject>();
        List<GameObject> tmpAddInstances = new List<GameObject>();

        foreach (var bind in prefabBinds)
        {
            tmpDelInstances.Clear();
            tmpAddInstances.Clear();

            foreach (var instance in bind.instances)
            {
                if (instance == null)
                {
                    continue;
                }
                //                tmpObj = NGUITools.AddChild(instance.transform.parent.gameObject,bind.prefab);
                tmpObj = PrefabUtility.InstantiatePrefab(bind.prefab) as GameObject;
                tmpObj.name = instance.name;
                tmpObj.transform.parent.BetterSetParent(instance.transform.parent);
                tmpObj.transform.LocalRotationEx(instance.transform.localRotation);
                tmpObj.transform.localScale = instance.transform.localScale;
                tmpObj.transform.localPosition = instance.transform.localPosition;
                tmpAddInstances.Add(tmpObj);

                List<SerializedProperty> tmpSPs = new List<SerializedProperty>();
                foreach (var reference in references)
                {
                    //这里sp.objectReferenceValue可能为空（已经替换过的）
                    if (reference == null)
                    {
                        continue;
                    }
                    if (reference.objectReferenceValue is Component && ((Component)reference.objectReferenceValue).gameObject.Equals(instance))
                    {
                        tmpSPs.Add(reference);
                    }
                    else if (reference.objectReferenceValue is GameObject && reference.objectReferenceValue.Equals(instance))
                    {
                        tmpSPs.Add(reference);
                    }
                }
                DestroyImmediate(instance);
                foreach (var tmpSP in tmpSPs)
                {
                    tmpSP.serializedObject.Update();
                    tmpSP.objectReferenceValue = tmpObj;
                    tmpSP.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        references.Clear();
    }


    public void InitGameObjectReferenceMap()
    {
        references = new List<SerializedProperty>();
        var components = GetComponents<Component>();

        foreach (var c in components)
        {
            if (c != null)
            {
                SerializedObject so = new SerializedObject(c);
                var sp = so.GetIterator();

                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue != null
                             && sp.name != "m_Script")
                        {
                            //                            Debug.Log("Add GameObject Reference:" + sp.name + "->" + sp.objectReferenceValue.name);
                            references.Add(sp.Copy());

                        }
                    }
                }//while end
            }
        }
    }
    //#endif

}
