/*******************************************************************
** 文件名:	MiscToolsMenu.cs 
** 版  权:	(C) 深圳冰川网络技术有限公司 2008 - All Rights Reserved
** 创建人:	郑秀程
** 日  期:	2020-04-16
** 版  本:	1.0
** 描  述:	杂项工具
********************************************************************/

using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using XGame.AssetScript.UI;
using XGame.TextSetting;
using XGame.UI;
using XGame.UI.State;
using XGameEditor.CodeGenerator;
using XGameEditor.Config;
using XGameEditor.UIEditorEnv;
using XClient.Game;
using XGameEditor.FunctionOpen;
using XGameEditor.Reddot;
using XGame.UI.TglBtn;
using System.Linq;
using NPOI.SS.UserModel;
using System.Text;
using System.Text.RegularExpressions;
using System;

namespace XGameEditor
{
    class XGameMiscMenus : Editor 
    {

        private static bool SetBubbonSoundForComponentOfPrefab<T>(GameObject prefab) where T : Component
        {
            bool isDirty = false;
            var comps = prefab.GetComponentsInChildren<T>(true);
            foreach (var comp in comps)
            {
                var sound = comp.GetComponent<UISound>();
                if (sound != null)
                {
                    sound.playMode = XGame.UI.PlayMode.Click;
                    sound.stopMode = StopMode.Auto;

                    if (sound.soundID == 0)
                    {
                        sound.soundID = 2001;
                        isDirty = true;
                    }
                }
                else
                {
                    var pointerEffect = comp.GetComponent<PointerEventEffect>();
                    if (pointerEffect == null)
                    {
                        pointerEffect = comp.gameObject.AddComponent<PointerEventEffect>();
                        pointerEffect.enableSoundEffect = true;
                    }

                    pointerEffect.enableSoundEffect = true;

                    if (!pointerEffect.enableSoundEffect || pointerEffect.soundID == 0)
                    {
                        pointerEffect.soundID = 2001;
                        isDirty |= true;
                    }
                }
            }

            return isDirty;
        }

        private static bool SetButtonSoundForSinglePrefab(string path)
        {
            path = XGameEditorUtility.ConvertToBaseOnAssetsPath(path);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            bool isDirty = false;
            if (prefab != null)
            {
                isDirty =  SetBubbonSoundForComponentOfPrefab<Button>(prefab);
                if (SetBubbonSoundForComponentOfPrefab<ToggleButton>(prefab))
                    isDirty = true;

                if(isDirty)
                {
                    Debug.Log($"处理预制体：{path}");
                    EditorUtility.SetDirty(prefab);
                }
            }

            return isDirty;
        }

        private static bool SetButtonSoundDirectory(string dir)
        {
            bool isDirty = false;
            var absolutePaths = Directory.GetFiles(dir, "*.prefab", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < absolutePaths.Length; i++)
            {
                var path = absolutePaths[i];
                EditorUtility.DisplayProgressBar($"处理中{i + 1}/{absolutePaths.Length}", path, (float)i / absolutePaths.Length);

                if(SetButtonSoundForSinglePrefab(path))
                    isDirty = true;
            }

            return isDirty;
        }

        [MenuItem("XGame/UI工具/给所有按钮添加上声音组件", false, MenuItemSortOrder.UI)]
        public static void SetAllButtonSound()
        {
            bool isDirty = SetButtonSoundDirectory("Assets/Game/ImmortalFamily/GameResources/Prefabs/UI/Widgets");
            if(SetButtonSoundDirectory("Assets/Game/ImmortalFamily/GameResources/Prefabs/UI/"))
                isDirty = true;

            EditorUtility.ClearProgressBar();

            if(isDirty)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }


