using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XGame.Anim;
using System.IO;
using CommonScript;
using XClient.Common;
using UnityEngine.UI;
using static XGame.Asset.ResUtil;
using XGameEditor.Config;
using XGame.UI.Framework;
using XGame;
using XGameEditor.CodeGenerator;

namespace XGameEditor.UI
{
    [CustomEditor(typeof(GamePlayConfig))]
    public class GamePlayEditor : Editor
    {
        //资源配置位置
        static string gameConfigPath = "Assets/G_Resources/App/GameConfig.asset";

        //UI开发canvas 目录
        static string canvasEditorPath = "Assets/G_Resources/App/UIEditorCanvas.prefab";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            if (GUILayout.Button("切换游戏主体"))
            {

                GamePlayConfig gamePlayConfig = (GamePlayConfig)target;
                int setupIndex = gamePlayConfig.setupGamePlayIndex;
                if(setupIndex< gamePlayConfig.gamePlayItems.Count)
                {
                    GamePlayItem item = gamePlayConfig.gamePlayItems[setupIndex];

                    if(null!= item)
                    {
                        //1.切换游戏资源路径
                        GameConfig gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>(gameConfigPath);
                        gameConfig.gameResDir = item.gameResDir;
                        gameConfig.strNamespace = item.strNamespace;
                        gameConfig.gameScriptDir = item.gameScriptDir;
                        EditorUtility.SetDirty(gameConfig);

                        //2.切换运行时的分辨率
                        SetupResotion(gamePlayConfig.gameObject, item.resolutionWidth, item.resolutionHeight);


                        //3.切换UI编辑canvas的分辨率
                        GameObject canvasObject = AssetDatabase.LoadAssetAtPath<GameObject>(canvasEditorPath);
                        SetupResotion(canvasObject, item.resolutionWidth, item.resolutionHeight);

                        //4.设置启动场景
                        SetupStartupScene(item);

                        //5.设置UI窗口启动配置
                        SetupUIFramework(gamePlayConfig.gameObject,item);

                        //6.设置自动生成代码配置
                        SetupCodeGenConfig(gameConfig, item);
                    }

                }


                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
            }
        }


        private void SetupResotion(GameObject go,int w,int h)
        {

            if(null== go)
            {
                return;
            }

            Vector2 referenceResolution = new Vector2(w,h);
            CanvasScaler[] canvasScalers = go.GetComponentsInChildren<CanvasScaler>(true);  
            int len = canvasScalers.Length; 
            for(int i=0;i<len;++i)
            {
                canvasScalers[i].referenceResolution = referenceResolution;
            }

            EditorUtility.SetDirty(go);
        }

        private void SetupStartupScene(GamePlayItem item)
        {
            EditorBuildSettingsScene[] scenes =  EditorBuildSettings.scenes;
            if(scenes.Length>=2)
            {
                scenes[1].enabled = true;
                string path = "Assets/"+ item.setupScenePath.path;
                scenes[1].path = path;
                scenes[1].guid = new GUID(AssetDatabase.AssetPathToGUID(path));
                EditorBuildSettings.scenes = scenes;
            }
        }

        private void SetupUIFramework(GameObject go,GamePlayItem item)
        {
            //UIFrameworkSettings fs = go.GetComponent<UIFrameworkSettings>();
            //if(null!= fs)
            //{
            //    string uiframeDataPath = CodeGeneratorEditorHelper.GetUIFramewordSettingDataSavePath();
            //    UIFrameworkSettingData data = AssetDatabase.LoadAssetAtPath<UIFrameworkSettingData>(uiframeDataPath);
            //    if(null!= data)
            //    {
            //        fs.m_Settings = data;
            //    }
            //}
        }

        private void SetupCodeGenConfig(GameConfig gameConfig, GamePlayItem item)
        {
            //CodeGeneratorSetting codeGeneratorSetting = CodeGeneratorSetting.Instance;
            //codeGeneratorSetting.prefabFileBaseDir = "Assets/"+gameConfig.gameResDir+ "/Prefabs/UI";
            //codeGeneratorSetting.normalMoudelDir = "Assets/" + gameConfig.gameScriptDir + "/Modules";
            //codeGeneratorSetting.csCodeFileRootDir = "Assets/" + gameConfig.gameScriptDir;
            //codeGeneratorSetting.uiCodeFileDir = "Assets/" + gameConfig.gameScriptDir+ "/UI";
            //EditorUtility.SetDirty(codeGeneratorSetting);


        }
    }
}
