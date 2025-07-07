///*******************************************************************
//** 文件名:	AssetConfigEditor.cs
//** 版  权:	(C) 深圳冰川网络技术有限公司
//** 创建人:	谌安
//** 日  期:	2017/6/7
//** 版  本:	1.0
//** 描  述:	WindowsConfig相关
//** 应  用:	1;快速赋值UIWindowsModelConfig中的classType属性
//*           2：因WindowModel越来越多，在制作新窗口的时候配置较麻烦，增加一个快速生成窗口

//********************************************************************/

//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using XGame.Effect;
//using XClient.Common;
////using UI.TestUIWindow;
//using rkt.Lua;
//using XGame.Asset.Loader;
//using XGame.EntityView.LightingEff;
//using XGame.

////[CustomEditor(typeof(UIWindowsModelConfig))]
////public class AssetConfigEditor : Editor
////{
////    private float interval = 2.0f;
////    public override void OnInspectorGUI()
////    {
////        if (GUILayout.Button("AddWindow"))
////        {
////            AsyncWindowConfig asyncWindowConfig = EditorWindow.GetWindow(typeof(AsyncWindowConfig)) as AsyncWindowConfig;

////            asyncWindowConfig.InitConfig(target as UIWindowsModelConfig);
////            asyncWindowConfig.Show();
////        }

////        if (GUILayout.Button("UpdateAsyncConfig"))
////        {
////            AsyncWindowConfig asyncWindowConfig = EditorWindow.GetWindow(typeof(AsyncWindowConfig)) as AsyncWindowConfig;

////            asyncWindowConfig.InitConfig(target as UIWindowsModelConfig, 1);
////            asyncWindowConfig.Show();
////        }

////        if (GUILayout.Button("Del Config"))
////        {
////            DelWindowConfig delConfig = EditorWindow.GetWindow(typeof(DelWindowConfig)) as DelWindowConfig;
////            delConfig.InitConfig(target as UIWindowsModelConfig);
////            delConfig.Show();
////        }


////        if (GUILayout.Button("Search"))
////        {
////            SearchWindowConfig searchConfig = EditorWindow.GetWindow(typeof(SearchWindowConfig)) as SearchWindowConfig;
////            searchConfig.InitConfig(target as UIWindowsModelConfig);
////            searchConfig.Show();
////        }

////        if (Application.isPlaying)
////        {
////            GUILayout.BeginVertical("box");
////            GUILayout.Label("Windows Auto Test");
////            interval = EditorGUILayout.Slider(new GUIContent("interval"), interval, 0, 15);
////            if (GUILayout.Button("Start WindowsTest"))
////            {
////                var configs = target as UIWindowsModelConfig;
////                var checkWindows = new List<WindowModel>(configs.WindowsModelConfig.Count);
////                foreach (var config in configs.WindowsModelConfig)
////                {
////                    checkWindows.Add(config.winModel);
////                }

////                UIWindowTester.Instance.intervalTime = interval;
////                UIWindowTester.Instance._windowModels = checkWindows;
////                var abCollect = UIWindowTester.Instance.gameObject.AddComponent<ABSnapShotCollector>();

////                abCollect.AttachUIWindows();

////                UIWindowTester.Instance.StartTest(); 
////            }

////            if (GUILayout.Button("Stop  WindowsTest"))
////            {
////                UIWindowTester.Instance.StopTest();
////            }
////            GUILayout.EndVertical();
////        }


////        DrawDefaultInspector();
////    }
////}

////[CustomEditor(typeof(UIConfig))]
////public class UIConfigEditor : Editor
////{

////    public override void OnInspectorGUI()
////    {
////        DrawDefaultInspector();
////        if (GUILayout.Button("UpdateResourceGuid"))
////        {
////            UIConfig config = target as UIConfig;

////            for (int i = 0; i < config.windows.Count; i++)
////            {
////                UIManager.Window winConfig = config.windows[i];

////                winConfig.prefab.UpdateResourceGuid();
////            }
////        }

////        if (GUILayout.Button("UpdateResoucePath"))
////        {
////            UIConfig config = target as UIConfig;

////            for (int i = 0; i < config.windows.Count; i++)
////            {
////                UIManager.Window winConfig = config.windows[i];

////                winConfig.prefab.UpdateResoucePath();
////            }
////        }
////    }
////}

