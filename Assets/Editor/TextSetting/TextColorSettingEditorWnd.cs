/*****************************************************************
** 文件名:	TextColorSetting.cs
** 版  权:	(C) 冰川网络网络科技
** 创建人:  郑秀程
** 日  期:	2021/5
** 版  本:	1.0
** 描  述:	设置文本颜色
** 应  用:  	

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using XGame.TextSetting;
using XGameEditor.Config;

namespace XGameEditor.TextSetting
{
    public class TextColorSettingEditorWnd : EditorWindow
    {
        private GameObject m_goText;
        private Color m_color;
        private TextSettingConfig m_textSettingConfig;

        [MenuItem("XGame/文本/颜色设置", priority = 1001)]
        public static void ShowWindow()
        {
            TextColorSettingEditorWnd window = GetWindow<TextColorSettingEditorWnd>(false, "文本颜色设置");
            window.minSize = new Vector2(500, 400);
            window.maxSize = new Vector2(700, 1000);
            window.Show();
        }

        private void OnEnable()
        {
            m_textSettingConfig = SingletonScriptableObjectAsset<TextSettingConfig>.Instance(true, XGameEditorConfig.Instance.gameSettingsSaveDir);
        }

        public void OnGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("文本设置配置", m_textSettingConfig, typeof(TextSettingConfig), true);
            GUI.enabled = true;

            var goText = (GameObject)EditorGUILayout.ObjectField("选择文本", m_goText, typeof(GameObject), true);
            if(goText == null)
            {
                m_goText = null;
                return;
            }

            UnityEngine.Object text = goText.GetComponent<Text>();
            if(text == null)
            {
                text = goText.GetComponent<TextMeshProUGUI>();
            }

            if(text == null || !(text is Text || text is TextMeshProUGUI))
            {
                m_goText = null;
                return;
            }

            if (m_goText != goText && goText != null)
            {
                if(text is Text || text is TextMeshProUGUI)
                {
                    if (text is Text)
                        m_color = (text as Text).color;
                    else
                        m_color = (text as TextMeshProUGUI).color;
                }
            }

            m_goText = goText;

            if (m_textSettingConfig.textColorLibrary == null)
            {
                EditorGUILayout.HelpBox("请先配置好TextSettingConfig！", MessageType.Error);
                if (GUILayout.Button("去配置"))
                {
                    XGameEditorUtilityEx.PingObject(m_textSettingConfig);
                }
                return;
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField("颜色库", m_textSettingConfig.textColorLibrary, typeof(TextColorLibrary), true);
            GUI.enabled = true;

            EditorGUILayout.BeginVertical();

            TextColorLibrary colorLibary = m_textSettingConfig.textColorLibrary;
            if (colorLibary != null)
            {
                Color bak = GUI.skin.button.normal.textColor;
                int fontSizeBake = GUI.skin.button.fontSize;
                GUI.skin.button.fontSize = 25;

                if (GUILayout.Button("原色: 这是颜色演示文本", GUILayout.Height(40)))
                {
                    if(text is Text)
                        (text as Text).color = m_color;
                    else
                        (text as TextMeshProUGUI).color = m_color;

                    EditorUtility.SetDirty(m_goText);
                    AssetDatabase.SaveAssets();
                }

                for (var i = 0; i < colorLibary.assets.Count; ++i)
                {
                    TextColorItem colorItem = colorLibary.assets[i];
                    GUI.skin.button.normal.textColor = colorItem.color;
                    if(GUILayout.Button(colorItem.name+": 这是颜色演示文本", GUILayout.Height(40)))
                    {
                        if (text is Text)
                            (text as Text).color = colorItem.color;
                        else
                            (text as TextMeshProUGUI).color = colorItem.color;

                        EditorUtility.SetDirty(m_goText);
                        AssetDatabase.SaveAssets();
                    }
                }

                GUI.skin.button.normal.textColor = bak;
                GUI.skin.button.fontSize = fontSizeBake;
            }

            EditorGUILayout.EndVertical();

            GUI.changed = true;
        }
    }

}