        [MenuItem("XGame/UI工具/删除所有未使用的字体材质", false, MenuItemSortOrder.UI)]
        public static void DeleteAllNotUseFontMaterials()
        {
            var lsNotUseMaterials = new List<Material>();
            int totalCount = FindAllNotUseFontMaterials(lsNotUseMaterials);

            foreach (var m in lsNotUseMaterials)
            {
                Debug.LogError($"删除未使用字体材质：{m.name}", m);
                var path = AssetDatabase.GetAssetPath(m);
                AssetDatabase.DeleteAsset(path);
            }

            Debug.LogError($"删除结束。总共：{totalCount}，删除：{lsNotUseMaterials.Count}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //删除风格配置脚本中的材质
            var textSettingConfig = SingletonScriptableObjectAsset<TextSettingConfig>.Instance(true, XGameEditorConfig.Instance.gameSettingsSaveDir);
            var styles = textSettingConfig.tmpTextStyles;
            bool isChanged = false;
            foreach (var style in styles)
            {
                for (var i = style.normalStyles.Count - 1; i >= 0; i--)
                {
                    var s = style.normalStyles[i];
                    if (s.fontMat == null)
                    {
                        style.normalStyles.RemoveAt(i);
                        Debug.LogError($"移除丢失的文字风格，name={s.name}");
                        isChanged = true;
                    }
                }

                for (var i = style.titleStyles.Count - 1; i >= 0; i--)
                {
                    var s = style.titleStyles[i];
                    if (s.fontMat == null)
                    {
                        style.titleStyles.RemoveAt(i);
                        Debug.LogError($"移除丢失的文字风格，name={s.name}");
                        isChanged = true;
                    }
                }

                for (var i = style.numberStyles.Count - 1; i >= 0; i--)
                {
                    var s = style.numberStyles[i];
                    if (s.fontMat == null)
                    {
                        style.numberStyles.RemoveAt(i);
                        Debug.LogError($"移除丢失的文字风格，name={s.name}");
                        isChanged = true;
                    }
                }

                for (var i = style.styles.Count - 1; i >= 0; i--)
                {
                    var s = style.styles[i];
                    if (s.fontMat == null)
                    {
                        style.styles.RemoveAt(i);
                        Debug.LogError($"移除丢失的文字风格，name={s.name}");
                        isChanged = true;
                    }
                }
            }

            if(isChanged)
            {
                textSettingConfig.Save();
                AssetDatabase.Refresh();
            }
        }
        
        [MenuItem("XGame/UI工具/查找所有未使用的字体材质", false, MenuItemSortOrder.UI)]
        public static void FindAllNotUseFontMaterials()
        {
            var lsNotUseMaterials = new List<Material>();
            int totalCount = FindAllNotUseFontMaterials(lsNotUseMaterials);

            foreach(var m in lsNotUseMaterials)
            {
                Debug.LogError($"找到未使用字体材质：{m.name}", m);
            }

            Debug.LogError($"查找结束。总共：{totalCount}，未使用：{lsNotUseMaterials.Count}");
        }

        public static int FindAllNotUseFontMaterials(List<Material> lsNotUseMaterials)
        {
            lsNotUseMaterials.Clear();

            //找到所有的字体材质
            var materalFolder = $"Assets/G_Artist/Fonts/SDF4";
            string[] arrMaterialGUID = AssetDatabase.FindAssets("t:material", new string[] { materalFolder });
            if (arrMaterialGUID.Length == 0)
                return 0;

            //白名单
            string[] blackNames = new string[] {
                //"TIT_s9","TIT_s10","TIT_s11","TIT_s12", "TIT_s13","TIT_s14", "TIT_s25","TIT_s26","TIT_s27","TIT_s28","TIT_s29",
                "COMMON_SDF Material", "NUMBER_SDF Material", "BATTLE_SDF Material", "sdf_zh_cn Material", "sdf_zh_tw Material",
            };

            Dictionary<Material, int> dicExistMaterials = new Dictionary<Material, int>();
            foreach (var guid in arrMaterialGUID)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);

                int count = 0;

                //白名单中的材质默认有1的使用计数
                foreach (var n in blackNames)
                {
                    if (m.name.LastIndexOf(n) != -1)
                    {
                        count = 1;
                        break;
                    }
                }

                dicExistMaterials.Add(m, count);
            }

            var prefabFolder = $"Assets/G_Resources/Game/Prefab";
            string[] arrGUID = AssetDatabase.FindAssets("t:prefab", new string[] { prefabFolder });
            int nCount = arrGUID.Length;

