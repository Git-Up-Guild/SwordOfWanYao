
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using XGameEditor;

namespace ShaderControl
{
    public class SCWindow : EditorWindow
    {
        enum ScanMode
        { 
            Dir,
            Project
        }

        enum SORT_TYPE
        {
            VariantsCount = 0,
            EnabledKeywordsCount = 1,
            ShaderFileName = 2,
            Keyword = 3
        }

        class KeywordView
        {
            public string keyword;
            public List<SCShader> shaders;
            public bool foldout;
        }

        SORT_TYPE sortType = SORT_TYPE.VariantsCount;
        const string PRAGMA_COMMENT_MARK = "// Edited by Shader Control: ";
        const string PRAGMA_DISABLED_MARK = "// Disabled by Shader Control: ";
        const string PRAGMA_MULTICOMPILE = "#pragma multi_compile ";
        const string PRAGMA_FEATURE = "#pragma shader_feature ";
        const string BACKUP_SUFFIX = "_backup";
        const string PRAGMA_UNDERSCORE = "__ ";
        GUIStyle blackStyle, commentStyle, disabledStyle, foldoutBold, foldoutNormal, foldoutDim, foldoutRTF;

        //shader总数
        private int totalShaderCount;

        //扫描到的目标shader数量
        private int scanTargetShaderCount = 0;

        int minimumKeywordCount;
        int maxKeywordsCountFound = 0;

        //找到的符合条件的Shader列表
        List<SCShader> shaders;

        Vector2 scrollViewPos;
        int totalKeywords;
        long totalVariants;
        int totalUsedKeywords; 
        //int totalBuildVariants;
        bool firstTime;
        GUIContent matIcon, shaderIcon;

        Dictionary<string, List<SCShader>> uniqueKeywords;
        Dictionary<string, List<SCShader>> uniqueEnabledKeywords;
        List<KeywordView> keywordView;
        string keywordFilter;

        private bool scanAllShaders = false;
        private ScanMode scanMode = ScanMode.Project;
        private string scanDir = string.Empty;

        #region Unity events

        void OnEnable()
        {
            // Setup styles
            Color backColor = EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.7f, 0.7f, 0.7f);
            Texture2D _blackTexture;
            _blackTexture = MakeTex(4, 4, backColor);
            _blackTexture.hideFlags = HideFlags.DontSave;
            blackStyle = new GUIStyle();
            blackStyle.normal.background = _blackTexture;
            firstTime = !EditorPrefs.GetBool("SHADER_CONTROL_FIRST_TIME");
        }

        Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        public static void ShowWindow()
        {
            EditorWindow.GetWindow<SCWindow>(false, "Shader Control", true);
        }