////[CustomEditor(typeof(UIDrawPointConfig))]
////public class UIDrawPointConfigEditor : Editor
////{
////	string strFind = "";
////	string strOldFind = "";
////	string strRes = "";
////	EMDrawPointResType eDrawPoint;
////	public override void OnInspectorGUI()
////	{
////		DrawDefaultInspector();
////		if (GUILayout.Button("CheckResourceBg"))
////		{
////			UIDrawPointConfig config = target as UIDrawPointConfig;

////			for (int i = 0; i < config.lstConfigRes.Count; i++)
////			{
////				rkt.UI.MapDrawPointRes res = config.lstConfigRes[i];

////				res.imageBg.CheckResouce("imageBg "+ i + ":");
////			}
////		}

////		if (GUILayout.Button("CheckResourceIcon"))
////		{
////			UIDrawPointConfig config = target as UIDrawPointConfig;

////			for (int i = 0; i < config.lstConfigRes.Count; i++)
////			{
////				rkt.UI.MapDrawPointRes res = config.lstConfigRes[i];
				
////				res.imageIcon.CheckResouce("imageIcon " + i + ":");
////			}
////		}

////		if (GUILayout.Button("UpdateResourceGuid"))
////		{
////			UIDrawPointConfig config = target as UIDrawPointConfig;

////			for (int i = 0; i < config.lstConfigRes.Count; i++)
////			{
////				rkt.UI.MapDrawPointRes res = config.lstConfigRes[i];

////				res.imageBg.UpdateResourceGuid();
////				res.imageIcon.UpdateResourceGuid();
////			}
////			EditorUtility.SetDirty(target);
////			AssetDatabase.SaveAssets();
////		}

////		if (GUILayout.Button("UpdateResoucePath"))
////		{
////			UIDrawPointConfig config = target as UIDrawPointConfig;

////			for (int i = 0; i < config.lstConfigRes.Count; i++)
////			{
////				rkt.UI.MapDrawPointRes res = config.lstConfigRes[i];
////				string strOldGuid = res.imageBg.resourceGuid;
////				res.imageBg.UpdateResourceGuid(true);
////				if (res.imageBg.resourceGuid != strOldGuid)
////				{
////					Debug.LogError("UIDrawPointConfig UpdateResoucePath BG ptah=" + res.imageBg.path + strOldGuid + " -> " + res.imageBg.resourceGuid);
////				}
////				strOldGuid = res.imageIcon.resourceGuid;
////				res.imageIcon.UpdateResourceGuid(true);
////				if (res.imageIcon.resourceGuid != strOldGuid)
////				{
////					Debug.LogError("UIDrawPointConfig UpdateResoucePath  Icon ptah=" + res.imageIcon.path + strOldGuid + " ->"  + res.imageIcon.resourceGuid);
////				}
////			}
////			EditorUtility.SetDirty(target);
////			AssetDatabase.SaveAssets();
////		}

////		if (GUILayout.Button("Test"))
////		{
////			UIDrawPointConfig config = target as UIDrawPointConfig;

////			for (int i = 0; i < config.lstConfigRes.Count; i++)
////			{
////				rkt.UI.MapDrawPointRes res = config.lstConfigRes[i];
////				if (!string.IsNullOrEmpty(res.imageBg.path))
////				{
////					GResources.LoadAsync<Sprite>(res.imageBg.path, (path, sp) => {
////						Debug.LogError("UIDrawPointConfig Test Bg idx=" + i + " sp=" + (sp != null).ToString());
////					});
////				}
////				if (!string.IsNullOrEmpty(res.imageIcon.path))
////				{
////					GResources.LoadAsync<Sprite>(res.imageIcon.path, (path, sp) => {
////						Debug.LogError("UIDrawPointConfig Test Icon idx=" + i + " sp=" + (sp != null).ToString());
////					});
////				}
////			}
////			EditorUtility.SetDirty(target);
////			AssetDatabase.SaveAssets();
////		}

////		EditorGUILayout.BeginVertical("Box");

////		EditorGUILayout.BeginHorizontal("Box");
////		EditorGUILayout.LabelField("Input Enum DrawPoint", GUILayout.Width(100));
////		strFind = EditorGUI.TextField(EditorGUILayout.GetControlRect(), strFind);
////		EditorGUILayout.EndHorizontal();

////		EMDrawPointResType tmp = EMDrawPointResType.None;
////		if (!strFind.Equals(strOldFind))
////		{
////			try
////			{
////				tmp = (EMDrawPointResType)System.Enum.Parse(typeof(EMDrawPointResType), strFind);
////				eDrawPoint = tmp;
////			}
////			catch (System.Exception e)
////			{ Debug.Log(e); }
////			strOldFind = strFind;
////		}
////		eDrawPoint = (EMDrawPointResType)EditorGUILayout.EnumPopup("DrawPoint", eDrawPoint);

