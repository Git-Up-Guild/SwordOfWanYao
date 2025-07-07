using CommonScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using XClient.Common;
using XClient.Game;
using XClient.GameInit;
using XGame;
using XGame.UI.Framework;
using XGame.Utils;
using XGameEditor.CodeGenerator;
using XGameEditor.Utils;

namespace XGameEditor
{
    [CustomEditor(typeof(GameProjectSettings))]
    public class GameProjectSettingsEditor : Editor
    {
        private GameProjectSettings _projs;

        private bool _isCreateMode = false;

        private GameProjectSettings.Settings _newSettings = new GameProjectSettings.Settings();

        private SerializedProperty _workSpace;
        private SerializedProperty _gameManagerPrefabRef;
        private SerializedProperty _currentProjectName;

        private string _viewProjName = string.Empty;

        private SerializeObjectFieldsDrawer _fieldDrawer;


        //资源配置位置
        static string gameConfigPath = "Assets/G_Resources/App/GameConfig.asset";

        //UI开发canvas 目录
        static string canvasEditorPath = "Assets/G_Resources/App/UIEditorCanvas.prefab";

        //切换项目成功,等待确定
        private bool sucWaitCommfirm = false;


        private void OnEnable()
        {
            _projs = (GameProjectSettings)target;
            _isCreateMode = false;

            _viewProjName = string.Empty;

            _workSpace = serializedObject.FindProperty("workSpace");
            _gameManagerPrefabRef = serializedObject.FindProperty("gameManagerPrefabRef");
            _currentProjectName = serializedObject.FindProperty("currentProjectName");
        }

        private void OnDisable()
        {
            _fieldDrawer?.Clear();
            _viewProjName = string.Empty;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //创建模式
            if(_isCreateMode)
            {
                OnNewProjectGUI();
                return;
            }

            //根目录
            EditorGUILayout.PropertyField(_workSpace);

            //游戏预制体路径
            EditorGUILayout.PropertyField(_gameManagerPrefabRef);

            if(string.IsNullOrEmpty(_projs.workSpace) ||
                string.IsNullOrEmpty(_projs.gameManagerPrefabRef.path))
            {
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.HelpBox("设置错误，请检查！", MessageType.Warning);
                return;
            }

            //项目列表
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("项目列表");

            string deleteProjName = string.Empty;
            string activeProjName = string.Empty;
            string viewProjName = string.Empty;

            var actProj = _projs.currentProjectSettings;

            foreach(var s in _projs.settings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("名称", GUILayout.Width(60));

                GUI.enabled = false;
                EditorGUILayout.TextField(s.name);
                GUI.enabled = true;

                if (GUILayout.Button("查看", GUILayout.Width(60)))
                {
                    viewProjName = s.name;
                }

                bool isActive = actProj != null && actProj.name == s.name;
                GUI.enabled = !isActive;

                if (isActive)
                {
                    if (GUILayout.Button("已激活", GUILayout.Width(60))) { }
                }
                else
                {
                    if (GUILayout.Button("激活", GUILayout.Width(60)))
                    {
                        activeProjName = s.name;
                    }
                }

                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    deleteProjName = s.name;
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("新建项目"))
            {
                _isCreateMode = true;
                ClearSettings();
            }

            if (!string.IsNullOrEmpty(deleteProjName))
                DeleteProject(deleteProjName);

            if (!string.IsNullOrEmpty(activeProjName))
                ActiveProject(activeProjName);

            OnViewProjectGUI(viewProjName);

            serializedObject.ApplyModifiedProperties();

            if(sucWaitCommfirm)
            {
                sucWaitCommfirm = false;
                EditorUtility.DisplayDialog("提示", "激活成功！", "好的");
            }
        }