        private void CreateStyles()
        {
            if (commentStyle == null)
            {
                commentStyle = new GUIStyle(EditorStyles.label);
            }
            commentStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.62f, 0.76f, 0.9f) : new Color(0.32f, 0.36f, 0.42f);
            if (disabledStyle == null)
            {
                disabledStyle = new GUIStyle(EditorStyles.label);
            }
            disabledStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.8f) : new Color(0.32f, 0.32f, 0.32f);
            if (foldoutRTF == null)
            {
                foldoutRTF = new GUIStyle(EditorStyles.foldout);
            }
            foldoutRTF.richText = true;
            if (foldoutBold == null)
            {
                foldoutBold = new GUIStyle(EditorStyles.foldout);
                foldoutBold.fontStyle = FontStyle.Bold;
            }
            if (foldoutNormal == null)
            {
                foldoutNormal = new GUIStyle(EditorStyles.foldout);
            }
            if (foldoutDim == null)
            {
                foldoutDim = new GUIStyle(EditorStyles.foldout);
                foldoutDim.fontStyle = FontStyle.Italic;
            }
            if (matIcon == null)
            {
                matIcon = EditorGUIUtility.IconContent("PreMatSphere");
                if (matIcon == null)
                    matIcon = new GUIContent();
            }
            if (shaderIcon == null)
            {
                shaderIcon = EditorGUIUtility.IconContent("Shader Icon");
                if (shaderIcon == null)
                    matIcon = new GUIContent();
            }
        }

        private void OnScanSettingGUI()
        {
            EditorGUILayout.BeginVertical(blackStyle);
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("扫描模式", GUILayout.Width(60));
            scanMode = (ScanMode)EditorGUILayout.EnumPopup(scanMode, GUILayout.Width(80));
            if(scanMode == ScanMode.Dir)
            {
                EditorGUILayout.LabelField("", GUILayout.Width(80));
                EditorGUILayout.LabelField("扫描路径", GUILayout.Width(60));
                scanDir = EditorGUILayout.TextField(scanDir, GUILayout.Width(250));
            }

            //EditorGUILayout.LabelField("", GUILayout.Width(80));
            //scanAllShaders = EditorGUILayout.Toggle(new GUIContent("Force Scan All Shaders", "Also includes shaders that are not located within a Resources folder"), scanAllShaders);
            EditorGUILayout.LabelField("", GUILayout.Width(80));
            //GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("开始扫描")))
            {
                ExecuteScan();
                GUIUtility.ExitGUI();
                return;
            }
            if (shaders != null && shaders.Count > 0 && GUILayout.Button(new GUIContent("Clean All Materials", "Removes all disabled keywords in all materials.  This option is provided to ensure no existing materials are referencing a disabled shader keyword.\n\nTo disable keywords, first expand any shader from the list and uncheck the unwanted keywords (press 'Save' to modify the shader file and to clean any existing material that uses that specific shader)."), GUILayout.Width(120)))
            {
                CleanAllMaterials();
                GUIUtility.ExitGUI();
                return;
            }
            if (GUILayout.Button("Help", GUILayout.Width(40)))
                ShowHelpWindow();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void OnStaticesGUI()
        {
            if (shaders != null)
            {
                if (totalKeywords == totalUsedKeywords || totalKeywords == 0)
                {
                    EditorGUILayout.HelpBox($"Total Shaders:{totalShaderCount.ToString()} Target Shaders:{scanTargetShaderCount}  Shaders Using Keywords: {shaders.Count}\nTotal Keywords:{totalKeywords}  Total Variants:{totalVariants}", MessageType.Info);
                }
                else
                {
                    int keywordsPerc = totalUsedKeywords * 100 / totalKeywords;
                    //int variantsPerc = totalBuildVariants * 100 / totalVariants;
                    //EditorGUILayout.HelpBox($"Total Shaders:{totalShaderCount} Target Shaders:{scanTargetShaderCount}  Shaders Using Keywords:{shaders.Count}\nUsed Keywords:{totalUsedKeywords} of {totalKeywords}({keywordsPerc}%)  Actual Variants: {totalBuildVariants} of {totalVariants} ({variantsPerc}%)", MessageType.Info);
                    EditorGUILayout.HelpBox($"Total Shaders:{totalShaderCount} Target Shaders:{scanTargetShaderCount}  Shaders Using Keywords:{shaders.Count}\nUsed Keywords:{totalUsedKeywords} of {totalKeywords}({keywordsPerc}%)  Variants: {totalVariants})", MessageType.Info);
                }
                EditorGUILayout.Separator();
                int shaderCount = shaders.Count;
                if (shaderCount > 1)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Sort By", GUILayout.Width(90));
                    SORT_TYPE prevSortType = sortType;
                    sortType = (SORT_TYPE)EditorGUILayout.EnumPopup(sortType);
                    if (sortType != prevSortType)
                    {
                        ExecuteScan();
                        EditorGUIUtility.ExitGUI();
                        return;
                    }

                    //保存结果
                    if(GUILayout.Button("保存", GUILayout.Width(60)))
                    {
                        SaveScanResult();
                    }

                    EditorGUILayout.EndHorizontal();

                    if (sortType != SORT_TYPE.Keyword)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Keywords >=", GUILayout.Width(90));
                        minimumKeywordCount = EditorGUILayout.IntSlider(minimumKeywordCount, 0, maxKeywordsCountFound);
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Keyword Filter", GUILayout.Width(90));
                    keywordFilter = EditorGUILayout.TextField(keywordFilter);
                    if (GUILayout.Button(new GUIContent("Clear", "Clear filter."), GUILayout.Width(60)))
                    {
                        keywordFilter = "";
                        GUIUtility.keyboardControl = 0;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Separator();
                }
            }
        }

        private void SaveScanResult()
        {
            StringBuilder sb = new StringBuilder();
            string filePath = XGameEditorUtilityEx.GetCSharpScriptDir<SCWindow>();
            string fileName = string.Empty;

            //关键字排序的，保存Keyworld和Shader列表
            if (sortType == SORT_TYPE.Keyword)
            {
                int kvCount = keywordView.Count;
                for (int s = 0; s < kvCount; s++)
                {
                    sb.AppendLine("Keyword#" + s + " " + keywordView[s].keyword + " found in " + keywordView[s].shaders.Count + " shader(s)");
                    int kvShadersCount = keywordView[s].shaders.Count;
                    for (int m = 0; m < kvShadersCount; m++)
                    {
                        sb.AppendLine("　　Shader#" + m + " " + keywordView[s].shaders[m].name);
                    }
                    sb.AppendLine();
                }
                fileName = "Keywords.txt";
            }
            else
            {
                for (int s = 0; s < shaders.Count; s++)
                {
                    SCShader shader = shaders[s];

                    //sb.AppendLine("Shader#" + s + " " + shader.name + " (" + shader.keywords.Count + " keywords, " + shader.keywordEnabledCount + " used, " + shader.actualBuildVariantCount + " variants, " + shader.variantCountFormApi + "  api variants)");
                    sb.AppendLine("Shader#" + s + " " + shader.name + " (" + shader.keywords.Count + " keywords, " + shader.keywordEnabledCount + " used, " + shader.totalVariantCount + " variants)");

                    //关键字
                    for (int k = 0; k < shader.keywords.Count; k++)
                    {
                        SCKeyword keyword = shader.keywords[k];
                        if (keyword.isUnderscoreKeyword)
                            continue;
                        sb.AppendLine("　　Keyword#" + k + " " + keyword.name);
                    }

                    //材质
                    for (int m = 0; m < shader.materials.Count; m++)
                    {
                        SCMaterial material = shader.materials[m];
                        sb.AppendLine("　　Material#" + m + " " + material.name + "(Use " + material.keywords.Count + " keywords.)");
                    }
                    sb.AppendLine();
                }

                fileName = "Shaders.txt";
            }

            if (scanMode == ScanMode.Dir)
            {
                fileName = scanDir.Replace("/", "_").Replace("\\", "_") + "_" + fileName;
            }

            filePath = filePath + "/" + fileName;

            File.WriteAllText(filePath, sb.ToString());

        }

        private void OnKeywordViewGUI()
        {
            if (keywordView != null)
            {
                int kvCount = keywordView.Count;
                for (int s = 0; s < kvCount; s++)
                {
                    if (!string.IsNullOrEmpty(keywordFilter) && keywordView[s].keyword.IndexOf(keywordFilter, StringComparison.CurrentCultureIgnoreCase) < 0)
                        continue;
                    EditorGUILayout.BeginHorizontal();
                    keywordView[s].foldout = EditorGUILayout.Foldout(keywordView[s].foldout, new GUIContent("#"+s+" <b>" + keywordView[s].keyword + "</b> found in " + keywordView[s].shaders.Count + " shader(s)"), foldoutRTF);
                    EditorGUILayout.EndHorizontal();
                    if (keywordView[s].foldout)
                    {
                        int kvShadersCount = keywordView[s].shaders.Count;
                        for (int m = 0; m < kvShadersCount; m++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("", GUILayout.Width(30));
                            EditorGUILayout.LabelField(shaderIcon, GUILayout.Width(18));

                            SCShader shader = keywordView[s].shaders[m];
                            GUI.enabled = shader.hasSource;

                            Shader theShader = AssetDatabase.LoadAssetAtPath<Shader>(shader.path);
                            EditorGUILayout.LabelField(theShader.name);

                            if (GUILayout.Button(new GUIContent("定位"), GUILayout.Width(60)))
                            {
                                Selection.activeObject = theShader;
                                EditorGUIUtility.PingObject(theShader);
                            }
                            GUI.enabled = true;
                            if (GUILayout.Button(new GUIContent("过滤"), GUILayout.Width(60)))
                            {
                                keywordFilter = keywordView[s].keyword;
                                sortType = SORT_TYPE.VariantsCount;
                                EditorGUIUtility.ExitGUI();
                                return;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        void OnGUI()
        {
            CreateStyles();
            OnScanSettingGUI();

            if (shaders != null)
            {
                OnStaticesGUI();

                scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos);

                //按照关键字排序
                if (sortType == SORT_TYPE.Keyword)
                {
                    OnKeywordViewGUI();
                }
                else
                {
                    for (int s = 0; s < shaders.Count; s++)
                    {
                        SCShader shader = shaders[s];
                        if (shader.keywordEnabledCount < minimumKeywordCount)
                            continue;
                        if (!string.IsNullOrEmpty(keywordFilter))
                        {
                            int kwCount = shader.keywords.Count;
                            bool found = false;
                            for (int w = 0; w < kwCount; w++)
                            {
                                if (shader.keywords[w].name.IndexOf(keywordFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                continue;
                        }

                        EditorGUILayout.BeginHorizontal();
                        if (shader.hasSource)
                        {
                            //shader.foldout = EditorGUILayout.Foldout(shader.foldout, new GUIContent(shader.name + " (" + shader.keywords.Count + " keywords, " + shader.keywordEnabledCount + " used, " + shader.actualBuildVariantCount + " variants, " + shader.variantCountFormApi + "  api variants)"), shader.editedByShaderControl ? foldoutBold : foldoutNormal);
                            shader.foldout = EditorGUILayout.Foldout(shader.foldout, new GUIContent(shader.name + " (" + shader.keywords.Count + " keywords, " + shader.keywordEnabledCount + " used, " + shader.totalVariantCount + " variants"), shader.editedByShaderControl ? foldoutBold : foldoutNormal);
                        }
                        else
                        {
                            shader.foldout = EditorGUILayout.Foldout(shader.foldout, new GUIContent(shader.name + " (" + shader.keywordEnabledCount + " keywords used by materials)"), foldoutDim);
                        }
                        EditorGUILayout.EndHorizontal();
                        if (shader.foldout)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("", GUILayout.Width(15));
                            if (shader.hasSource)
                            {
                                if (GUILayout.Button(new GUIContent("Locate", "Locate the shader in the project panel."), GUILayout.Width(60)))
                                {
                                    Shader theShader = AssetDatabase.LoadAssetAtPath<Shader>(shader.path);
                                    Selection.activeObject = theShader;
                                    EditorGUIUtility.PingObject(theShader);
                                }
                                if (GUILayout.Button(new GUIContent("Open", "Open the shader with the system default editor."), GUILayout.Width(60)))
                                {
                                    EditorUtility.OpenWithDefaultApp(shader.path);
                                }
                                if (!shader.pendingChanges)
                                    GUI.enabled = false;
                                if (GUILayout.Button(new GUIContent("Save", "Saves any keyword change to the shader file (disable/enable the keywords just clicking on the toggle next to the keywords shown below). A backup is created in the same folder."), GUILayout.Width(60)))
                                {
                                    UpdateShader(shader);
                                }
                                GUI.enabled = true;
                                if (!shader.hasBackup)
                                    GUI.enabled = false;
                                if (GUILayout.Button(new GUIContent("Restore", "Restores shader from the backup copy."), GUILayout.Width(60)))
                                {
                                    RestoreShader(shader);
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                                GUI.enabled = true;
                                if (shader.materials.Count > 0 && GUILayout.Button(new GUIContent(shader.showMaterials ? "Hide Materials" : "List Materials", "Show or hide the materials that use these keywords."), GUILayout.Width(95)))
                                {
                                    shader.showMaterials = !shader.showMaterials;
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField("(Shader source not available)");
                                if (shader.materials.Count > 0 && GUILayout.Button(new GUIContent(shader.showMaterials ? "Hide Materials" : "List Materials", "Show or hide the materials that use these keywords."), GUILayout.Width(110)))
                                {
                                    shader.showMaterials = !shader.showMaterials;
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                            }
                            EditorGUILayout.EndHorizontal();


                            for (int k = 0; k < shader.keywords.Count; k++)
                            {
                                SCKeyword keyword = shader.keywords[k];
                                if (keyword.isUnderscoreKeyword)
                                    continue;
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("", GUILayout.Width(15));
                                if (shader.hasSource)
                                {
                                    bool prevState = keyword.enabled;
                                    keyword.enabled = EditorGUILayout.Toggle(prevState, GUILayout.Width(18));
                                    if (prevState != keyword.enabled)
                                    {
                                        shader.pendingChanges = true;
                                        shader.UpdateVariantCount();
                                        UpdateProjectStats();
                                        GUIUtility.ExitGUI();
                                        return;
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.Toggle(true, GUILayout.Width(18));
                                }
                                if (!keyword.enabled)
                                {
                                    EditorGUILayout.LabelField(keyword.name, disabledStyle);
                                }
                                else
                                {
                                    EditorGUILayout.LabelField(keyword.name);
                                    if (!shader.hasSource && GUILayout.Button(new GUIContent("Prune Keyword", "Removes the keyword from all materials that reference it."), GUILayout.Width(110)))
                                    {
                                        if (EditorUtility.DisplayDialog("Prune Keyword", "This option will disable the keyword " + keyword.name + " in all materials that use " + shader.name + " shader.\nDo you want to continue?", "Ok", "Cancel"))
                                        {
                                            PruneMaterials(shader, keyword.name);
                                            UpdateProjectStats();
                                        }
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                                if (shader.showMaterials)
                                {
                                    // show materials using this shader
                                    for (int m = 0; m < shader.materials.Count; m++)
                                    {
                                        SCMaterial material = shader.materials[m];
                                        if (material.ContainsKeyword(keyword.name))
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("", GUILayout.Width(30));
                                            EditorGUILayout.LabelField(matIcon, GUILayout.Width(18));
                                            EditorGUILayout.LabelField(material.name);
                                            if (GUILayout.Button(new GUIContent("定位"), GUILayout.Width(60)))
                                            {
                                                Material theMaterial = AssetDatabase.LoadAssetAtPath<Material>(material.path);
                                                Selection.activeObject = theMaterial;
                                                EditorGUIUtility.PingObject(theMaterial);
                                            }
                                            EditorGUILayout.EndHorizontal();
                                        }
                                    }
                                }
                            }

                            if (shader.showMaterials)
                            {
                                // show materials using this shader that does not use any keywords
                                bool first = true;
                                for (int m = 0; m < shader.materials.Count; m++)
                                {
                                    SCMaterial material = shader.materials[m];
                                    if (material.keywords.Count == 0)
                                    {
                                        if (first)
                                        {
                                            first = false;
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.LabelField("", GUILayout.Width(15));
                                            EditorGUILayout.LabelField("Materials using this shader and no keywords:");
                                            EditorGUILayout.EndHorizontal();
                                        }
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField("", GUILayout.Width(15));
                                        EditorGUILayout.LabelField(matIcon, GUILayout.Width(18));
                                        EditorGUILayout.LabelField(material.name);
                                        if (GUILayout.Button(new GUIContent("Locate", "Locates the material in the project panel."), GUILayout.Width(60)))
                                        {
                                            Material theMaterial = AssetDatabase.LoadAssetAtPath<Material>(material.path);
                                            Selection.activeObject = theMaterial;
                                            EditorGUIUtility.PingObject(theMaterial);
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                }
                            }
                        }
                        EditorGUILayout.Separator();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }

        void OnInspectorUpdate()
        {
            if (firstTime)
                ShowHelpWindow();

        }

        void ShowHelpWindow()
        {
            if (firstTime)
            {
                EditorPrefs.SetBool("SHADER_CONTROL_FIRST_TIME", true);
                firstTime = false;
            }
            EditorUtility.DisplayDialog("About Shader Control", "Shader Control is an useful script that scans your project and lists shaders that could be adding unnecesary code to your build, increasing its size and compilation time.\n\nPress 'Scan Project' to show a list of shaders along the keywords they're using. Shaders that don't use keywords won't be listed.\n\nDisable any number of keywords and press 'Save' to modify the shader file.\n\nImportant: a disabled keyword will deactivate a shader feature. Make sure you know what you're doing! You may need to review the shader documentation, its source code for comments or contact the author concerning the keywords that can be safely disabled before applying any change.\n\nAlthough Shader Control will make a backup copy of the shader before applying any change to it, it's recommended to have a backup copy of your project before applying automatic changes to your shaders.\n\nPlease contact us on kronnect.com support forum for any question or suggestions. Thanks!", "Ok");
        }

        #endregion

        #region Shader handling

        /// <summary>
        /// 查找使用了关键字的Shaders
        /// </summary>
        private void FindUseKeywordShaders()
        {
            if (shaders == null)
                shaders = new List<SCShader>();
            else
                shaders.Clear();

            //找到所有的Shader文件
            string[] guids = AssetDatabase.FindAssets("t:Shader");
            totalShaderCount = guids.Length;
            for (int k = 0; k < totalShaderCount; k++)
            {
                string guid = guids[k];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path != null)
                {
                    string pathUpper = path.ToUpper();

                    if (!IsTargetShader(path))
                    {
                        continue;
                    }

                    scanTargetShaderCount++;
                    Debug.Log("找到目标Shader，路径：" + path + ", 计数：" + scanTargetShaderCount);

                    if (scanAllShaders/* || pathUpper.Contains("\\RESOURCES\\") || pathUpper.Contains("/RESOURCES/")*/)
                    {
                        SCShader shader = new SCShader();
                        shader.path = path;
                        shader.name = Path.GetFileNameWithoutExtension(path);
                        shader.GUID = guid;
                        ScanShader(shader);
                        if (shader.keywords.Count > 0)
                        {
                            shaders.Add(shader);
                        }
                    }
                }
            }
        }

        private bool IsTargetShader(string path)
        {
            //只处理指定目录下的Shader
            if (scanMode == ScanMode.Dir)
            {
                string scanDirLower = scanDir.ToLower();
                if (path.ToLower().StartsWith(scanDirLower))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取使用了Shader的材质
        /// </summary>
        private void LoadAndReferenceMaterials()
        {
            Dictionary<string, SCShader> shaderCache = new Dictionary<string, SCShader>(shaders.Count);
            shaders.ForEach(shader =>
            {
                shaderCache.Add(shader.GUID, shader);
            });

            string[] matGuids = AssetDatabase.FindAssets("t:Material");
            int totaMatCount = matGuids.Length;
            for (int k = 0; k < totaMatCount; k++)
            {
                string matGUID = matGuids[k];
                string matPath = AssetDatabase.GUIDToAssetPath(matGUID);
                Material mat = (Material)AssetDatabase.LoadAssetAtPath<Material>(matPath);

                //没有设置Shader的材质要过滤掉
                if (mat.shader == null)
                    continue;

                //获取Shader信息
                string path = AssetDatabase.GetAssetPath(mat.shader);
                string shaderGUID = AssetDatabase.AssetPathToGUID(path);

                SCMaterial scMat = new SCMaterial(mat.name, matPath, matGUID);
                scMat.SetKeywords(mat.shaderKeywords);

                SCShader shader;
                if (shaderCache.ContainsKey(shaderGUID))
                {
                    shader = shaderCache[shaderGUID];
                }
                else
                {
                    if (mat.shaderKeywords == null || mat.shaderKeywords.Length == 0)
                        continue;

                    if (!IsTargetShader(path))
                    {
                        continue;
                    }

                    Shader shad = AssetDatabase.LoadAssetAtPath<Shader>(path);
                   
                    //添加没有源码的Shader
                    shader = new SCShader();
                    shader.GUID = shaderGUID;
                    if (shad != null)
                    {
                        shader.name = Path.GetFileNameWithoutExtension(path);
                        shader.path = path;
                        ScanShader(shader);
                    }
                    else
                    {
                        shader.name = mat.shader.name;
                    }
                    shaders.Add(shader);
                    shaderCache.Add(shaderGUID, shader);
                    totalShaderCount++;
                }

                shader.materials.Add(scMat);
                shader.AddKeywordsByName(mat.shaderKeywords);
            }

            // refresh variant and keywords count due to potential additional added keywords from materials (rogue keywords) and shader features count
            maxKeywordsCountFound = 0;
            shaders.ForEach((SCShader shader) =>
            {
                if (shader.keywordEnabledCount > maxKeywordsCountFound)
                {
                    maxKeywordsCountFound = shader.keywordEnabledCount;
                }
                shader.UpdateVariantCount();
            });
        }

        void ExecuteScan()
        {
            try
            {
                FindUseKeywordShaders();
                LoadAndReferenceMaterials();

                switch (sortType)
                {
                    case SORT_TYPE.VariantsCount:
                        shaders.Sort((SCShader x, SCShader y) =>
                        {
                            //return y.actualBuildVariantCount.CompareTo(x.actualBuildVariantCount);
                            return y.totalVariantCount.CompareTo(x.totalVariantCount);
                        });
                        break;
                    case SORT_TYPE.EnabledKeywordsCount:
                        shaders.Sort((SCShader x, SCShader y) =>
                        {
                            return y.keywordEnabledCount.CompareTo(x.keywordEnabledCount);
                        });
                        break;
                    case SORT_TYPE.ShaderFileName:
                        shaders.Sort((SCShader x, SCShader y) =>
                        {
                            return x.name.CompareTo(y.name);
                        });
                        break;
                }
                UpdateProjectStats();
            }
            catch (Exception ex)
            {
                Debug.LogError("Unexpected exception caught while scanning project: " + ex.Message);
            }
        }

        //扫描单个Shader文件
        void ScanShader(SCShader shader)
        {
            // Inits shader
            shader.passes.Clear();
            shader.keywords.Clear();
            shader.hasBackup = File.Exists(shader.path + BACKUP_SUFFIX);
            shader.pendingChanges = false;
            shader.editedByShaderControl = false;

            // Reads shader
            string[] shaderLines = File.ReadAllLines(shader.path);
            string[] separator = new string[] { " " };
            SCShaderPass currentPass = new SCShaderPass();
            SCShaderPass basePass = null;
            int pragmaControl = 0;
            int pass = -1;
            bool blockComment = false;
            SCKeywordLine keywordLine = new SCKeywordLine();
            for (int k = 0; k < shaderLines.Length; k++)
            {
                string line = shaderLines[k].Trim();
                if (line.Length == 0)
                    continue;

                //过滤掉注释
                int lineCommentIndex = line.IndexOf("//");
                int blocCommentIndex = line.IndexOf("/*");
                int endCommentIndex = line.IndexOf("*/");

                if (blocCommentIndex > 0 && (lineCommentIndex > blocCommentIndex || lineCommentIndex < 0))
                {
                    blockComment = true;
                }

                if (endCommentIndex > blocCommentIndex && (lineCommentIndex > endCommentIndex || lineCommentIndex < 0))
                {
                    blockComment = false;
                }

                //注释跳过
                if (blockComment)
                    continue;

                //PASS
                string lineUPPER = line.ToUpper();
                if (lineUPPER.Equals("PASS") || lineUPPER.StartsWith("PASS "))
                {
                    if (pass >= 0)
                    {
                        currentPass.pass = pass;
                        if (basePass != null)
                            currentPass.Add(basePass.keywordLines);
                        shader.Add(currentPass);
                    }
                    else if (currentPass.keywordCount > 0)
                    {
                        basePass = currentPass;
                    }
                    currentPass = new SCShaderPass();
                    pass++;
                    continue;
                }

                int j = line.IndexOf(PRAGMA_COMMENT_MARK);
                if (j >= 0)
                {
                    pragmaControl = 1;
                }
                else
                {
                    j = line.IndexOf(PRAGMA_DISABLED_MARK);
                    if (j >= 0)
                        pragmaControl = 3;
                }

                //lineCommentIndex==0是自己注释的，pragmaControl==1或者pragmaControl==3是工具注释
                if (lineCommentIndex == 0 && pragmaControl != 1 && pragmaControl != 3)
                {
                    continue;
                }

                //是否为特性（只有用到的才会生成变体）
                bool isShaderFeature = false;
                j = line.IndexOf(PRAGMA_MULTICOMPILE);
                if (j < 0)
                {
                    j = line.IndexOf(PRAGMA_FEATURE);
                    isShaderFeature = (j >= 0);
                }

                if (j >= 0)
                {
                    if (pragmaControl != 2)
                    {
                        keywordLine = new SCKeywordLine();
                    }
                    keywordLine.isFeature = isShaderFeature;
                    // exclude potential comments inside the #pragma line
                    int lastStringPos = line.IndexOf("//", j + 22);
                    if (lastStringPos < 0)
                    {
                        lastStringPos = line.Length;
                    }

                    int length = lastStringPos - j - 22;

                    //从一行中拆分出关键字
                    string[] kk = line.Substring(j + 22, length).Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    //去掉关键字左右的空格
                    for (int i = 0; i < kk.Length; i++)
                    {
                        kk[i] = kk[i].Trim();
                        //Debug.Log("发现关键字！" + kk[i]);
                    }

                    // Act on keywords
                    switch (pragmaControl)
                    {
                        case 1: // Edited by Shader Control line
                            shader.editedByShaderControl = true;
                            // Add original keywords to current line
                            for (int s = 0; s < kk.Length; s++)
                            {
                                keywordLine.Add(shader.GetKeyword(kk[s]));
                            }
                            pragmaControl = 2;
                            break;
                        case 2:
                            // check enabled keywords
                            keywordLine.DisableKeywords();
                            for (int s = 0; s < kk.Length; s++)
                            {
                                SCKeyword keyword = keywordLine.GetKeyword(kk[s]);
                                if (keyword != null)
                                    keyword.enabled = true;
                            }
                            currentPass.Add(keywordLine);
                            pragmaControl = 0;
                            break;
                        case 3: // disabled by Shader Control line
                            shader.editedByShaderControl = true;
                            // Add original keywords to current line
                            for (int s = 0; s < kk.Length; s++)
                            {
                                SCKeyword keyword = shader.GetKeyword(kk[s]);
                                keyword.enabled = false;
                                keywordLine.Add(keyword);
                            }
                            currentPass.Add(keywordLine);
                            pragmaControl = 0;
                            break;
                        case 0:
                            // Add keywords to current line
                            for (int s = 0; s < kk.Length; s++)
                            {
                                keywordLine.Add(shader.GetKeyword(kk[s]));
                            }
                            currentPass.Add(keywordLine);
                            break;
                    }
                }
            }
            currentPass.pass = Mathf.Max(pass, 0);
            if (basePass != null)
                currentPass.Add(basePass.keywordLines);
            shader.Add(currentPass);

            shader.UpdateVariantCount();


        }

        void UpdateProjectStats()
        {
            totalKeywords = 0;
            totalUsedKeywords = 0;
            totalVariants = 0;
            //totalBuildVariants = 0;
            if (shaders == null)
                return;

            //初始化字典
            if (uniqueKeywords == null)
                uniqueKeywords = new Dictionary<string, List<SCShader>>();
            else
                uniqueKeywords.Clear();

            if (uniqueEnabledKeywords == null)
                uniqueEnabledKeywords = new Dictionary<string, List<SCShader>>();
            else
                uniqueEnabledKeywords.Clear();

            int shadersCount = shaders.Count;
            for (int k = 0; k < shadersCount; k++)
            {
                SCShader shader = shaders[k];
                int keywordsCount = shader.keywords.Count;
                for (int w = 0; w < keywordsCount; w++)
                {
                    SCKeyword keyword = shader.keywords[w];

                    List<SCShader> shadersWithThisKeyword;
                    if (!uniqueKeywords.TryGetValue(keyword.name, out shadersWithThisKeyword))
                    {
                        shadersWithThisKeyword = new List<SCShader>();
                        uniqueKeywords[keyword.name] = shadersWithThisKeyword;
                        totalKeywords++;
                    }
                    shadersWithThisKeyword.Add(shader);
                    if (keyword.enabled)
                    {
                        List<SCShader> shadersWithThisKeywordEnabled;
                        if (!uniqueEnabledKeywords.TryGetValue(keyword.name, out shadersWithThisKeywordEnabled))
                        {
                            shadersWithThisKeywordEnabled = new List<SCShader>();
                            uniqueEnabledKeywords[keyword.name] = shadersWithThisKeywordEnabled;
                            totalUsedKeywords++;
                        }
                        shadersWithThisKeywordEnabled.Add(shader);
                    }
                }

                totalVariants += shader.totalVariantCount;
                //totalVariants += shader.totalVariantCount;
                //totalBuildVariants += shader.actualBuildVariantCount;
            }

            if (keywordView == null)
            {
                keywordView = new List<KeywordView>();
            }
            else
            {
                keywordView.Clear();
            }

            foreach (KeyValuePair<string, List<SCShader>> kvp in uniqueEnabledKeywords)
            {
                KeywordView kv = new KeywordView { keyword = kvp.Key, shaders = kvp.Value };
                keywordView.Add(kv);
            }

            keywordView.Sort(delegate (KeywordView x, KeywordView y)
           {
               return y.shaders.Count.CompareTo(x.shaders.Count);
           });

        }

        void UpdateShader(SCShader shader)
        {
            try
            {
                // Create backup
                string backupPath = shader.path + BACKUP_SUFFIX;
                if (!File.Exists(backupPath))
                {
                    AssetDatabase.CopyAsset(shader.path, backupPath);
                    shader.hasBackup = true;
                }

                // Reads and updates shader from disk
                string[] shaderLines = File.ReadAllLines(shader.path);
                string[] separator = new string[] { " " };
                StringBuilder sb = new StringBuilder();
                int pragmaControl = 0;
                shader.editedByShaderControl = false;
                SCKeywordLine keywordLine = new SCKeywordLine();
                bool blockComment = false;
                for (int k = 0; k < shaderLines.Length; k++)
                {

                    int lineCommentIndex = shaderLines[k].IndexOf("//");
                    int blocCommentIndex = shaderLines[k].IndexOf("/*");
                    int endCommentIndex = shaderLines[k].IndexOf("*/");
                    if (blocCommentIndex > 0 && (lineCommentIndex > blocCommentIndex || lineCommentIndex < 0))
                    {
                        blockComment = true;
                    }
                    if (endCommentIndex > blocCommentIndex && (lineCommentIndex > endCommentIndex || lineCommentIndex < 0))
                    {
                        blockComment = false;
                    }

                    int j = -1;
                    bool isShaderFeature = false;
                    if (!blockComment)
                    {
                        j = shaderLines[k].IndexOf(PRAGMA_COMMENT_MARK);
                        if (j >= 0)
                        {
                            pragmaControl = 1;
                        }
                        j = shaderLines[k].IndexOf(PRAGMA_MULTICOMPILE);
                        if (j < 0)
                        {
                            j = shaderLines[k].IndexOf(PRAGMA_FEATURE);
                            isShaderFeature = (j >= 0);
                        }
                        if (pragmaControl != 1 && lineCommentIndex == 0 && shaderLines[k].IndexOf(PRAGMA_DISABLED_MARK) < 0)
                        {
                            // do not process a commented line
                            j = -1;
                        }
                    }
                    if (j >= 0)
                    {
                        if (pragmaControl != 2)
                        {
                            keywordLine.Clear();
                        }
                        keywordLine.isFeature = isShaderFeature;
                        // exclude potential comments inside the #pragma line
                        int lastStringPos = shaderLines[k].IndexOf("//", j + 22);
                        if (lastStringPos < 0)
                        {
                            lastStringPos = shaderLines[k].Length;
                        }
                        int length = lastStringPos - j - 22;
                        string[] kk = shaderLines[k].Substring(j + 22, length).Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        // Sanitize keywords
                        for (int i = 0; i < kk.Length; i++)
                            kk[i] = kk[i].Trim();
                        // Act on keywords
                        switch (pragmaControl)
                        {
                            case 1:
                                // Read original keywords
                                for (int s = 0; s < kk.Length; s++)
                                {
                                    SCKeyword keyword = shader.GetKeyword(kk[s]);
                                    keywordLine.Add(keyword);
                                }
                                pragmaControl = 2;
                                break;
                            case 0:
                            case 2:
                                if (pragmaControl == 0)
                                {
                                    for (int s = 0; s < kk.Length; s++)
                                    {
                                        SCKeyword keyword = shader.GetKeyword(kk[s]);
                                        keywordLine.Add(keyword);
                                    }
                                }
                                int kCount = keywordLine.keywordCount;
                                int kEnabledCount = keywordLine.keywordsEnabledCount;
                                if (kEnabledCount < kCount)
                                {
                                    // write original keywords
                                    if (kEnabledCount == 0)
                                    {
                                        sb.Append(PRAGMA_DISABLED_MARK);
                                    }
                                    else
                                    {
                                        sb.Append(PRAGMA_COMMENT_MARK);
                                    }
                                    shader.editedByShaderControl = true;
                                    sb.Append(keywordLine.isFeature ? PRAGMA_FEATURE : PRAGMA_MULTICOMPILE);
                                    if (keywordLine.hasUnderscoreVariant)
                                        sb.Append(PRAGMA_UNDERSCORE);
                                    for (int s = 0; s < kCount; s++)
                                    {
                                        SCKeyword keyword = keywordLine.keywords[s];
                                        sb.Append(keyword.name);
                                        if (s < kCount - 1)
                                            sb.Append(" ");
                                    }
                                    sb.AppendLine();
                                }

                                if (kEnabledCount > 0)
                                {
                                    // Write actual keywords
                                    sb.Append(keywordLine.isFeature ? PRAGMA_FEATURE : PRAGMA_MULTICOMPILE);
                                    if (keywordLine.hasUnderscoreVariant)
                                        sb.Append(PRAGMA_UNDERSCORE);
                                    for (int s = 0; s < kCount; s++)
                                    {
                                        SCKeyword keyword = keywordLine.keywords[s];
                                        if (keyword.enabled)
                                        {
                                            sb.Append(keyword.name);
                                            if (s < kCount - 1)
                                                sb.Append(" ");
                                        }
                                    }
                                    sb.AppendLine();
                                }
                                pragmaControl = 0;
                                break;
                        }
                    }
                    else
                    {
                        sb.AppendLine(shaderLines[k]);
                    }
                }

                // Writes modified shader
                File.WriteAllText(shader.path, sb.ToString());
                AssetDatabase.Refresh();

                // Also update materials
                CleanMaterials(shader);

                ScanShader(shader); // Rescan shader
            }
            catch (Exception ex)
            {
                Debug.LogError("Unexpected exception caught while updating shader: " + ex.Message);
            }
        }

        void RestoreShader(SCShader shader)
        {
            try
            {
                string shaderBackupPath = shader.path + BACKUP_SUFFIX;
                if (!File.Exists(shaderBackupPath))
                {
                    EditorUtility.DisplayDialog("Restore shader", "Shader backup is missing!", "OK");
                    return;
                }
                File.Copy(shaderBackupPath, shader.path, true);
                File.Delete(shaderBackupPath);
                if (File.Exists(shaderBackupPath + ".meta"))
                    File.Delete(shaderBackupPath + ".meta");
                AssetDatabase.Refresh();

                ScanShader(shader); // Rescan shader
                UpdateProjectStats();
            }
            catch (Exception ex)
            {
                Debug.LogError("Unexpected exception caught while restoring shader: " + ex.Message);
            }
        }

        #endregion

        #region Material handling

        void CleanMaterials(SCShader shader)
        {
            // Updates any material using this shader
            Shader shad = (Shader)AssetDatabase.LoadAssetAtPath<Shader>(shader.path);
            if (shad != null)
            {
                bool requiresSave = false;
                string[] matGUIDs = AssetDatabase.FindAssets("t:Material");
                foreach (string matGUID in matGUIDs)
                {
                    string matPath = AssetDatabase.GUIDToAssetPath(matGUID);
                    Material mat = (Material)AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (mat != null && mat.shader.name.Equals(shad.name))
                    {
                        foreach (SCKeyword keyword in shader.keywords)
                        {
                            foreach (string matKeyword in mat.shaderKeywords)
                            {
                                if (matKeyword.Equals(keyword.name))
                                {
                                    if (!keyword.enabled && mat.IsKeywordEnabled(keyword.name))
                                    {
                                        mat.DisableKeyword(keyword.name);
                                        EditorUtility.SetDirty(mat);
                                        requiresSave = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                if (requiresSave)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        void CleanAllMaterials()
        {
            if (!EditorUtility.DisplayDialog("Clean All Materials", "This option will scan all materials and will prune any disabled keywords. This option is provided to ensure no materials are referencing a disabled shader keyword.\n\nRemember: to disable keywords, first expand any shader from the list and uncheck the unwanted keywords (press 'Save' to modify the shader file and to clean any existing material that uses that specific shader).\n\nDo you want to continue?", "Yes", "Cancel"))
            {
                return;
            }
            try
            {
                for (int k = 0; k < shaders.Count; k++)
                {
                    CleanMaterials(shaders[k]);
                }
                ExecuteScan();
                Debug.Log("Cleaning finished.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Unexpected exception caught while cleaning materials: " + ex.Message);
            }
        }

        void PruneMaterials(SCShader shader, string keywordName)
        {
            try
            {
                bool requiresSave = false;
                int materialCount = shader.materials.Count;
                for (int k = 0; k < materialCount; k++)
                {
                    SCMaterial material = shader.materials[k];
                    if (material.ContainsKeyword(keywordName))
                    {
                        Material theMaterial = (Material)AssetDatabase.LoadAssetAtPath<Material>(shader.materials[k].path);
                        if (theMaterial == null)
                            continue;
                        theMaterial.DisableKeyword(keywordName);
                        EditorUtility.SetDirty(theMaterial);
                        material.RemoveKeyword(keywordName);
                        shader.RemoveKeyword(keywordName);
                        requiresSave = true;
                    }
                }
                if (requiresSave)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Unexpected exception caught while pruning materials: " + ex.Message);
            }

        }

        #endregion
    }

}