////		EditorGUILayout.EndVertical();

////		if (GUILayout.Button("Find"))
////		{
////			UIDrawPointConfig config = target as UIDrawPointConfig;
////			for (int i = 0; i < config.lstConfigRes.Count; i++)
////			{
////				rkt.UI.MapDrawPointRes res = config.lstConfigRes[i];
////				if (res.eType == eDrawPoint)
////				{
////					strRes = "所在位置：" + i.ToString();
////					break;
////				}
////			}
////		}

////		EditorGUILayout.BeginVertical("Box");
////		EditorGUILayout.LabelField(strRes, GUILayout.Width(1000));
////		EditorGUILayout.EndVertical();
////	}
////}

////[CustomEditor(typeof(LightingEffectHighLevelOptimizeConfig))]
////public class LightingEffectHighLevelOptimizeConfigEditor : Editor
////{
////    private bool bOnlyShowUsing=true;
////    public override void OnInspectorGUI()
////    {
////        DrawDefaultInspector();
////        if (Application.isPlaying)
////        {
////            if (GUILayout.Button("Update"))
////            {
////                if (XGameComs.Get<ILightingEffectCom>().lightingEffectFactory == null)
////                {
////                    Debug.LogError("LightingEffectFactory.Instance == null");
////                    return;
////                }
////                XGameComs.Get<ILightingEffectCom>().lightEffectOptimizedMgr.UpdateLightingEffectHighLevelOptimizeConfig();
////            }
////            EditorGUILayout.LabelField("CurLightingEffectOptimizeConfigs:");
////            Cur_Max[] configs = XGameComs.Get<ILightingEffectCom>().CurLightingEffectCount;
////            if (configs != null)
////            {
////                int Count = configs.Length;
////                for (int i = 0; i < Count; i++)
////                {
////                    LightingEffectRefType refType = (LightingEffectRefType)i;
////                    Cur_Max curConfig = configs[i];
////                    GUILayout.BeginHorizontal("box");
////                    EditorGUILayout.LabelField(refType.ToString(), string.Format("{0,3}/{1,3}", curConfig.nCurCount.ToString(), curConfig.nMaxCount.ToString()));
////                    if (GUILayout.Button("Check"))
////                    {
////                        GlacierEditor.EditorUtil.ClearConsole();
////                        foreach (var nodeInstaceID in XGameComs.Get<ILightingEffectCom>().lightEffectOptimizedMgr.RefType2ActiveEffectNodeInstanceID[i])
////                        {
////                            FeaturesGroupEx node = XGameComs.Get<ILightingEffectCom>().lightEffectOptimizedMgr.GetEffectNodeByInstaceID(nodeInstaceID);
////                            Debug.LogWarning(node.gameObject.ToString(), node);
////                        }
////                    }
////                    GUILayout.EndVertical();
////                }
////            }

////            EditorGUILayout.LabelField("UsedEffects:");
////            Dictionary<int, int> usedEffects = LightingEffectFactory.Instance.UsedEffects;
////            if (usedEffects != null)
////            {
////                GUILayout.BeginVertical("box");
////                foreach (var pair in usedEffects)
////                {
////                    if (pair.Value != 0)
////                        EditorGUILayout.LabelField(string.Format("EffectID:{0,3}->Num:{1,3}", pair.Key.ToString(), pair.Value.ToString()));
////                }
////                GUILayout.EndVertical();
////            }


////            EditorGUILayout.LabelField("EffectID2EffectNode:");
////            bOnlyShowUsing = GUILayout.Toggle(bOnlyShowUsing, "OnlyShowUsing");
////            Dictionary<int, List<Effect.EffectNode>> EffectID2EffectNode = LightEffectOptimizedMgr.Instance.EffectID2EffectNode;
////            if (EffectID2EffectNode != null)
////            {
////                GUILayout.BeginVertical("box");
////                foreach (var pair in EffectID2EffectNode)
////                {
////                    if (pair.Value == null)
////                        continue;
////                    var nodelst = pair.Value;
////                    if (bOnlyShowUsing)
////                    {
////                        nodelst = GetActiveEffectNode(nodelst);
////                    }
////                    if (nodelst == null)
////                        continue;
////                    GUILayout.BeginHorizontal("box");
////                    EditorGUILayout.LabelField(string.Format("EffectID:{0,3}->Num:{1,3}", pair.Key.ToString(), pair.Value.Count.ToString()));
////                    if (GUILayout.Button("Check"))
////                    {
////                        GlacierEditor.EditorUtil.ClearConsole();
////                        foreach (var node in nodelst)
////                        {

