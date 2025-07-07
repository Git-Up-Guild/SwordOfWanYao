/*******************************************************************
** 文件名: AssetBundleBuildConfig.cs
** 版  权:    (C) 深圳冰川网络技术有限公司 
** 创建人:     郑秀程
** 日  期:    2016/3/29
** 版  本:    1.0
** 描  述:    AssetBundle打包配置类
** 应  用:    用来记录AssetBundle打包的相关配置

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AssetBundleIDMap : ScriptableObject
{
    [System.Serializable]
    public class IndexData
    {
        public string abName;
        public int abIndex;

        public IndexData(string abName, int abIndex)
        {
            this.abName = abName;
            this.abIndex = abIndex;
        }
    }

    public List<IndexData> indexDataList;

    private Dictionary<string, int> m_ABToIDMap;        //AB-ID
    private Dictionary<int, string> m_IDToABMap;    //ID-AB
    private const int ID_START = 100;      //预留一部分ID用来处理特殊的文件

    private static AssetBundleIDMap m_Instance;
    private const string AssetBUndleIDMapFilePath = "Assets/Editor/Export/ABIDMap.asset";


    public static string GetABNameFromID(int assetBundleID)
    {
        return GetInstance().GetAssetBundleNameFromID(assetBundleID);
    }

    public static int GetABID(string assetBundleName, bool genrateWhenNotExist = false)
    {        
        return GetInstance().GetAssetBundleID(assetBundleName, genrateWhenNotExist);
    }

    public static void MarkDirty()
    {
        AssetBundleIDMap map = GetInstance();
        EditorUtility.SetDirty(map);      
    }

    public void InitData()
    {
        m_ABToIDMap = new Dictionary<string, int>();
        m_IDToABMap = new Dictionary<int, string>();
        if (indexDataList != null) {
            for (int i = 0; i < indexDataList.Count; i++) {
                m_ABToIDMap.Add(indexDataList[i].abName, indexDataList[i].abIndex);
                m_IDToABMap.Add(indexDataList[i].abIndex, indexDataList[i].abName);
            }
        }
    }

    public int GetAssetBundleID(string assetBundleName, bool genrateWhenNotExist = false)
    {
        string finalName = assetBundleName.ToLower();
        int abID = -1;
        if (!m_ABToIDMap.TryGetValue(finalName, out abID) && genrateWhenNotExist) {
            abID = GenrateAssetBundleID();
            m_ABToIDMap.Add(finalName, abID);
            m_IDToABMap.Add(abID, finalName);
            indexDataList.Add(new IndexData(finalName, abID));
        }
        return abID;
    }

    public string GetAssetBundleNameFromID(int assetBundleID)
    {
        string abName = null;
        m_IDToABMap.TryGetValue(assetBundleID, out abName);
        return abName;
    }

    //预留成Int型，方便后续扩展成其他ID生成方式
    private int GenrateAssetBundleID()
    {
        return ID_START + m_ABToIDMap.Count;
    }


    private static AssetBundleIDMap GetInstance()
    {
        if (m_Instance == null) {           
            m_Instance = AssetDatabase.LoadAssetAtPath<AssetBundleIDMap>(AssetBUndleIDMapFilePath);
            if (m_Instance == null) {
                m_Instance = ScriptableObject.CreateInstance<AssetBundleIDMap>();
                if (!Directory.Exists(AssetBUndleIDMapFilePath))
                {
                    Directory.CreateDirectory(AssetBUndleIDMapFilePath);
                }
                AssetDatabase.CreateAsset(m_Instance, AssetBUndleIDMapFilePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            m_Instance.InitData();
        }
        return m_Instance;
       
    }
}