            for (var i = 0; i < arrGUID.Length; ++i)
            {
                string guid = arrGUID[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                EditorUtility.DisplayProgressBar("处理中", $"{prefab.name} {i}/{nCount}", (float)i / nCount);

                //检查文本组件
                var widgets = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var w in widgets)
                {
                    if (w.fontSharedMaterial == null)
                        continue;

                    //被使用就增加计数1
                    int count = 0;
                    var m = w.fontSharedMaterial;
                    if (dicExistMaterials.TryGetValue(m, out count))
                    {
                        dicExistMaterials[m] = count + 1;
                    }
                }

                //检查toggleBtn组件
                var matSwitchers = prefab.GetComponentsInChildren<UITMPTextMaterialSwitcher>(true);
                foreach (var s in matSwitchers)
                {
                    if (s == null)
                        continue;

                    foreach(var m in s.materials)
                    {
                        if (m != null)
                        {
                            int count = 0;
                            if (dicExistMaterials.TryGetValue(m, out count))
                            {
                                dicExistMaterials[m] = count + 1;
                            }
                        }
                    }
                }

                //文本风格切换组件
                var switchers = prefab.GetComponentsInChildren<TextStyleSwitcher>(true);
                foreach (var w in switchers)
                {
                    if (w.tmpMats != null && w.tmpMats.Length > 0)
                    {
                        foreach(var m in w.tmpMats)
                        {
                            int count = 0;
                            if (dicExistMaterials.TryGetValue(m, out count))
                            {
                                dicExistMaterials[m] = count + 1;
                            }
                        }
                    }
                }
            }

            //找到所有计数为0的材质（这些都是没有被使用的材质）
            foreach (var kv in dicExistMaterials)
            {
                if (kv.Value == 0)
                {
                    lsNotUseMaterials.Add(kv.Key);
                }
            }

            EditorUtility.ClearProgressBar();

            return dicExistMaterials.Count;
        }


        [MenuItem("XGame/常用/启动场景(GameEntry)", false, 0)]
        public static void OpenLoginScene()
        {
            EditorSceneManager.OpenScene(XGameEditorConfig.Instance.gameEntryScenePath);
        }

        [MenuItem("XGame/常用/游戏初始化配置(GameInitConfig)", true, 2)]
        public static bool ShowGameIniConfigAssetsValidate()
        {
            return !string.IsNullOrEmpty(CodeGeneratorSetting.Instance.projectName);
        }

        [MenuItem("XGame/常用/游戏初始化配置(GameInitConfig)", false, 2)]
        public static void ShowGameIniConfigAssets()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(GameProjectSettings).Name}");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<GameProjectSettings>(path);
                if (asset != null)
                {
                    XGameEditorUtility.PingObject(asset.currentProjectSettings.gameInitConfig);
                    break;
                }
            }
        }


        [MenuItem("XGame/常用/打开UGUI编辑环境(Overlay)", false,3)]
        public static void OpenUGUIEditorSceneOverlay()
        {
            UIEditorEnvSettings.OpenUGUIEditorSceneOverlay();
        }

        [MenuItem("XGame/常用/打开UGUI编辑环境(Camera)", false, 4)]
        public static void OpenUGUIEditorSceneCamera()
        {
            UIEditorEnvSettings.OpenUGUIEditorSceneCamera();
        }

#if UNITY_ANDROID
        [MenuItem("XGame/常用/资源加载模式/AssetBundle", false, 5)]
        public static void EnableAssetBundleResMode()
        {
            //添加宏
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var ss = symbols.Split(';').ToList();
            if (!ss.Contains("EDITOR_ASSET_BUNDLE_RES_MODE"))
            {
                ss.Add("EDITOR_ASSET_BUNDLE_RES_MODE");
                symbols = string.Join(";", ss);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
            }

            var msg = "AssetBundle模式还需要进行以下操作：\n1、使用打包工具生成AssetBundle资源，注意勾选【完整包选项】；\n2、使用【XGame/其它】菜单清理PersistentDataPath目录；\n3、渲染API需要选择【OpenGL ES3.2】。";
            EditorUtility.DisplayDialog("提示", msg, "知道了");
        }

        [MenuItem("XGame/常用/资源加载模式/AssetDatabase", false, 5)]
        public static void EnableAssetDatabaseResMode()
        {
            //添加宏
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var ss = symbols.Split(';').ToList();
            if (ss.Contains("EDITOR_ASSET_BUNDLE_RES_MODE"))
            {
                ss.Remove("EDITOR_ASSET_BUNDLE_RES_MODE");
                symbols = string.Join(";", ss);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
            }
        }