////                            Debug.LogWarning(node.gameObject.ToString(), node);
////                        }
////                    }
////                    GUILayout.EndHorizontal();
////                }
////                GUILayout.EndVertical();
////            }
////        }
////    }

////    List<EffectNode> GetActiveEffectNode(List<EffectNode> list)
////    {
////        List<EffectNode> rt=null;
////        foreach(var node in list)
////        {
////            if (node == null)
////                continue;
////            if (node.gameObject.activeInHierarchy)
////            {
////                if (rt == null)
////                    rt = new List<EffectNode>();
////                rt.Add(node);
////            }
////        }
////        return rt;
////    }
            


////    public override bool RequiresConstantRepaint()
////    {
////        return Application.isPlaying;
////    }

////    public void DebugLightingEffectFactoryCurEffect(int index,LightingEffectOptimizeConfig config)
////    {

////    }
////}


////public class AsyncWindowConfig : EditorWindow
////{
////    public UIWindowsModelConfig allData;
////    public UIManager.WindowModelConfig config =null;
////    private const string ConfigFileName = "UIAsyncConfig.asset";

////    #region 根节点

////    public string parentStrModel = "";

////    private string oldParentStr = "";
    
////    Object dir = null;
////    #endregion

////    #region 子节点数量 
////    int m_subNode = 0;
////    int m_oldNode = 0;
////    #endregion

////    #region 子节点

////    private List<string> curModelstr;

////    private List<string> oldModelStr;

////    private List<Object> subDir;

////    private List<string> subParentNode;

////    private List<string> oldsubParentNode;
////    #endregion

////    #region update 逻辑

////    public string upParentStrModel = "";

////    private string upOldParentStr = "";

////    private XClient.Common.WindowModel updateModel = XClient.Common.WindowModel.None;

////    #endregion

////    /// <summary>
////    /// 0:  添加  1：更改
////    /// </summary>
////    private int kind = 0;

////    private Vector2 scrollPosition;

////    public void InitConfig(UIWindowsModelConfig tC,int tkind = 0)
////    {
////        allData = tC;

////        kind = tkind;

////        config = new UIManager.WindowModelConfig();

////        config.prefab = new ResourceRef();

////        config.subNode = new List<UIManager.SonWindowNode>();

////        parentStrModel = "";

////        oldParentStr = "";

////        dir = null;

////        curModelstr = new List<string>();

////        oldModelStr = new List<string>();

////        subDir = new List<Object>();

////        subParentNode = new List<string>();

////        oldsubParentNode = new List<string>();

////        m_subNode = 0;
////        m_oldNode = 0;
////    }

////    #region Draw GUI
////    public void OnGUI()
////    {
////        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

////        if (kind == 1)
////        {
////            if (!IsStartUpdate())
////            {
////                EditorGUILayout.EndScrollView();
////                return;
////            }
////        }

////        if (kind == 1)
////        {
////            EditorGUILayout.Separator();
////        }
        
////        EditorGUILayout.BeginVertical("Box");

////        EditorGUILayout.BeginHorizontal("Box");

////        EditorGUILayout.LabelField("Input string To enum", GUILayout.Width(150));

////        parentStrModel = EditorGUI.TextField(EditorGUILayout.GetControlRect(), parentStrModel);

////        EditorGUILayout.EndHorizontal();

////        XClient.Common.WindowModel tmp = XClient.Common.WindowModel.None;
////        if (!parentStrModel.Equals(oldParentStr))
////        {
////            try
////            {
////                tmp = (XClient.Common.WindowModel)System.Enum.Parse(typeof(XClient.Common.WindowModel), parentStrModel);
////                config.winModel = tmp;
////            }
////            catch (System.Exception e)
////            {
////                Debug.Log(e);
////            }
            
////            oldParentStr = parentStrModel;
////        }
////        config.winModel = (XClient.Common.WindowModel)EditorGUILayout.EnumPopup("WidnowModel", config.winModel);

////        EditorGUILayout.BeginHorizontal("Box");
////        EditorGUILayout.LabelField("select prefab", GUILayout.Width(200));
////        EditorGUI.BeginChangeCheck();
////        dir = EditorGUILayout.ObjectField(dir, typeof(Object), false, GUILayout.Width(150));
////        if (EditorGUI.EndChangeCheck())
////        {
////            config.prefab.path = dir != null ? AssetDatabase.GetAssetPath(dir) : null;