        private void OnViewProjectGUI(string newProjName)
        {
           if(string.IsNullOrEmpty(newProjName) && string.IsNullOrEmpty(_viewProjName))
            {
                _fieldDrawer?.Clear();
                return;
            }

            if (_fieldDrawer == null)
                _fieldDrawer = new SerializeObjectFieldsDrawer();

            if (!string.IsNullOrEmpty(newProjName))
            {
                _viewProjName = newProjName;

                _fieldDrawer?.Clear();

                int index = -1;
                for (var i = 0; i < _projs.settings.Count; i++)
                {
                    var s = _projs.settings[i];
                    if (s.name == _viewProjName)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    _viewProjName = string.Empty;
                    _fieldDrawer.Clear();
                    return;
                }


                _fieldDrawer.Setup(_projs, "settings", index, new List<string>() {
                            "name", "codeBaseDir", "resBaseDir", "globalNameSpace",
                            "projectNameSpace",  "gameInitConfig","resolutionWidth","resolutionHeight","setupScenePath"}, (name, isStart) =>
                            {
                                if (isStart && (name == "gameInitConfig"|| name == "setupScenePath" || name == "resolutionWidth" || name == "resolutionHeight"))
                                    GUI.enabled = true;
                                else
                                    GUI.enabled = false;
                            }, () => {
                                GUI.enabled = true;
                            });

            }

            EditorGUILayout.Space(20);

            _fieldDrawer.Draw();

        }

        private void OnNewProjectGUI()
        {
            _newSettings.name = EditorGUILayout.TextField("名称", _newSettings.name);

            _newSettings.globalNameSpace = EditorGUILayout.TextField("全局命名空间", _newSettings.globalNameSpace);
            _newSettings.projectNameSpace = EditorGUILayout.TextField("项目命名空间", _newSettings.projectNameSpace);
            _newSettings.codeBaseDir = EditorGUILayout.TextField("脚本存放目录", _newSettings.codeBaseDir);
            _newSettings.resBaseDir = EditorGUILayout.TextField("资源存放目录", _newSettings.resBaseDir);
            _newSettings.resolutionWidth = int.Parse(EditorGUILayout.TextField("分辨率宽", _newSettings.resolutionWidth+""));
            _newSettings.resolutionHeight = int.Parse(EditorGUILayout.TextField("分辨率高", _newSettings.resolutionHeight+""));
            _newSettings.setupScenePath = EditorGUILayout.TextField("启动场景", _newSettings.setupScenePath);

            if (GUILayout.Button("确定"))
            {
                if (!CheckNewProjectSettings())
                {
                    return;
                }

                foreach (var s in _projs.settings)
                {
                    if (s.name.ToLower() == _newSettings.name.ToLower())
                    {
                        EditorUtility.DisplayDialog("提示", "已经存在同名的项目", "知道了");
                        break;
                    }
                }

                if (CreateNewProject())
                {

                    //更新代码自动生成器
                    SyncProjectSettingsToCodeGenerator(_newSettings);

                    _isCreateMode = false;

                    //第一个项目，创建对应的目录
                    if (_projs.settings.Count == 1)
                    {
                        ActiveProject(_projs.settings[0].name);
                    }
                }
            }

            if (GUILayout.Button("取消"))
            {
                _isCreateMode = false;
            }
        }

