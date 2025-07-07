/*******************************************************************
** 文件名:	GameProjectSettings.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程 
** 日  期:	2024/10/30
** 版  本:	1.0
** 描  述:	
** 应  用:  	游戏项目设置
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using XClient.GameInit;
using XGame;
using XGame.Attr;
using XGame.Utils;

namespace XClient.Game
{
    [CreateAssetMenu(menuName = "XGame/项目配置")]
    public class GameProjectSettings : ScriptableObject
    {
        [Serializable]
        public class Settings
        {
            [Label("名称")]
            public string name;

            [Label("代码根目录")]
            public string codeBaseDir = "GameScripts";

            [Label("资源根目录")]
            public string resBaseDir = "GameResources";

            [Label("全局命名空间")]
            public string globalNameSpace = "GameScripts";

            [Label("项目命名空间")]
            public string projectNameSpace = string.Empty;

            [Label("分辨率宽")]
            public int resolutionWidth = 1080;

            [Label("分辨率高")]
            public int resolutionHeight = 1920;

            //启动场景名字
            [Label("启动场景")]
            public string setupScenePath;


            [Label("游戏初始化配置")]
            public GameInitConfig gameInitConfig;
        }

        [Label("工作目录")]
        public string workSpace;

        [ALabel("游戏管理预制体")]
        public ResourceRef gameManagerPrefabRef;

        [Label("项目列表")]
        [SerializeField]
        public List<Settings> settings = new List<Settings>();

        [HideInInspector]
        public string currentProjectName;

        //当前的项目配置
        public Settings currentProjectSettings
        {
            get
            {
                if (settings == null)
                    return null;
                foreach(var s in settings)
                {
                    if (s.name == currentProjectName)
                        return s;
                }
                return null;
            }
        }

    }

}