////            config.prefab.path = config.prefab.path.Replace("Assets/GResources/", "");
////            config.prefab.path = config.prefab.path.Replace(".prefab", "");

////            string objPath = UnityEditor.AssetDatabase.GetAssetPath(dir);
////            config.prefab.resourceGuid = UnityEditor.AssetDatabase.AssetPathToGUID(objPath);

////            config.prefab.UpdateResourceGuid();

////            config.prefab.UpdateResoucePath();
////        }

////        EditorGUILayout.EndHorizontal();

////        EditorGUILayout.BeginVertical("Box");

////        config.changeSceneHide = EditorGUILayout.Toggle("Set Change Scene Hide", config.changeSceneHide);

////        parentStrModel = EditorGUI.TextField(EditorGUILayout.GetControlRect(), parentStrModel);

////        EditorGUILayout.EndVertical();


////        EditorGUILayout.BeginHorizontal();
////		EditorGUILayout.EndHorizontal();
////        EditorGUILayout.EndVertical();

////        EditorGUILayout.BeginHorizontal("Box");
        
////        EditorGUILayout.LabelField("Set SubNode Count", GUILayout.Width(150));
////        m_subNode = EditorGUILayout.IntField(m_subNode);
        
////        EditorGUILayout.EndHorizontal();

////        if (m_oldNode != m_subNode)
////        {
////            m_oldNode = m_subNode;

////            #region 移除超出的
////            if (config.subNode.Count > m_subNode)
////            {
////                config.subNode.RemoveRange(m_subNode, config.subNode.Count - m_subNode);

////                curModelstr.RemoveRange(m_subNode, config.subNode.Count - m_subNode);

////                oldModelStr.RemoveRange(m_subNode, config.subNode.Count - m_subNode);

////                subDir.RemoveRange(m_subNode, config.subNode.Count - m_subNode);

////                subParentNode.RemoveRange(m_subNode, config.subNode.Count - m_subNode);

////                oldsubParentNode.RemoveRange(m_subNode, config.subNode.Count - m_subNode);
////            }
////            #endregion
////        }

////        DrawNodeConfigView(m_subNode);

////        if (kind == 0)
////        {
////            if (GUILayout.Button("AddToData"))
////            {
////                allData.WindowsModelConfig.Add(config);

////                EditorUtility.SetDirty(allData);

////                this.Close();
////            }
////        }
////        else if (kind == 1)
////        {
////            if (GUILayout.Button("UpdateData"))
////            {
////                foreach (UIManager.WindowModelConfig tmpconfig in allData.WindowsModelConfig)
////                {
////                    if (tmpconfig.winModel.Equals(updateModel))
////                    {
////                        tmpconfig.winModel = config.winModel;

////                        tmpconfig.prefab = config.prefab;

////                        if (tmpconfig.subNode != null)
////                            tmpconfig.subNode.Clear();
////                        else
////                            tmpconfig.subNode = new List<UIManager.SonWindowNode>();

////                        for (int i = 0; i < config.subNode.Count; i++)
////                        {
////                            UIManager.SonWindowNode curSubNode = config.subNode[i];

////                            tmpconfig.subNode.Add(curSubNode);
////                        }

////                        break;
////                    }
////                }

////                EditorUtility.SetDirty(allData);

////                this.Close();
////            }
        
////        }

////        EditorGUILayout.EndScrollView();
////    }
////    #endregion

////    private bool IsStartUpdate()
////    {
        
////        EditorGUILayout.BeginVertical("Box");

////        EditorGUILayout.BeginHorizontal("Box");

////        EditorGUILayout.LabelField("Input string To enum(Update Modle)", GUILayout.Width(250));

////        upParentStrModel = EditorGUI.TextField(EditorGUILayout.GetControlRect(), upParentStrModel);

////        EditorGUILayout.EndHorizontal();

////        XClient.Common.WindowModel tmp = XClient.Common.WindowModel.None;

////        if (!upParentStrModel.Equals(upOldParentStr))
////        {
////            try
////            {
////                tmp = (XClient.Common.WindowModel)System.Enum.Parse(typeof(XClient.Common.WindowModel), upParentStrModel);
////                updateModel = tmp;

