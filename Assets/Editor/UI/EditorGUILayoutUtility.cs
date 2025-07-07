/*****************************************************************
** 文件名:	EditorGUILayoutUtility.cs
** 版  权:	
** 创建人:  郑秀程
** 日  期:	2020/5
** 版  本:	1.0
** 描  述:	UI绘制工具类
** 应  用:  	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEngine;
using UnityEditor;

namespace XGameEditor.UI
{
    public enum DialogType
    {
        Del,
        ResetLoad,
    }

    public class EditorGUILayoutUtility
    {
        public static bool DrawPointInSceneView(ref Vector3 point,Quaternion rotation, string label = null)
        {
            if (!string.IsNullOrEmpty(label))
            {
                Handles.Label(point, label);
            }

            EditorGUI.BeginChangeCheck();

            point = Handles.DoPositionHandle(point, rotation);
            bool change = EditorGUI.EndChangeCheck();
            return change;
        }

        public static bool DrawPointInSceneView(ref Vector3 point, string label = null)
        {
            return DrawPointInSceneView(ref point, Quaternion.identity, label);
        }

        public static bool DisplayDialog(DialogType dialogType)
        {
            switch (dialogType)
            {
                case DialogType.Del:
                    return DisplayDialog("提示", "确定删除数据");
                case DialogType.ResetLoad:
                    return DisplayDialog("提示", "确定要重新加载数据");
            }
            return false;
        }

        public static bool DisplayDialog(string title, string label)
        {
            return EditorUtility.DisplayDialog(title, label, "确定", "取消");
        }

        public static void DrawCommonSaveAndLoad(System.Action saveAction, System.Action loadAction, int width = 200)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("保存", GUILayout.Width(width)))
            {
                if (saveAction != null)
                {
                    saveAction();
                }
            }

            if (GUILayout.Button("加载", GUILayout.Width(width)))
            {
                if (saveAction != null)
                {
                    if (DisplayDialog(DialogType.ResetLoad))
                    {
                        loadAction();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        static public void DrawSpace(int space, bool horizontal)
        {
            if (horizontal)
            {
                EditorGUILayout.LabelField("", GUILayout.Width(space));
            }
            else
            {
                EditorGUILayout.LabelField("", GUILayout.Width(10), GUILayout.Height(space));
            }
        }

        static public void DrawSplitLine(int topSpace = -1, int bottomSpace = -1,bool drawLine=true, int width = -1)
        {
            GUI.enabled = false;
            if (topSpace == 0)
            {
                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.Space(topSpace);
            }
            if (drawLine)
            {
                if (width > 0)
                {
                    GUILayout.Button("", GUILayout.Height(2f), GUILayout.Width(width));
                }
                else
                {
                    GUILayout.Button("", GUILayout.Height(2f));
                }
            }
           

            if (topSpace == 0)
            {
                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.Space(bottomSpace);
            }
            GUI.enabled = true;
        }

        /// <summary>
        /// 绘制float并保留指定位置(-1)代表完全保留  10*n ：代表保留N个小数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="clapNumber"></param>
        /// <returns></returns>
        static public float DrawFloatClamp(string label, float value, float labelWidth, float contentWidth, int clapNumber = -1)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));

            value = EditorGUILayout.FloatField(value, GUILayout.Width(contentWidth));

            if (clapNumber >= 0)
            {
                if (clapNumber > 0)
                {
                    int v = (int)(value * clapNumber);
                    value = v / clapNumber;
                }
                else
                {
                    value = (int)value;
                }


            }

            EditorGUILayout.EndHorizontal();
            return value;
        }

        static float ClapValue(float v, int clapVal)
        {
            if (clapVal <= 0) return v;
            v = (int)(v * clapVal);
            v = v / clapVal;
            return v;
        }
        static public Vector3 DrawVector3Filed(Vector3 value, int contentWidth, int clapVal = -1)
        {
            int labelWidth = 15;

            EditorGUILayout.BeginHorizontal(GUILayout.Width((labelWidth + contentWidth) * 3));

            EditorGUILayout.LabelField("X", GUILayout.Width(labelWidth));
            value.x = EditorGUILayout.FloatField(value.x, GUILayout.Width(contentWidth));

            EditorGUILayout.LabelField("Y", GUILayout.Width(labelWidth));
            value.y = EditorGUILayout.FloatField(value.y, GUILayout.Width(contentWidth));

            EditorGUILayout.LabelField("Z", GUILayout.Width(labelWidth));
            value.z = EditorGUILayout.FloatField(value.z, GUILayout.Width(contentWidth));

            if (clapVal > 0)
            {
                value.x = ClapValue(value.x, clapVal);
                value.y = ClapValue(value.y, clapVal);
                value.z = ClapValue(value.z, clapVal);
            }

            EditorGUILayout.EndHorizontal();


            return value;
        }

        static public Vector2 DrawVector2Filed(Vector2 value, int contentWidth, int clapVal = -1)
        {
            int labelWidth = 15;

            EditorGUILayout.BeginHorizontal(GUILayout.Width((labelWidth + contentWidth) * 3));

            EditorGUILayout.LabelField("X", GUILayout.Width(labelWidth));
            value.x = EditorGUILayout.FloatField(value.x, GUILayout.Width(contentWidth));

            EditorGUILayout.LabelField("Y", GUILayout.Width(labelWidth));
            value.y = EditorGUILayout.FloatField(value.y, GUILayout.Width(contentWidth));

            EditorGUILayout.LabelField("Z", GUILayout.Width(labelWidth));
           
            if (clapVal > 0)
            {
                value.x = ClapValue(value.x, clapVal);
                value.y = ClapValue(value.y, clapVal);
               
            }

            EditorGUILayout.EndHorizontal();

            return value;
        }


        static public int DrawIntFiled(int value, string label, bool borizontal = true, bool enable = true, int labelWidth = 100, int inputWidth = 200)
        {
            if (borizontal)
            {
                EditorGUILayout.BeginHorizontal();
            }

            GUI.enabled = enable;
            EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
            value = EditorGUILayout.IntField(value, GUILayout.Width(inputWidth));
            GUI.enabled = true;
            if (borizontal)
            {
                EditorGUILayout.EndHorizontal();
            }
            return value;
        }

        static public float DrawFloatFiled(float value, string label, bool borizontal = false, bool enable = true, int labelWidth = 100, int inputWidth = 200)
        {
            if (borizontal)
            {
                EditorGUILayout.BeginHorizontal();
            }

            GUI.enabled = enable;
            EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
            value = EditorGUILayout.FloatField(value, GUILayout.Width(inputWidth));
            GUI.enabled = true;
            if (borizontal)
            {
                EditorGUILayout.EndHorizontal();
            }
            return value;
        }

        static public string DrawStringFiled(string value, string label, bool borizontal = false, bool enable = true, int labelWidth = 100, int inputWidth = 200)
        {
            if (borizontal)
            {
                EditorGUILayout.BeginHorizontal();
            }

            GUI.enabled = enable;
            EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
            value = EditorGUILayout.TextField(value, GUILayout.Width(inputWidth));
            GUI.enabled = true;
            if (borizontal)
            {
                EditorGUILayout.EndHorizontal();
            }

            return value;
        }

        static public int SelectItemArr(string label, int value, string[] dataArr, bool borizontal = false, int labelWidth = 100)
        {
            if (borizontal)
            {
                EditorGUILayout.BeginHorizontal();
            }
            EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
            value = EditorGUILayout.Popup(value, dataArr);

            if (borizontal)
            {
                EditorGUILayout.EndHorizontal();
            }
            return value;
        }

        static public bool DrawFoldout(float offsetX, string label, bool canShowFoldout, ref bool openState, int labelWidth = -1)
        {
            return DrawFoldout(offsetX, label, Color.black, canShowFoldout, ref openState, labelWidth);
        }

        static public bool DrawFoldout(float offsetX, string label, Color labelColor, bool canShowFoldout, ref bool openState, int labelWidth = -1,int fontSize=-1)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = labelColor;
            style.normal.background = null;
            if (fontSize > 0)
            {
                style.fontSize = fontSize;
            } 
           return  DrawFoldout(offsetX, label, style,  canShowFoldout, ref openState); 
        }

        static public bool DrawFoldout(float offsetX, string label,GUIStyle gUIStyle, bool canShowFoldout, 
            ref bool openState, int labelWidth = -1)
        {
            bool onClick = false;

            if (string.IsNullOrEmpty(label) || string.IsNullOrEmpty(label.Trim()))
            {
                label = "未命名";
            }

            Rect rect = EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("", GUILayout.Width(offsetX));

            if (canShowFoldout)
            {
                Rect posRect = new Rect(rect.x + offsetX, rect.y - 2, 10, 20);
                openState = EditorGUI.Foldout(posRect, openState, ""); //绘制折叠的三角形
                EditorGUILayout.LabelField("", GUILayout.Width(6));
            }

 
            if (labelWidth > 0)
            {
                if (GUILayout.Button(label, gUIStyle, GUILayout.Width(labelWidth)))
                {
                    onClick = true;
                }
            }
            else
            {
                if (GUILayout.Button(label, gUIStyle))
                {
                    onClick = true;
                }
            }

            EditorGUILayout.EndHorizontal();

            return onClick;
        }

        static public bool LabelButton(string label, int width, Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            bool onClick = false;
            GUIStyle style = new GUIStyle();
            style.alignment = alignment;
            style.normal.textColor = color;
            style.normal.background = null;
            if (GUILayout.Button(label, style, GUILayout.Width(width)))
            {
                onClick = true;
            }

            return onClick;
        }

        public static void DrawTitle(string title, Color color, int fontSize = 18, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            GUIStyle style = new GUIStyle();//绘制标题
            style.alignment = alignment;
            style.normal.textColor = color;
            style.fontSize = fontSize; style.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField(title, style);
        }

        public static string DrawTextArea(string label,string stringValue, int labelWidth,int contentWidth,int contentHight)
        {
            EditorGUILayout.BeginHorizontal();
            { 
                EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
                stringValue = EditorGUILayout.TextArea(stringValue,
                    GUILayout.Width(contentWidth), GUILayout.Height(contentHight));
                
            }
            EditorGUILayout.EndHorizontal();
            return stringValue;
        }
    }

  
}