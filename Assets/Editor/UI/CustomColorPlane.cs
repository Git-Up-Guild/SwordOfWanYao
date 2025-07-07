/*****************************************************************
** 文件名:	CustomColorPlane.cs
** 版  权:	
** 创建人:  郑秀程
** 日  期:	2020/5
** 版  本:	1.0
** 描  述:	自定义颜色面板
** 应  用:  	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace XGameEditor.UI
{
    public class CustomColorPlane : EditorWindow
    {
        public List<Color> colorList = new List<Color>();
        public static void Open()
        {
            EditorWindow.GetWindow<CustomColorPlane>().Create();
        }

        public void Load()
        {
        }

        public void Save()
        {
        }

        void Create()
        {
            minSize = new Vector2(400, 400);
            maxSize = new Vector2(400, 400);
            colorList.Clear();
        }

        Vector2 pos;
        Color col = Color.red;
        float rowNum = 10;
        bool add = true;
        private void OnGUI()
        {
            int index = 0;
            pos = EditorGUILayout.BeginScrollView(pos, false, false, GUILayout.Height(250));
            add = true;

            float w = 300 / rowNum;
            while (index < colorList.Count)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; index < colorList.Count && i < rowNum; i++, index++)
                {
                    GUI.color = colorList[index];
                    if (colorList[i] == col)
                    {
                        add = false;
                    }
                    if (GUILayout.Button("", GUILayout.Width(w), GUILayout.Height(w)))
                    {
                        col = colorList[index];
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.color = Color.white;

            EditorGUILayout.EndScrollView();

            EditorGUILayoutUtility.DrawSplitLine(20, 20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当前选择颜色", GUILayout.Width(100));
            col = EditorGUILayout.ColorField(col);
            if (GUILayout.Button(add ? "+" : "-"))
            {
                if (add)
                {
                    colorList.Add(col);
                }
                else
                {
                    foreach (var v in colorList)
                    {
                        if (v == col)
                        {
                            colorList.Remove(v);
                            break;
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayoutUtility.DrawSplitLine(20, 20);

            if (GUILayout.Button("确定"))
            {
            }
        }
    }
}