////                if (!updateModel.Equals(XClient.Common.WindowModel.None))
////                {
////                    foreach (UIManager.WindowModelConfig tmpconfig in allData.WindowsModelConfig)
////                    {
////                        #region 更新整个数据
////                        if (tmpconfig.winModel.Equals(updateModel))
////                        {
////                            config = new UIManager.WindowModelConfig();

////                            config.winModel = tmpconfig.winModel;

////                            config.changeSceneHide = tmpconfig.changeSceneHide;

////                            config.prefab = new ResourceRef();

////                            config.prefab.path = tmpconfig.prefab.path;

////                            config.prefab.UpdateResourceGuid();

////                            //config.prefab.UpdateResoucePath();

////                            config.prefab.resourceGuid = tmpconfig.prefab.resourceGuid;

////                            config.subNode = new List<UIManager.SonWindowNode>();

////                            foreach (UIManager.SonWindowNode t in tmpconfig.subNode)
////                            {
////                                UIManager.SonWindowNode tt = new UIManager.SonWindowNode();

////                                tt.winModel = t.winModel;

////                                tt.prefab = new ResourceRef();

////                                tt.prefab.path = t.prefab.path;
                                
////                                tt.prefab.UpdateResourceGuid();

////                                tt.prefab.resourceGuid = t.prefab.resourceGuid;

////                                //tt.prefab.UpdateResoucePath();

////                                tt.parentPath = t.parentPath;

////                                tt.parentModel = t.parentModel;

////                                config.subNode.Add(tt);
////                            }

////                            parentStrModel = config.winModel.ToString();

////                            oldParentStr = config.winModel.ToString();
                            
////                            string s1 = config.prefab.path;

////                            s1 = "Assets/GResources/" + s1;

////                            s1 = s1 + ".prefab";

////                            dir = AssetDatabase.LoadAssetAtPath(s1, typeof(Object));

////                            config.prefab.UpdateResoucePath();

////                            config.prefab.UpdateResourceGuid();

////                            if (config.subNode != null)
////                            {
////                                m_subNode = config.subNode.Count;

////                                curModelstr = new List<string>();

////                                oldModelStr = new List<string>();

////                                subDir = new List<Object>();

////                                subParentNode = new List<string>();

////                                oldsubParentNode = new List<string>();

////                                for (int i = 0; i < config.subNode.Count; i++)
////                                {
////                                    curModelstr.Add(config.subNode[i].winModel.ToString());
////                                    oldModelStr.Add(config.subNode[i].winModel.ToString());

////                                    subParentNode.Add(config.subNode[i].winModel.ToString());
////                                    oldsubParentNode.Add(config.subNode[i].winModel.ToString());

////                                    string s = config.subNode[i].prefab.path;

////                                    s = "Assets/GResources/" + s;

////                                    s = s + ".prefab";
                                    
////                                    subDir.Add(AssetDatabase.LoadAssetAtPath(s, typeof(Object)));

////                                    config.subNode[i].prefab.UpdateResoucePath();

////                                    config.subNode[i].prefab.UpdateResourceGuid();
////                                }
////                            }
////                            else
////                                m_subNode = 0;
////                            break;
////                        }
////                        #endregion
////                    }
////                }
////            }
////            catch (System.Exception e)
////            { Debug.Log(e); }

////            upOldParentStr = upParentStrModel;
////        }
////        updateModel = (XClient.Common.WindowModel)EditorGUILayout.EnumPopup("WidnowModel", updateModel);


////        return updateModel != XClient.Common.WindowModel.None;
////    }

////    //通用路径配置Item绘制
////    private void DrawNodeConfigView(int count)
////    {
////        for (int i = 0; i < count; i++)
////        {
////            UIManager.SonWindowNode sonWinConfig = null;
            
////            Object curDir = null;
////            if (config.subNode.Count <= i)
////            {
////                sonWinConfig = new UIManager.SonWindowNode();

////                sonWinConfig.prefab = new ResourceRef();

////                config.subNode.Add(sonWinConfig);

////                curModelstr.Add("");

////                oldModelStr.Add("");

////                curDir = new Object();

////                subDir.Add(curDir);

////                subParentNode.Add("");

////                oldsubParentNode.Add("");
////            }
////            else
////            {
////                sonWinConfig = config.subNode[i];
////            }
////            EditorGUILayout.BeginVertical("Box");

////            EditorGUILayout.LabelField("sub " + i + "----------------------------------------------------------------");

////            EditorGUILayout.BeginHorizontal("Box");
////            EditorGUILayout.LabelField("Input string To enum", GUILayout.Width(150));

