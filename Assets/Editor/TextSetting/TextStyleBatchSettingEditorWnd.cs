/*****************************************************************
** 文件名:	TextStyleBatchSettingEditorWnd.cs
** 版  权:	(C) 冰川网络网络科技
** 创建人:  郑秀程
** 日  期:	2021/5
** 版  本:	1.0
** 描  述:	批量设置文本风格
** 应  用:  	

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using XGame.TextSetting;
using XGame.UI;
using XGame.UI.Framework.EffList;
using XGameEditor.Config;
using static PSDTMPTextStyleSettings;

namespace XGameEditor.TextSetting
{
    public class TextStyleBatchSettingEditorWnd : EditorWindow
    {
        /// <summary>
        /// 分类标题
        /// </summary>
        private static readonly string[] s_CcatgoryNames = { "普通", "标题", "数字" };

        [MenuItem("XGame/文本/批量设置文本风格", priority = 1003)]
        public static void ShowWindow()
        {
            ShowWindow(null);
        }

        public static void ShowWindow(Transform root = null)
        {
            TextStyleBatchSettingEditorWnd window = GetWindow<TextStyleBatchSettingEditorWnd>(false, "批量设置文本风格设置");
            window.minSize = new Vector2(1000, 400);
            window.maxSize = new Vector2(1400, 800);
            window.Show();
            window.m_root = root;
            if (root != null)
                window.m_texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        }

        //根对象
        private Transform m_root;

        //文本列表
        private TextMeshProUGUI[] m_texts;

        //风格文件配置
        private TextSettingConfig m_textSettingConfig;

        private Material m_material;
        private TMPro.TMP_ColorGradient m_colorGradientPreset;
        private VertexGradient m_vertexGradient;
        private StyleSetting m_curStyleSetting;

        private Vector2 m_scrollPosition = Vector2.zero;

        private Dictionary<PSDTMPTextStyleSettings, List<StyleSetting>> m_dicFontStyleSettings = new Dictionary<PSDTMPTextStyleSettings, List<StyleSetting>>();
        private Dictionary<PSDTMPTextStyleSettings, string[]> m_dicFontStyleNames = new Dictionary<PSDTMPTextStyleSettings, string[]>();

        private void OnEnable()
        {
            m_textSettingConfig = SingletonScriptableObjectAsset<TextSettingConfig>.Instance(true, XGameEditorConfig.Instance.gameSettingsSaveDir);
            m_dicFontStyleSettings.Clear();
            m_dicFontStyleNames.Clear();

            if(m_root != null)
                m_texts = m_root.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var settings in m_textSettingConfig.tmpTextStyles)
            {
                List<StyleSetting> lsStyleSettings = new List<StyleSetting>();
                List<string> lsStyleNames = new List<string>();

                lsStyleSettings.AddRange(settings.normalStyles);
                for (var i = 0; i < settings.normalStyles.Count; ++i)
                    lsStyleNames.Add(GetStyleName("普通", settings.normalStyles[i]));

                lsStyleSettings.AddRange(settings.titleStyles);
                for (var i = 0; i < settings.titleStyles.Count; ++i)
                    lsStyleNames.Add(GetStyleName("标题", settings.titleStyles[i]));

                lsStyleSettings.AddRange(settings.numberStyles);
                for (var i = 0; i < settings.numberStyles.Count; ++i)
                    lsStyleNames.Add(GetStyleName("数字", settings.numberStyles[i]));

                lsStyleSettings.AddRange(settings.styles);
                for (var i = 0; i < settings.styles.Count; ++i)
                    lsStyleNames.Add(GetStyleName("其它", settings.styles[i]));

                m_dicFontStyleSettings.Add(settings, lsStyleSettings);
                m_dicFontStyleNames.Add(settings, lsStyleNames.ToArray());
            }
        }

        private void OnDisable()
        {
            m_dicFontStyleSettings.Clear();
            m_dicFontStyleNames.Clear();
            m_root = null;
        }

        public void OnGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("文本设置配置", m_textSettingConfig, typeof(TextSettingConfig), true);
            GUI.enabled = true;

            Transform newRoot = (Transform)EditorGUILayout.ObjectField("选择要设置的根对象", m_root, typeof(Transform), true);
            if(newRoot != m_root)
            {
                m_root = newRoot;
                if(m_root != null)
                {
                    m_texts = newRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                }
                else
                {
                    m_texts = null;
                }
            }


            if(m_root == null)
            {
                EditorGUILayout.HelpBox("请先选中要进行设置的根对象", MessageType.Error);
                return;
            }

            if(m_texts == null)
            {
                EditorGUILayout.HelpBox("没有找到任何TextMeshProUGUI文本", MessageType.Warning);
                return;
            }

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
            DrawTextStylesInfos();
            EditorGUILayout.EndScrollView();
        }

        //解析风格
        private void ParseTextStyle(StyleSetting style, out bool hasOutline, out bool hasUnderlay, out bool hasColorGradient)
        {
            hasOutline = false;
            hasUnderlay = false;
            hasColorGradient = false;

            if (style.colorGrandient != null)
                hasColorGradient = true;

            if (style.fontMat == null)
                return;

            if (style.fontMat.IsKeywordEnabled("UNDERLAY_ON"))
                hasUnderlay = true;

            float outlineWidth = style.fontMat.GetFloat("_OutlineWidth");
            if(outlineWidth > 0.0001f)
                hasOutline = true;

        }

        private void DrawTextStylesInfos()
        {
            EditorGUILayout.BeginVertical();
            foreach (var text in m_texts)
            {
                EffectiveListItemMeta item = text.gameObject.GetComponentInParent<EffectiveListItemMeta>();
                if (item != null && item.gameObject.hideFlags.HasFlag(HideFlags.DontSave))
                    continue;

                EditorGUILayout.BeginHorizontal();
                string goFullPath = XGame.Utils.Utility.GetGameObjectFullPath(text.gameObject, 4);

                //int pos = goFullPath.LastIndexOf(text.name);
                //if (pos != -1)
                //    goFullPath = goFullPath.Substring(0, pos);

                //路径
                EditorGUILayout.LabelField(goFullPath);

                //文字
                if(GUILayout.Button(text.text, GUILayout.Width(250)))
                {
                    XGameEditorUtilityEx.PingObject(text);
                }

                //对象名称
                //EditorGUILayout.ObjectField(text, typeof(TextMeshProUGUI), true, GUILayout.Width(200));

                PSDTMPTextStyleSettings styleSettings;
                StyleSetting style;
                GetSingleTextStyle(text, out styleSettings, out style);
                var newStyleSettings = DrawTextStyleSettingsPopup(styleSettings);
                if(newStyleSettings != styleSettings)
                {
                    text.font = newStyleSettings.sdfFont;
                    EditorUtility.SetDirty(text);
                    AssetDatabase.SaveAssets();
                }

                var newStyle = DrawTextStylePopup(styleSettings, style);
                if(newStyle != style)
                {
                    text.fontMaterial = newStyle.fontMat;
                    if (newStyle.colorGrandient != null)
                    {
                        text.enableVertexGradient = true;
                        text.colorGradientPreset = newStyle.colorGrandient;
                    }
                    else
                    {
                        text.enableVertexGradient = false;
                    }

                    EditorUtility.SetDirty(text);
                    AssetDatabase.SaveAssets();

                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private string GetStyleName(string catgoryName, StyleSetting style)
        {
            bool hasOutline, hasUnderlay, hasColorGradient;
            ParseTextStyle(style, out hasOutline, out hasUnderlay, out hasColorGradient);

            string info = "";
            if (hasOutline)
                info += "*描边";
            if (hasUnderlay)
                info += "*投影";
            if (hasColorGradient)
                info += "*渐变";

            string name;
            if (string.IsNullOrEmpty(info))
                name = catgoryName + "/" + style.name + "_" + style.desc;
            else
                name = catgoryName + "/" + style.name + "_" + style.desc + " (" + info + ")";

            return name;
        }

        private StyleSetting DrawTextStylePopup(PSDTMPTextStyleSettings settings, StyleSetting style)
        {
            if(settings == null)
            {
                GUIStyle guiStyle = new GUIStyle("label");
                guiStyle.normal.textColor = Color.red;
                EditorGUILayout.LabelField("请先选字体", guiStyle, GUILayout.Width(250));
                return null;
            }

            List<StyleSetting> lsSettings;
            if(!m_dicFontStyleSettings.TryGetValue(settings, out lsSettings))
            {
                GUIStyle guiStyle = new GUIStyle("label");
                guiStyle.normal.textColor = Color.red;
                EditorGUILayout.LabelField("该字体没有可用的字体风格", guiStyle, GUILayout.Width(250));
                return null;
            }

            string[] names = m_dicFontStyleNames[settings];
            int selIndex = lsSettings.IndexOf(style);
            int newSelIndex = EditorGUILayout.Popup(selIndex, names, GUILayout.Width(250));
            if (newSelIndex != -1)
            {
                return lsSettings[newSelIndex];
            }
            return null;
        }

        private PSDTMPTextStyleSettings DrawTextStyleSettingsPopup(PSDTMPTextStyleSettings settings)
        {
            string[] names = new string[m_textSettingConfig.tmpTextStyles.Count];
            int selIndex = -1;
            for(var i = 0; i < m_textSettingConfig.tmpTextStyles.Count; ++i)
            {
                var s = m_textSettingConfig.tmpTextStyles[i];
                names[i] = s.fontName;
                if (s == settings)
                {
                    selIndex = i;
                }
            }

            int newSelIndex = EditorGUILayout.Popup(selIndex, names, GUILayout.Width(110));
            if (newSelIndex != -1)
            {
                return m_textSettingConfig.tmpTextStyles[newSelIndex];
            }
            return null;
        }

        private void GetSingleTextStyle(TextMeshProUGUI text, out PSDTMPTextStyleSettings styleSettings, out StyleSetting style)
        {
            styleSettings = null;
            style = null;
            foreach (var setting in m_textSettingConfig.tmpTextStyles)
            {
                if (setting.sdfFont == text.font)
                {
                    styleSettings = setting;
                    break;
                }
            }

            if (styleSettings == null)
                return;

            Dictionary<string, StyleSetting> allStyles = styleSettings.GetAllStyles();

            foreach (var s in allStyles.Values)
            {
                if (s.fontMat != null)
                {
                    if (text.fontSharedMaterial.name.Equals(s.fontMat.name) || text.fontSharedMaterial.name.Equals(s.fontMat.name + " (Instance)"))
                    {
                        style = s;
                        break;
                    }
                }
                else
                {
                    Debug.LogError("文本风格材质丢失，请检查，name=" + s.name + ", desc" + s.desc);
                }
            }
        }
    }
}