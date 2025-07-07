using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using ReferenceFinder;
using UnityEngine.UI;
using XGame;
using TMPro;
using XGame.I18N;
using DG.Tweening;
using static DG.Tweening.DOTweenAnimation;

public class DoTweenTemplateWindow : EditorWindow
{

    //模板节点
    class TempNode
    {
        public int nTempID;
        public bool bPlayBackward;
        public bool bRecover;
        public GameObject tempGo;
        public DOTweenAnimationEx ani;
        public bool bEnable;

    }

    //预制体检查的目录
    string m_detectPrefabFolder = "Assets/XGameEditor/Editor/AssetsTool/DoTween";

    //被替换的目录
    string m_replaceFolder = "Assets/G_Resources/Game/Prefab/LuaUI";

    //预制体列表
    List<GameObject> m_TemplatesList;

    //模板替换列表
    List<TempNode> m_TempNodeList;

    private bool m_bUpdateTempNode = true;

    private GameObject m_goReplaceTagert;



    [MenuItem("XGame/资源工具/替换UI动画工具")]
    static void ShowWindow()
    {
        GetWindow<DoTweenTemplateWindow>();
    }

    public DoTweenTemplateWindow()
    {
        this.titleContent = new GUIContent("UI替换动画工具");
        this.minSize = new Vector2(750f, 512f);
    }

    private void OnEnable()
    {
        m_bUpdateTempNode = true;
        m_TempNodeList = new List<TempNode>();
        m_TemplatesList = new List<GameObject>();
        string[] filePaths = Directory.GetFiles(m_detectPrefabFolder, "*.prefab", SearchOption.AllDirectories);
        for (int i = 0; i < filePaths.Length; ++i)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(filePaths[i]);   //PrefabUtility.LoadPrefabContents(filePaths[i]);
            m_TemplatesList.Add(go);
        }


    }






    private void OnGUI()
    {

        if (GUILayout.Button("增加模板预制体（+）"))
        {
            m_bUpdateTempNode = true;
            m_TemplatesList.Add(null);
        }
        GUILayout.Space(16f);
        for (int i = 0; i < m_TemplatesList.Count; i++)
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                m_TemplatesList[i] = EditorGUILayout.ObjectField(m_TemplatesList[i], typeof(GameObject), true) as GameObject;
                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    m_bUpdateTempNode = true;
                    m_TemplatesList.RemoveAt(i);
                }
            }

        }


       

        if (GUILayout.Button("刷新模板"))
        {
            m_bUpdateTempNode = true;
        }

        //更新模板节点
        UpdateTempNode();

        //显示模板节点
        for (int i = 0; i < m_TempNodeList.Count; ++i)
        {
            string name = $"模板ID： { m_TempNodeList[i].nTempID}, {m_TempNodeList[i].tempGo.name}  ";
            m_TempNodeList[i].bEnable = EditorGUILayout.Toggle(name, m_TempNodeList[i].bEnable, GUILayout.Height(20f), GUILayout.Width(460f));
        }


        //单个替换
        m_goReplaceTagert = EditorGUILayout.ObjectField(m_goReplaceTagert, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("单个替换"))
        {
            if (null != m_goReplaceTagert)
            {
                string path = AssetDatabase.GetAssetPath(m_goReplaceTagert);
                ReplaceOnePrefab(path);
            }

        }



        //替换的UI目录
        DrawFolderEdit("替换目录", ref m_replaceFolder);


        if (GUILayout.Button("批量替换"))
        {
            ReplaceTemplate();
        }


       

    }

    void UpdateTempNode()
    {
        if (false == m_bUpdateTempNode)
        {
            return;
        }

        m_bUpdateTempNode = false;
        List<TempNode> oldNodeList = m_TempNodeList;
        m_TempNodeList = new List<TempNode>();

        for (int i = 0; i < m_TemplatesList.Count; ++i)
        {
            if (null == m_TemplatesList[i])
            {
                continue;
            }

            DOTweenAnimationEx[] doTweenAnis = m_TemplatesList[i].GetComponentsInChildren<DOTweenAnimationEx>();
            for (int j = 0; j < doTweenAnis.Length; ++j)
            {
                DOTweenAnimationEx ani = doTweenAnis[j];
                if (ani.templateID == 0)
                {
                    continue;
                }

                TempNode node = new TempNode();

                node.nTempID = ani.templateID;
                node.bPlayBackward = ani.playBackward;
                node.bRecover = ani.recover;
                node.tempGo = m_TemplatesList[i];
                node.ani = ani;
                node.bEnable = true;

                //同步是否同步状态
                for (int n = 0; n < oldNodeList.Count; ++n)
                {
                    if (oldNodeList[i].nTempID == node.nTempID && oldNodeList[i].tempGo == node.tempGo)
                    {
                        node.bEnable = oldNodeList[i].bEnable;
                        break;
                    }
                }
                m_TempNodeList.Add(node);
            }

        }
    }


    private void DrawFolderEdit(string label, ref string result, bool isFolder = true, bool isCheckExists = true)
    {
        using (var hor = new GUILayout.HorizontalScope())
        {
            result = EditorGUILayout.TextField(label, result);
            if (GUILayout.Button("浏览"))
            {
                result = isFolder ? EditorUtility.OpenFolderPanel(label, result, "") :
                    EditorUtility.OpenFilePanel(label, result, "");
            }
            if (isCheckExists && ((isFolder && !Directory.Exists(result)) || (!isFolder && !File.Exists(result))))
            {
                GUILayout.Box("", "Wizard Error", GUILayout.Width(32), GUILayout.Height(16));
            }
        }

    }





    void ReplaceTemplate()
    {
        string[] filePaths = Directory.GetFiles(m_replaceFolder, "*.prefab", SearchOption.AllDirectories);
        for (int i = 0; i < filePaths.Length; ++i)
        {
            ReplaceOnePrefab(filePaths[i]);
        }
    }

    void ReplaceOnePrefab(string path)
    {
        GameObject go = PrefabUtility.LoadPrefabContents(path);

        if(null== go)
        {
            Debug.LogError("资源不存在:" + path);
            return;
        }

        DOTweenAnimationEx[] anis = go.GetComponentsInChildren<DOTweenAnimationEx>();
        bool bModified = false;
        for (int n = 0; n < anis.Length; ++n)
        {
            DOTweenAnimationEx replaceAni = anis[n];
            for (int j = 0; j < m_TempNodeList.Count; ++j)
            {
                //没有启用的，跳过
                if (m_TempNodeList[j].bEnable == false)
                {
                    continue;
                }

                DOTweenAnimationEx tempAni = m_TempNodeList[j].ani;
                if (tempAni.templateID == replaceAni.templateID)
                {
                    Vector3 keepValue = replaceAni.endValueV3;
                    UnityEditorInternal.ComponentUtility.CopyComponent(tempAni);
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(replaceAni);

                    if (replaceAni.animationType == AnimationType.Move)
                    {
                        replaceAni.endValueV3 = keepValue;
                    }

                    bModified = true;

                }


            }


        }

        if (bModified)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }

        PrefabUtility.UnloadPrefabContents(go);

    }




}









