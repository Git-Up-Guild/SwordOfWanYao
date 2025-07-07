/*****************************************************************
** 文件名:	TextStyleSettingEditorWnd.cs
** 版  权:	(C) 冰川网络网络科技
** 创建人:  郑秀程
** 日  期:	2021/5
** 版  本:	1.0
** 描  述:	设置一文本风格
** 应  用:  	

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using TMPro;
using UnityEditor;
using UnityEngine;
using XGame.TextSetting;
using XGameEditor.Config;
using static PSDTMPTextStyleSettings;

namespace XGameEditor.TextSetting
{
    public class TextStyleSettingEditorWnd : EditorWindow
    {
        [MenuItem("XGame/文本/创建 TMPTextStyleSettings 文件", priority = 1003)]
        public static void CreateStyleSettingAsset()
        {
            string dirPath = GetCurrentAssetDirectory();
            string path = dirPath + "/PSDTMPTextStyleSettings.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            PSDTMPTextStyleSettings styleSettings = ScriptableObject.CreateInstance<PSDTMPTextStyleSettings>();
            AssetDatabase.CreateAsset(styleSettings, path);
            AssetDatabase.Refresh();
        }

        [MenuItem("XGame/文本/设置文本风格", priority = 1002)]
        public static void ShowWindow()
        {
            TextStyleSettingEditorWnd window = GetWindow<TextStyleSettingEditorWnd>(false, "设置文本风格");
            window.minSize = new Vector2(500, 400);
            window.maxSize = new Vector2(700, 1000);
            window.Show();
        }

        public static string GetCurrentAssetDirectory()
        {
            foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                if (System.IO.Directory.Exists(path))
                    return path;
                else if (System.IO.File.Exists(path))
                    return System.IO.Path.GetDirectoryName(path);
            }

            return "Assets";
        }

        private TextMeshProUGUI m_text;
        private Material m_material;
        private TMPro.TMP_ColorGradient m_colorGradientPreset;
        private VertexGradient m_vertexGradient;
        private StyleSetting m_curStyleSetting;
        private TextSettingConfig m_textSettingConfig;

        private void OnEnable()
        {
            m_textSettingConfig = SingletonScriptableObjectAsset<TextSettingConfig>.Instance(true, XGameEditorConfig.Instance.gameSettingsSaveDir);
        }

        public void OnGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("文本设置配置", m_textSettingConfig, typeof(TextSettingConfig), true);
            GUI.enabled = true;

            TextMeshProUGUI text = (TextMeshProUGUI)EditorGUILayout.ObjectField("选择文本", m_text, typeof(TextMeshProUGUI), true);
            if (m_text != text)
            {
                m_curStyleSetting = null;
                if (text != null)
                {
                    m_colorGradientPreset = text.colorGradientPreset;
                    m_material = text.fontMaterial;
                    m_vertexGradient = text.colorGradient;
                }
            }

            m_text = text;

            if (m_text == null)
                return;

            if (m_textSettingConfig.textColorLibrary == null)
            {
                EditorGUILayout.HelpBox("请先配置好TextSettingConfig！", MessageType.Error);
                if (GUILayout.Button("去配置"))
                {
                    XGameEditorUtility.PingObject(m_textSettingConfig);
                }
                return;
            }

            PSDTMPTextStyleSettings styleSettings = null;
            foreach(var style in m_textSettingConfig.tmpTextStyles)
            {
                if(style.sdfFont == m_text.font)
                {
                    styleSettings = style;
                    break;
                }
            }

            if(styleSettings == null)
            {
                EditorGUILayout.HelpBox("该文本的字体不支持风格设置，请检查！", MessageType.Error);
                return;
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField("匹配到风格配置文件", styleSettings, typeof(PSDTMPTextStyleSettings), true);
            if(m_curStyleSetting != null)
            {
                EditorGUILayout.ObjectField("当前使用到的材质", m_curStyleSetting.fontMat, typeof(Material), true);
                EditorGUILayout.ObjectField("当前使用到的渐变", m_curStyleSetting.colorGrandient, typeof(TMP_ColorGradient), true);
            }
            GUI.enabled = true;

            EditorGUILayout.BeginVertical();

            Color bak = GUI.skin.button.normal.textColor;
            int fontSizeBake = GUI.skin.button.fontSize;
            GUI.skin.button.fontSize = 18;

            EditorGUILayout.BeginHorizontal();
            int nHroCount = 1;
            float nW = position.width * 0.5f - 5;

            if (GUILayout.Button("原始风格", GUILayout.Height(30), GUILayout.Width(nW)))
            {
                text.colorGradientPreset = m_colorGradientPreset;
                text.fontMaterial = m_material;
                text.colorGradient = m_vertexGradient;
                m_curStyleSetting = null;

                EditorUtility.SetDirty(m_text);
                AssetDatabase.SaveAssets();
            }

            for (var i = 0; i < styleSettings.styles.Count; ++i)
            {
                if(nHroCount == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }

                var style = styleSettings.styles[i];
                string desc = style.desc;
                if (string.IsNullOrEmpty(desc))
                    desc = "没有指定风格描述";
                if (GUILayout.Button(style.name + ": " + desc, GUILayout.Height(30), GUILayout.Width(nW)))
                {
                    m_text.fontMaterial = style.fontMat;
                    if (style.colorGrandient != null)
                    {
                        m_text.enableVertexGradient = true;
                        m_text.colorGradientPreset = style.colorGrandient;
                    }

                    m_curStyleSetting = style;

                    EditorUtility.SetDirty(m_text);
                    AssetDatabase.SaveAssets();
                }

                nHroCount++;

                if(nHroCount >= 2)
                {
                    nHroCount = 0;
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (nHroCount > 0)
            {
                EditorGUILayout.EndHorizontal();
            }

            GUI.skin.button.normal.textColor = bak;
            GUI.skin.button.fontSize = fontSizeBake;

            EditorGUILayout.EndVertical();

            GUI.changed = true;
        }
    }
}