////            curModelstr[i] = EditorGUI.TextField(EditorGUILayout.GetControlRect(), curModelstr[i]);
////            EditorGUILayout.EndHorizontal();

////            XClient.Common.WindowModel tmp = XClient.Common.WindowModel.None;
////            if (!curModelstr[i].Equals(oldModelStr[i]))
////            {
////                try
////                {
////                    tmp = (XClient.Common.WindowModel)System.Enum.Parse(typeof(XClient.Common.WindowModel), curModelstr[i]);
////                    sonWinConfig.winModel = tmp;
////                }
////                catch (System.Exception e)
////                { Debug.Log(e); }

////                oldModelStr[i] = curModelstr[i];
////            }
////            sonWinConfig.winModel = (XClient.Common.WindowModel)EditorGUILayout.EnumPopup("WidnowModel", sonWinConfig.winModel);

////            EditorGUILayout.BeginHorizontal("Box");
////            EditorGUILayout.LabelField("select prefab", GUILayout.Width(200));

////            EditorGUI.BeginChangeCheck();
////            subDir[i] = EditorGUILayout.ObjectField(subDir[i], typeof(Object), false, GUILayout.Width(150));
////            if (EditorGUI.EndChangeCheck())
////            {
////                sonWinConfig.prefab.path = subDir[i] != null ? AssetDatabase.GetAssetPath(subDir[i]) : null;

////                sonWinConfig.prefab.path = sonWinConfig.prefab.path.Replace("Assets/GResources/", "");

////                sonWinConfig.prefab.path = sonWinConfig.prefab.path.Replace(".prefab", "");


////                string objPath = UnityEditor.AssetDatabase.GetAssetPath(subDir[i]);

////                sonWinConfig.prefab.resourceGuid = UnityEditor.AssetDatabase.AssetPathToGUID(objPath);

////                sonWinConfig.prefab.UpdateResourceGuid();

////                sonWinConfig.prefab.UpdateResoucePath();
////            }


////            EditorGUILayout.EndHorizontal();

////			EditorGUILayout.BeginHorizontal();
////			EditorGUILayout.EndHorizontal();

////            EditorGUILayout.BeginHorizontal("Box");

////            EditorGUILayout.LabelField("Parent Path", GUILayout.Width(150));

////            sonWinConfig.parentPath = EditorGUI.TextField(EditorGUILayout.GetControlRect(), sonWinConfig.parentPath);

////            EditorGUILayout.EndHorizontal();

////            EditorGUILayout.BeginHorizontal("Box");
////            EditorGUILayout.LabelField("Input string To enum", GUILayout.Width(150));

////            subParentNode[i] = EditorGUI.TextField(EditorGUILayout.GetControlRect(), subParentNode[i]);
////            EditorGUILayout.EndHorizontal();

////            XClient.Common.WindowModel tmp1 = XClient.Common.WindowModel.None;
////            if (!subParentNode[i].Equals(oldsubParentNode[i]))
////            {
////                try
////                {
////                    tmp1 = (XClient.Common.WindowModel)System.Enum.Parse(typeof(XClient.Common.WindowModel), subParentNode[i]);
////                    sonWinConfig.parentModel = tmp1;
////                }
////                catch (System.Exception e)
////                { Debug.Log(e); }

////                oldsubParentNode[i] = subParentNode[i];
////            }
////            sonWinConfig.parentModel = (XClient.Common.WindowModel)EditorGUILayout.EnumPopup("ParentModel", sonWinConfig.parentModel);


////            EditorGUILayout.EndVertical();
////        }
////    }

////    private Object GetObjectByPath(string path)
////    {
////        string s1 = path;

////        s1 = "Assets/GResources/" + s1;

////        s1 = s1 + ".prefab";

////        Object tmpDir = AssetDatabase.LoadAssetAtPath(s1, typeof(Object));

////        return tmpDir;
////    }
////}

////public class DelWindowConfig : EditorWindow
////{
////    public int delIdx = -1;
    
////    public string parentStrModel = "";
    
////    private string oldParentStr = "";

////    public XClient.Common.WindowModel delModel = XClient.Common.WindowModel.None;

////    public UIWindowsModelConfig allData;

////    public void InitConfig(UIWindowsModelConfig tC)
////    {
////        allData = tC;
////    }


////    #region Draw GUI
////    public void OnGUI()
////    {
////        EditorGUILayout.BeginHorizontal("Box");

////        EditorGUILayout.LabelField("Del By Idx(default -1)", GUILayout.Width(150));