        private void ActiveProject(string projName)
        {
            var oldActiveProjName = _projs.currentProjectName;

            //设置当前激活的项目
            _currentProjectName.stringValue = projName;
            serializedObject.ApplyModifiedProperties();

            //更新代码自动生成器
            SyncProjectSettingsToCodeGenerator(_projs.currentProjectSettings);



            bool isOk = true;

            //修改预制体
            var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/" + _projs.gameManagerPrefabRef.path);
            if(go != null)
            {
                //绑定UI窗口配置数据
                var frameworkComp = go.GetComponent<UIFrameworkSettings>();
                if (frameworkComp == null)
                    frameworkComp = go.AddComponent<UIFrameworkSettings>();
                frameworkComp.SettingData = CodeGeneratorSetting.Instance.uiFrameworkSettingData;

                //增加状态关联脚本
                var typeName = $"{CodeGeneratorSetting.Instance.projectFullNameSpace}.GameStateAssociateWindowsSetup";
                if(AddComponentToGameManagerPrefab<IStateAssociateWindowsSetup>(typeName, go) == null)
                    isOk = false;

                //绑定游戏扩展脚本
                typeName = $"{CodeGeneratorSetting.Instance.projectFullNameSpace}.GameExtensions"; 
                if (AddComponentToGameManagerPrefab<IGameExtensions>(typeName, go) == null)
                    isOk = false;

                //绑定游戏初始化配置
                var gameCom = go.GetComponent<CGame>();
                gameCom.GameInitConfig = _projs.currentProjectSettings.gameInitConfig;

                //其它的一些额外处理

                //1.切换游戏资源路径
                GameConfig gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>(gameConfigPath);
                gameConfig.gameResDir = CodeGeneratorSetting.Instance.resBaseDir;
                gameConfig.strNamespace = CodeGeneratorSetting.Instance.projectNameSpace;
                gameConfig.gameScriptDir = CodeGeneratorSetting.Instance.codeFileBaseDir;
                EditorUtility.SetDirty(gameConfig);

                //2.切换运行时的分辨率
                __SetupResotion(go, CodeGeneratorSetting.Instance.resolutionWidth, CodeGeneratorSetting.Instance.resolutionHeight);


                //3.切换UI编辑canvas的分辨率
                GameObject canvasObject = AssetDatabase.LoadAssetAtPath<GameObject>(canvasEditorPath);
                __SetupResotion(canvasObject, CodeGeneratorSetting.Instance.resolutionWidth, CodeGeneratorSetting.Instance.resolutionHeight);

                //4.设置启动场景
                __SetupStartupScene();

                //同步场景路径
                gameCom.GameInitConfig.gameScenePath = CodeGeneratorSetting.Instance.setupScenePath;
                gameCom.GameInitConfig.gameStateBuildinSceneIndex = -1;



                EditorUtility.SetDirty(go);
                EditorUtility.SetDirty(gameCom.GameInitConfig);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if(!isOk)
            {
                ActiveProject(oldActiveProjName);
                EditorUtility.DisplayDialog("提示", "激活失败，请重新尝试！", "好的");   
            }
            else
            {
                sucWaitCommfirm = true;
                Debug.Log("激活成功！");
            }
        }



        private void __SetupResotion(GameObject go, int w, int h)
        {

            if (null == go)
            {
                return;
            }

            Vector2 referenceResolution = new Vector2(w, h);
            CanvasScaler[] canvasScalers = go.GetComponentsInChildren<CanvasScaler>(true);
            int len = canvasScalers.Length;
            for (int i = 0; i < len; ++i)
            {
                canvasScalers[i].referenceResolution = referenceResolution;
            }

            EditorUtility.SetDirty(go);
        }

        private void __SetupStartupScene()
        {
            string path = CodeGeneratorSetting.Instance.setupScenePath;
            if(string.IsNullOrEmpty(path))
            {
                return;
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            int count = scenes.Length;
            for(int i=0;i<count;++i)
            {
                //已经添加了
                if (scenes[i].path == path)
                {
                    return;
                }
            }

            //新增场景
            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[count+1]; 
            Array.Copy(scenes, newScenes, scenes.Length);
            EditorBuildSettingsScene newScene = new EditorBuildSettingsScene();
            newScene.path = path;
            newScene.enabled = true;
            newScene.guid = new GUID(AssetDatabase.AssetPathToGUID(path));
            newScenes[count] = newScene;
            EditorBuildSettings.scenes = newScenes;


            /*
            if (scenes.Length >= 2)
            {
                scenes[1].enabled = true;
              
                scenes[1].path = path;
                scenes[1].guid = new GUID(AssetDatabase.AssetPathToGUID(path));
                EditorBuildSettings.scenes = scenes;
            }*/
        }

        private bool DeleteProject(string projName)
        {
            DirectoryInfo directory = new DirectoryInfo($"Assets/{_projs.workSpace}/{projName}");
            if (directory.Exists)
                directory.Delete(true);

            foreach (var s in _projs.settings)
            {
                if (s.name == projName)
                {
                    _projs.settings.Remove(s);
                    EditorUtility.SetDirty(_projs);
                    break;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        private MonoBehaviour AddComponentToGameManagerPrefab<T>(string typePath, GameObject go)
        {
            //先移除掉
            var comps = go.GetComponents<T>();
            if (comps != null)
            {
                foreach(var c in comps)
                {
                    DestroyImmediate(c as UnityEngine.Object, true);
                }
            }

            //再添加上
            Assembly[] assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            Type type = null;
            int assemblyArrayLength = assemblyArray.Length;
            for (int i = 0; i < assemblyArrayLength; ++i)
            {
                type = assemblyArray[i].GetType(typePath);
                if (type != null)
                {
                    break;
                }
            }

            if (type != null)
            {
                return go.AddComponent(type) as MonoBehaviour;
            }
            else
            {
                Debug.LogError($"添加脚本失败！type={typePath}");
            }

            return null;
        }

        private bool CheckNewProjectSettings()
        {
            if (string.IsNullOrEmpty(_newSettings.name)
                || string.IsNullOrEmpty(_newSettings.globalNameSpace)
                || string.IsNullOrEmpty(_newSettings.projectNameSpace)
                || string.IsNullOrEmpty(_newSettings.codeBaseDir)
                || string.IsNullOrEmpty(_newSettings.resBaseDir))
            {
                EditorUtility.DisplayDialog("提示", "设置错误，请检查！", "知道了");
                return false;
            }

            return true;
        }

        private bool CreateNewProject()
        {
            if (!CheckNewProjectSettings())
                return false;

            var s = new GameProjectSettings.Settings();

            s.name = _newSettings.name;
            s.globalNameSpace = _newSettings.globalNameSpace;
            s.projectNameSpace = _newSettings.projectNameSpace;
            s.codeBaseDir = _newSettings.codeBaseDir;
            s.resBaseDir = _newSettings.resBaseDir;

            //游戏初始化配置
            XGameEditorUtility.CreateDirectoryRecursive($"Assets/{_projs.workSpace}/{s.name}/{s.resBaseDir}/Configs");
            var gameInitConfig = ScriptableObject.CreateInstance<GameInitConfig>();
            AssetDatabase.CreateAsset(gameInitConfig, $"Assets/{_projs.workSpace}/{s.name}/{s.resBaseDir}/Configs/GameInitConfig.asset");
            s.gameInitConfig = gameInitConfig;



            _projs.settings.Add(s);

            EditorUtility.SetDirty(_projs);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        private void SyncProjectSettingsToCodeGenerator(GameProjectSettings.Settings s)
        {
            CodeGeneratorSetting.Instance.authorName = CodeGeneratorEvn.Instance.authorName;
            CodeGeneratorSetting.Instance.codeBaseDir = s.codeBaseDir;
            CodeGeneratorSetting.Instance.resourceBaseDir = s.resBaseDir;
            CodeGeneratorSetting.Instance.projectName = s.name;
            CodeGeneratorSetting.Instance.projectNameSpace = s.projectNameSpace;
            CodeGeneratorSetting.Instance.globalNameSpace = s.globalNameSpace;
            CodeGeneratorSetting.Instance.resolutionWidth = s.resolutionWidth;
            CodeGeneratorSetting.Instance.resolutionHeight = s.resolutionHeight;
            CodeGeneratorSetting.Instance.setupScenePath = s.setupScenePath;

            //生成环境
            CodeGeneratorEditorHelper.CreateCodeGenerateEnv();

            CodeGeneratorSetting.Instance.Dirty();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void ClearSettings()
        {
            _newSettings = new GameProjectSettings.Settings();
        }

    }

    public class GameProjectSettingsEditorUtility
    {
        private static GameProjectSettings _Settings;

        /// <summary>
        /// 获取配置文件
        /// </summary>
        /// <returns></returns>
        public static GameProjectSettings GetProjectSettings()
        {
            if(_Settings != null)
                return _Settings;

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(GameProjectSettings).Name}");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<GameProjectSettings>(path);
                if (asset != null)
                {
                    _Settings = asset;
                }
            }
            return _Settings;
        }

        [MenuItem("XGame/项目列表")]
        public static void NewProject()
        {
            GameProjectSettings projs = GetProjectSettings();

            if(projs == null)
            {
                projs =ScriptableObject.CreateInstance<GameProjectSettings>();
                AssetDatabase.CreateAsset(projs, "Assets/GameProjectSettins.asset");

                _Settings = projs;
            }

            XGameEditorUtility.PingObject(projs);
        }
    }
}