#endif

        [MenuItem("XGame/常用/客户端配置表生成", true, 10)]
        public static bool ValidGenerateGameScheme()
        {
            return !Application.isPlaying;
        }

        [MenuItem("XGame/常用/客户端配置表生成", false, 10)]
        public static void GenerateGameScheme()
        {
            XGameEditorUtility.ExecuteBat(XGameEditorConfig.Instance.gameSchemeBatToolPath);
        }

        [MenuItem("XGame/常用/编辑器配置", false, 101)]
        public static void ShowXGameEditorConfig()
        {
            XGameEditorUtility.PingObject(XGameEditorConfig.Instance);
        }


        [MenuItem("XGame/其它/TextMeshPro/查找没有被引用的材质", false, 101)]
        public static void CheckTextMeshProMaterials()
        {
            EditorUtility.DisplayProgressBar("查找所有的材质", "查找中...", 0f);

            List<string> lsMatFiles = new List<string>();
            Dictionary<string, Material> dicMaterials = new Dictionary<string, Material>();
            XGameEditorUtilityEx.GetFilesPath(Application.dataPath + "/G_Artist/Fonts/SDF4", true, lsMatFiles, new string[] { ".mat" });
            if (lsMatFiles.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            for (var i = 0; i < lsMatFiles.Count; ++i)
            {
                var path = lsMatFiles[i];
                path = XGameEditorUtilityEx.BaseOnAssetsPath(path);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                dicMaterials.Add(mat.name, mat);
                EditorUtility.DisplayProgressBar("查找所有材质列表", $"查找中{i}/{lsMatFiles.Count}...", (float)i / lsMatFiles.Count);
            }

            EditorUtility.DisplayProgressBar("查找所有的预制体", "查找中...", 0f);
            List<string> lsPrefabFiles = new List<string>();
            XGameEditorUtilityEx.GetFilesPath(Application.dataPath + "/G_Resources/Game/Prefab", true, lsPrefabFiles, new string[] { ".prefab" });
            if (lsPrefabFiles.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorUtility.DisplayProgressBar("查找TextMeshPro用到的材质列表", "查找中...", 0f);
            Dictionary<string, Material> dicUsedMaterials = new Dictionary<string, Material>();
            for (var i = 0; i < lsPrefabFiles.Count; ++i)
            {
                var path = lsPrefabFiles[i];
                path = XGameEditorUtilityEx.BaseOnAssetsPath(path);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    var texts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var t in texts)
                    {
                        var mat = t.fontMaterial;
                        if (!dicUsedMaterials.ContainsKey(mat.name))
                            dicUsedMaterials.Add(mat.name, mat);
                    }
                }
                EditorUtility.DisplayProgressBar("查找TextMeshPro用到的材质列表", $"查找中{i}/{lsPrefabFiles.Count}...", (float)i / lsPrefabFiles.Count);
            }

            EditorUtility.DisplayProgressBar("查找未使用材质", $"查找中...", 1f);
            int nCount = 0;
            foreach (var item in dicMaterials)
            {
                if (!dicUsedMaterials.ContainsKey(item.Key + " (Instance)"))
                {
                    nCount++;
                    Debug.Log($"未被使用材质{nCount}：{item.Key}", item.Value);
                }
            }

            EditorUtility.ClearProgressBar();
        }

        [MenuItem("XGame/其它/TextMeshPro/设置最小文字尺寸为20", false, 101)]
        public static void ModifyAllTextMeshProTextSize()
        {
            EditorUtility.DisplayProgressBar("查找所有的预制体", "查找中...", 0f);
            List<string> lsPrefabFiles = new List<string>();
            XGameEditorUtilityEx.GetFilesPath(Application.dataPath + "/G_Resources/Game/Prefab", true, lsPrefabFiles, new string[] { ".prefab" });
            if (lsPrefabFiles.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            EditorUtility.DisplayProgressBar("修改字体大小", "修改中...", 0f);
            for (var i = 0; i < lsPrefabFiles.Count; ++i)
            {
                var path = lsPrefabFiles[i];
                path = XGameEditorUtilityEx.BaseOnAssetsPath(path);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    var texts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                    bool isDirty = false;
                    foreach (var t in texts)
                    {
                        var nearestPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject);
                        if (nearestPrefab == null)
                        {
                            if (t.fontSize < 20)
                            {
                                t.fontSize = 20;
                                isDirty = true;
                            }
                        }
                    }

                    var txts = prefab.GetComponentsInChildren<Text>(true);
                    foreach (var t in txts)
                    {
                        var nearestPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject);
                        if (nearestPrefab == null)
                        {
                            if (t.fontSize < 20)
                            {
                                t.fontSize = 20;
                                isDirty = true;
                            }
                        }
                    }

                    if (isDirty)
                    {
                        Debug.Log($"预制体中的文字大小被修改, path={path}");
                        EditorUtility.SetDirty(prefab);
                    }

                }
                EditorUtility.DisplayProgressBar("修改字体大小", $"修改中{i}/{lsPrefabFiles.Count}...", (float)i / lsPrefabFiles.Count);
            }

            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("XGame/其它/禁用Graphic RaycastTarget属性", false, 201)]
        public static void DisableRaycastTarget()
        {
            var sel = Selection.activeGameObject;
            if (sel == null)
                return;

            var graphics = sel.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                g.raycastTarget = false;
            }

            EditorUtility.SetDirty(sel);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("XGame/其它/拷贝 PersistentDataPath 目录", false, 103)]
        public static void CopyPersistentDataPath()
        {
            Debug.Log(Application.persistentDataPath);
            GUIUtility.systemCopyBuffer = Application.persistentDataPath;
        }

        [MenuItem("XGame/其它/清除 PersistentDataPath 目录", false, 102)]
        public static void ClearPersistentDataPath()
        {
            Debug.Log(Application.persistentDataPath);
            XGameEditorUtilityEx.DeleteDir($"{Application.persistentDataPath}");
        }

        [MenuItem("XGame/其它/清除 PlayerPrefs 数据", false, 101)]
        public static void ClearPlayerPrefas()
        {
            PlayerPrefs.DeleteAll();
        }

        private static string FindQuotedText(string input)
        {
            Dictionary<char, bool> result = new Dictionary<char, bool>();

            // 查找双引号包围的内容
            var doubleQuoted = Regex.Matches(input, "\"(.*?)\"");
            foreach (Match match in doubleQuoted)
            {
                foreach (var c in match.Groups[1].Value)
                {
                    result[c] = true;
                }
            }

            // 查找单引号包围的内容
            var singleQuoted = Regex.Matches(input, "'(.*?)'");
            foreach (Match match in singleQuoted)
            {
                foreach (var c in match.Groups[1].Value)
                {
                    result[c] = true;
                }
            }

            var sb = new StringBuilder();
            foreach (var c in result.Keys)
                sb.Append(c);

            return sb.ToString();
        }

        private static string RemoveGarbledCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder();
            foreach (char c in input)
            {
                // 检查字符是否属于以下类别：
                // - 基本ASCII (0-127)
                // - 中文 (4E00-9FFF)
                // - 常用标点符号
                // - 不是控制字符
                // - 不是Unicode替换字符
                if ((c >= 0x20 && c <= 0x7E) ||
                    (c >= 0x4E00 && c <= 0x9FFF) ||
                    (c >= 0x3000 && c <= 0x303F) ||
                    (c >= 0xFF00 && c <= 0xFFEF))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        [MenuItem("XGame/常用/收集所有字符", false, 501)]
        public static void CollectGameSchmeTexts()
        {
            Dictionary<char, bool> chars = new Dictionary<char, bool>();

            string[] prefabDirs = new string[] {
                 $"Assets/Game/ImmortalFamily/GameResources/Prefabs",
            };

            string[] codeSourceDirs = new string[] { 
                Application.dataPath,
            };

            StringBuilder charDetail = new StringBuilder();

            try
            {
                //收集脚本中的字符
                EditorUtility.DisplayProgressBar("收集字符(1/3)", "收集预制体中的字符...", 0.1f);
                string[] prefabs = AssetDatabase.FindAssets("t:Prefab", prefabDirs);
                foreach (var pref in prefabs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(pref); // 将GUID转换为资产路径
                    GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    var lstComp = new List<TextMeshProUGUI>();
                    asset.GetComponentsInChildren<TextMeshProUGUI>(true, lstComp);

                    foreach (var comp in lstComp)
                    {
                        foreach (var c in comp.text)
                        {
                            chars[c] = true;
                        }
                    }
                }

                //收集脚本中的字符
                EditorUtility.DisplayProgressBar("收集字符(2/3)", "收集脚本中的字符...", 0.3f);
                var lstFiles = new List<string>();
                foreach (var p in codeSourceDirs)
                {
                    if (!Directory.Exists(p))
                    {
                        Debug.LogError($"错误的目录：{p}");
                        continue;
                    }

                    lstFiles.Clear();
                    XGameEditorUtility.GetFilesPath(p, true, lstFiles, new string[] { ".cs" });

                    foreach (var codeFile in lstFiles)
                    {
                        if (codeFile.Contains("Editor") || codeFile.Contains("gamescheme") || codeFile.Contains("minigamepol")
                            || codeFile.Contains("commprotocol") || codeFile.Contains("cgprotocol"))
                            continue;

                        var encoding = EncodingUtility.DetectFileEncoding(codeFile);
                        string content = string.Empty;
                        if (encoding != Encoding.UTF8)
                        {
                            var bytes = File.ReadAllBytes(codeFile);
                            bytes = Encoding.Convert(encoding, Encoding.UTF8, bytes);
                            content = Encoding.UTF8.GetString(bytes);
                        }
                        else
                        {
                            content = File.ReadAllText(codeFile, encoding);
                        }

                        content = FindQuotedText(content);

                        foreach (var c in content)
                        {
                            chars[c] = true;
                        }
                    }
                }

                //将收集到的字符写入文件
                var extraFilePath = "./../../../tools/I18NTool/collector_chars_extra.txt";
                StringBuilder sb = new StringBuilder();
                foreach (var c in chars.Keys)
                    sb.Append(c);

                var output = RemoveGarbledCharacters(sb.ToString());
                File.WriteAllText(extraFilePath, output);

                //收集配置文件中的字符
                EditorUtility.DisplayProgressBar("收集字符(3/3)", "收集配置文件中的字符...", 0.6f);
                XGameEditorUtility.ExecuteBat(XGameEditorConfig.Instance.collectGameSchemeTextBatPath);

                File.WriteAllText("chars_detail.txt", charDetail.ToString());
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }


        }

        [MenuItem("XGame/功能开放/功能配置", false, XGameEditor.MenuItemSortOrder.Game)]
        public static void ShowFunctionOpenConfigs()
        {
            XGameEditorUtility.PingObject(FunctionOpenConfigs.Instance);
        }

        [MenuItem("XGame/红点系统/刷新配置", false, XGameEditor.MenuItemSortOrder.Game)]
        public static void RefreshReddotConfigs()
        {
            ReddotConfigsEditor.RefreshConfigs(ReddotConfigs.Instance);
            XGameEditorUtility.PingObject(ReddotConfigs.Instance);
        }

        [MenuItem("XGame/红点系统/查看配置", false, XGameEditor.MenuItemSortOrder.Game)]
        public static void ShowReddotConfigs()
        {
            XGameEditorUtility.PingObject(ReddotConfigs.Instance);
        }
    }
}