////        delIdx = EditorGUI.IntField(EditorGUILayout.GetControlRect(), delIdx);

////        EditorGUILayout.EndHorizontal();

////        EditorGUILayout.BeginVertical("Box");

////        EditorGUILayout.BeginHorizontal("Box");

////        EditorGUILayout.LabelField("Input string To enum", GUILayout.Width(150));

////        parentStrModel = EditorGUI.TextField(EditorGUILayout.GetControlRect(), parentStrModel);

////        EditorGUILayout.EndHorizontal();

////        XClient.Common.WindowModel tmp = XClient.Common.WindowModel.None;
////        if (!parentStrModel.Equals(oldParentStr))
////        {
////            try
////            {
////                tmp = (XClient.Common.WindowModel)System.Enum.Parse(typeof(XClient.Common.WindowModel), parentStrModel);
////                delModel = tmp;
////            }
////            catch (System.Exception e)
////            { Debug.Log(e); }

////            oldParentStr = parentStrModel;
////        }
////        delModel = (XClient.Common.WindowModel)EditorGUILayout.EnumPopup("WidnowModel", delModel);


////        if (GUILayout.Button("Delete"))
////        {
////            if (delIdx != -1)
////            {
////                if (allData.WindowsModelConfig.Count > delIdx)
////                {
////                    allData.WindowsModelConfig.RemoveAt(delIdx);

////                    EditorUtility.SetDirty(allData);

////                    this.Close();
////                }
////            }
////            else if (delModel != XClient.Common.WindowModel.None)
////            {
////                foreach (UIManager.WindowModelConfig config in allData.WindowsModelConfig)
////                {
////                    if (config.winModel.Equals(delModel))
////                    {
////                        allData.WindowsModelConfig.Remove(config);

////                        EditorUtility.SetDirty(allData);

////                        this.Close();

////                        break;
////                    }
////                }
////            }
////        }
////    }
////    #endregion
////}

////public class SearchWindowConfig : EditorWindow
////{

////    public string parentStrModel = "";

////    private string oldParentStr = "";

////    private string resultStr = "";

////    public XClient.Common.WindowModel searchModel = XClient.Common.WindowModel.None;

////    public UIWindowsModelConfig allData;

////    public void InitConfig(UIWindowsModelConfig tC)
////    {
////        allData = tC;

////        resultStr = "";
////    }


////    #region Draw GUI
////    public void OnGUI()
////    {

////        EditorGUILayout.BeginVertical("Box");

////        EditorGUILayout.BeginHorizontal("Box");

////        EditorGUILayout.LabelField("Input string To enum", GUILayout.Width(150));

////        parentStrModel = EditorGUI.TextField(EditorGUILayout.GetControlRect(), parentStrModel);

////        EditorGUILayout.EndHorizontal();

////        XClient.Common.WindowModel tmp = XClient.Common.WindowModel.None;

////        if (!parentStrModel.Equals(oldParentStr))
////        {
////            try
////            {
////                tmp = (XClient.Common.WindowModel)System.Enum.Parse(typeof(XClient.Common.WindowModel), parentStrModel);
////                searchModel = tmp;
////            }
////            catch (System.Exception e)
////            { Debug.Log(e); }

////            oldParentStr = parentStrModel;
////        }
////        searchModel = (XClient.Common.WindowModel)EditorGUILayout.EnumPopup("WidnowModel", searchModel);


////        if (GUILayout.Button("Search"))
////        {
////            if (searchModel != XClient.Common.WindowModel.None)
////            {
////                string s = "";

////                for (int i = 0; i < allData.WindowsModelConfig.Count; i++)
////                {
////                    UIManager.WindowModelConfig t = allData.WindowsModelConfig[i];

////                    if (t.winModel.Equals(searchModel))
////                    {
////                        s += searchModel + "所在位置:第"+i+"索引";
////                        //break;
////                    }

////                    for (int j = 0; j < t.subNode.Count; j++)
////                    {
////                        if (t.subNode[j].winModel.Equals(searchModel))
////                        {
////                            s += searchModel + "所在位置:第" + i + "索引."+"  第"+j+"子节点";
////                            //break;
////                        }
////                    }
////                }
////                resultStr = s;
////            }
////        }

////        EditorGUILayout.BeginHorizontal("Box");

////        EditorGUILayout.LabelField(resultStr, GUILayout.Width(3000));

////        EditorGUILayout.EndHorizontal();

////    }
////    #endregion
////}