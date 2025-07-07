using UnityEngine;
using UnityEditor;
using System.IO;
using XGameEditor;
using System.Collections.Generic;
using XClient.Client;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace subjectnerdagreement.psdexport
{
    [CustomEditor(typeof(PSDSetting))]
    public class PSDSettingEditor : Editor
    {
        protected PSDSetting m_PsdSetting;
        private int testStyleIndex = 0;
        private GameObject testText;

        public void OnEnable()
        {
            m_PsdSetting = (PSDSetting)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            testText = EditorGUILayout.ObjectField("测试文本", testText, typeof(GameObject), true) as GameObject;
            if (testText != null)
            {
                string[] arrStyle = new string[m_PsdSetting.txtStyles.Count];
                for (var i = 0; i < arrStyle.Length; ++i)
                {
                    arrStyle[i] = m_PsdSetting.txtStyles[i].name;
                }

                int newIndex = EditorGUILayout.Popup("选择风格", testStyleIndex, arrStyle);
                bool bNewIndex = false;
                if (testStyleIndex != newIndex)
                {
                    testStyleIndex = newIndex;
                    bNewIndex = true;
                }
                ApplyTxtStyle(testText, bNewIndex);
            }

        }

        private void ApplyTxtStyle(GameObject text, bool bNewStyle)
        {
            ImportTextStyle style = m_PsdSetting.txtStyles[testStyleIndex];

            Shadow[] coms = text.GetComponents<Shadow>();
            Shadow shadow = null;
            Outline outline = null;
            foreach (var s in coms)
            {
                if (bNewStyle)
                {
                    Object.DestroyImmediate(s);
                }
                else
                {
                    if (s.GetType().FullName == typeof(Shadow).FullName)
                    {
                        shadow = s;
                    }
                    if (s.GetType().FullName == typeof(Outline).FullName)
                    {
                        outline = s as Outline;
                    }
                }
            }

            if (style.enableShadow)
            {
                if (shadow == null)
                {
                    shadow = text.AddComponent<Shadow>();
                }
                shadow.effectColor = style.shadowColor;
                shadow.effectDistance = new Vector2(style.shadowX, style.shadowY);
            }
            else
            {
                if (shadow != null)
                    Object.DestroyImmediate(shadow);
            }

            if (style.enableOut)
            {
                if (outline == null)
                {
                    outline = text.AddComponent<Outline>();
                }
                outline.effectColor = style.outColor;
                outline.effectDistance = new Vector2(style.outX, style.outY);
            }
            else
            {
                if (outline != null)
                    Object.DestroyImmediate(outline);
            }
        }
    }
}
