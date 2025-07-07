/*****************************************************
** 文 件 名：TreeMenu.cs
** 版    本：V1.0
** 创 建 人：李世柳
** 创建日期：2020/1/12 16:49:54
** 内容简述：树形菜单面板 
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XGameEditor.MapBehavior
{
    public class TreeMenuItem
    {
        public string name;
        public object userData;

        public bool bSelect = false;
        public bool openState = true;

        public TreeMenuItem(string name, object userData = null)
        {
            this.name = name;
            this.userData = userData;
        }
        public List<TreeMenuItem> childInfo = new List<TreeMenuItem>();
    }

    public interface ITreeMenu
    {
        void OnDraw(Vector2 pos, TreeMenuItem item);
        TreeMenuItem GetSelectItem();

        object GetSelectUserData();
    }

    public class TreeMenu: ITreeMenu
    {
        Color defauleColor;
        Color bSelectColor;
        System.Action<TreeMenuItem> onDrawItemCallBack = null;
        int labelWidth;
        public static ITreeMenu Create(Color defauleColor, Color bSelectColor, System.Action<TreeMenuItem> onDrawItemCallBack = null)
        {
            TreeMenu menu = new TreeMenu();
            menu.defauleColor = defauleColor;
            menu.bSelectColor = bSelectColor;
            menu.onDrawItemCallBack = onDrawItemCallBack;
           
            return menu;
        }
        private TreeMenu()
        {

        }
        public void OnDraw(Vector2 pos, TreeMenuItem item)
        { 
            treeIndex = 0;
             
            DrawLabel(item,0); 
        }

        public TreeMenuItem GetSelectItem()
        {
            return selectItem;
        }
        public object GetSelectUserData()
        {
            return selectItem != null ? selectItem.userData : null;
        }

        TreeMenuItem selectItem;
        Vector2 scrolPos;


        int treeIndex = 0;
        void DrawLabel(TreeMenuItem item, int space = 1)
        {
            if (item == null) return;
            Vector2 pos = new Vector2();
            pos.x = space * 20;
            pos.y = treeIndex * 20;

            if (DrawFoldout(item, pos, onDrawItemCallBack))
            {
                if (item != selectItem) {
                    if (selectItem != null)
                    {
                        selectItem.bSelect = false;
                    }
                    selectItem = item;
                    selectItem.bSelect = true;
                }
                
            }
            if (item.openState)
            {
                for (int i = 0; i < item.childInfo.Count; i++)
                {
                    DrawLabel(item.childInfo[i], space + 1);
                }
            }
            treeIndex++;
        }
       
        private bool DrawFoldout(TreeMenuItem item, Vector2 position, System.Action<TreeMenuItem> onDrawEnd)
        {
            if (item == null) return false;
            bool onClick = false;
            Rect rect = EditorGUILayout.BeginHorizontal();

            float allWidth = 300;

            string label = item.name;
            if (string.IsNullOrEmpty(label.Trim()) || label.Trim().Length == 0)
            {
                label = "未命名";
            }

            EditorGUILayout.LabelField("", GUILayout.Width(position.x));
            allWidth -= position.x;
            if (item.childInfo.Count > 0)
            {
                Rect r = new Rect();
                r.x = position.x;
                r.y = rect.y;
                r.width = 15;
                r.height = 15;
                item.openState = EditorGUI.Foldout(r, item.openState,"");
                EditorGUILayout.LabelField("", GUILayout.Width(15));
                allWidth -= 19;//15+4
            }
            else
            {
            }

            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = item == selectItem ? bSelectColor : defauleColor;

            style.normal.background = null;

            if (GUILayout.Button(label, style,GUILayout.Width(allWidth-20)))
            {
                onClick = true;
            }


            if (onDrawEnd != null)
            {
                onDrawEnd(item);
            }
            EditorGUILayout.EndHorizontal();

            return onClick;
        }
    }
}
 