using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ReferenceFinder
{
    public class ReferenceFinderWindow : EditorWindow
    {
        private static class Styles
        {
            //工具栏按钮样式
            public static GUIStyle toolbarButtonGUIStyle = new GUIStyle("ToolbarButton");
            //工具栏样式
            public static GUIStyle toolbarGUIStyle = new GUIStyle("Toolbar");

            public static GUIStyle iconButtonStyle = new GUIStyle("IconButton");
            public static GUIStyle searchFieldStyle = new GUIStyle("ToolbarSeachTextFieldPopup");
            public static GUIStyle searchCancelButtontyle = new GUIStyle("ToolbarSeachCancelButton");
            public static GUIStyle textFieldRoundEdgeStyle = new GUIStyle("PR DisabledLabel");

            public static GUIContent helpBtnContent = new GUIContent("", EditorGUIUtility.IconContent("_Help").image, "资源引用查找工具-帮助文档");
        }

        private bool m_refreshing = false;
        private int m_refreshingCount = 0;
        private EventType tempEventType;
        private GenericMenu searchColumnsMenu;
        private ReferenceFinderConfig config { get { return ReferenceFinderConfig.Instance; } }

        public AssetTreeView assetTreeView;
        private TreeViewState m_TreeViewState;

        //查找资源引用信息
        [MenuItem("Assets/Find References In Project %#&f", priority = 25)]
        private static void FindRef()
        {
            var dic = ReferenceFinderData.AssetDict;
            OpenWindow();
            ReferenceFinderWindow window = GetWindow<ReferenceFinderWindow>();
            window.assetTreeView.UpdateSelectedAssets();
        }

        //打开窗口
        [MenuItem("XGame/资源工具/资源引用查找窗口", priority = 1)]
        public static void OpenWindow()
        {
            ReferenceFinderWindow window = GetWindow<ReferenceFinderWindow>();
            window.wantsMouseMove = false;
            var icon = EditorGUIUtility.FindTexture("Search Icon");
            window.titleContent = new GUIContent("资源引用查找窗口", icon);
            window.Show();
            window.Focus();
        }

        //Unity隐藏函数：自定义菜单
        private void ShowButton(Rect position)
        {
            if (GUI.Button(position, Styles.helpBtnContent, Styles.iconButtonStyle))
            {
                string path = Path.Combine(ReferenceFinderConfig.RelativePath, ReferenceFinderConfig.helpDocPath);
                path = Path.Combine(Application.dataPath.Remove(Application.dataPath.Length - 6, 6), path);
                Application.OpenURL("file:///" + path);
            }
        }

        private void OnEnable()
        {
            if (assetTreeView == null)
            {
                //初始化TreeView
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();

                assetTreeView = new AssetTreeView(m_TreeViewState);
            }
            assetTreeView.needUpdateAssetTree = true;
        }

        private void OnDisable()
        {
        }

        private void OnSelectionChange()
        {
            assetTreeView.OnSelectionChange();
        }

        #region 绘制

        private void OnGUI()
        {
            Event p = Event.current;
            tempEventType = p.type;
            DrawOptionBar();
            var treeRect = new Rect(0, Styles.toolbarGUIStyle.fixedHeight, position.width, position.height - Styles.toolbarGUIStyle.fixedHeight);

            //绘制Treeview
            assetTreeView.OnGUI(treeRect);
        }

        public void DrawOptionBar()
        {
            EditorGUILayout.BeginHorizontal(Styles.toolbarGUIStyle);
            {
                GUIContent refreshContent = new GUIContent("刷新", m_refreshing ? EditorGUIUtility.IconContent($"WaitSpin{m_refreshingCount.ToString("00")}").image : EditorGUIUtility.FindTexture("TreeEditor.Refresh"));

                if (Event.current.type == EventType.Layout)
                {
                    m_refreshingCount++;
                    m_refreshingCount %= 12;
                }

                //刷新数据
                if (GUILayout.Button(refreshContent, Styles.toolbarButtonGUIStyle, GUILayout.Width(50)))
                {
                    m_refreshing = true;
                    ReferenceFinderData.CollectDependenciesInfo(false, () => m_refreshing = false);
                    assetTreeView.needUpdateAssetTree = true;
                    EditorGUIUtility.ExitGUI();
                }
                //修改模式
                bool isDepend = ReferenceFinderConfig.Instance.isDepend;
                isDepend = GUILayout.Toggle(isDepend, isDepend
                    ? new GUIContent("查依赖", EditorGUIUtility.FindTexture("tree_icon_frond"))
                    : new GUIContent("查引用", EditorGUIUtility.FindTexture("tree_icon_leaf")),
                    Styles.toolbarButtonGUIStyle,
                    GUILayout.Width(65));
                if (ReferenceFinderConfig.Instance.isDepend != isDepend)
                {
                    ReferenceFinderConfig.Instance.isDepend = isDepend;
                    assetTreeView.needUpdateAssetTree = true;
                }

                //是否需要更新状态
                //config.needUpdateState = GUILayout.Toggle(config.needUpdateState, config.needUpdateState
                //    ? new GUIContent("状态检查", EditorGUIUtility.FindTexture("lightMeter/greenLight"))
                //    : new GUIContent("状态检查", EditorGUIUtility.FindTexture("lightMeter/lightOff")),
                //    Styles.toolbarButtonGUIStyle,
                //    GUILayout.Width(70));

                //专注模式
                config.focusMode = GUILayout.Toggle(config.focusMode, config.focusMode
                    ? new GUIContent("专注模式", EditorGUIUtility.FindTexture("lightMeter/greenLight"))
                    : new GUIContent("专注模式", EditorGUIUtility.FindTexture("lightMeter/lightOff")),
                    Styles.toolbarButtonGUIStyle,
                    GUILayout.Width(70));

                //搜索栏
                Rect searchRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                var searchString = assetTreeView != null ? assetTreeView.searchString : string.Empty;
                searchString = GUI.TextField(searchRect, searchString, Styles.searchFieldStyle);
                float width = searchRect.width;

                EditorGUIUtility.AddCursorRect(searchRect, MouseCursor.Text);

                searchRect.width = 15;
                if ((tempEventType == EventType.MouseDown && searchRect.Contains(Event.current.mousePosition)))
                {
                    //当放大镜被点击
                    searchColumnsMenu = new GenericMenu();

                    foreach (EAssetTreeColumns columnType in assetTreeView.columnCtrl.GetComlumnType())
                    {
                        var column = assetTreeView.columnCtrl.GetColumn(columnType);
                        if (column != null && column is ITreeViewSearchableColumn<AssetViewItem>)
                        {
                            searchColumnsMenu.AddItem(new GUIContent(column.ColumnName), assetTreeView.searchColumns == columnType, () =>
                            {
                                assetTreeView.searchColumns = columnType;
                            });
                        }
                    }
                    searchColumnsMenu.ShowAsContext();
                }

                //取消键
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchRect.x = searchRect.x + width - 15;
                    GUI.Label(searchRect, string.Empty, Styles.searchCancelButtontyle);
                    if ((tempEventType == EventType.MouseDown && searchRect.Contains(Event.current.mousePosition)))
                    {
                        searchString = "";
                    }
                }
                else
                {
                    if (tempEventType == EventType.Repaint)
                    {
                        searchRect.x = searchRect.x + 17;
                        searchRect.width = 30;
                        Styles.textFieldRoundEdgeStyle.Draw(searchRect, new GUIContent("All"), 0);
                    }
                }
                assetTreeView.searchString = searchString;

                //过滤项
                if (GUILayout.Button(new GUIContent("筛选", EditorGUIUtility.FindTexture("FilterByType")), Styles.toolbarButtonGUIStyle, GUILayout.Width(50)))
                {
                    ReferenceFinderFilterWindow.OpenWindow(this);
                }

                //清除
                if (GUILayout.Button(new GUIContent("清除", EditorGUIUtility.FindTexture("TreeEditor.Trash")), Styles.toolbarButtonGUIStyle, GUILayout.Width(50)))
                {
                    assetTreeView.ClearSelectAssetGUID();
                }

                //扩展
                var Expand = EditorGUIUtility.IconContent("d_scrolldown");
                if (GUILayout.Button(new GUIContent("扩展", Expand.image), Styles.toolbarButtonGUIStyle, GUILayout.Width(50)))
                {
                    assetTreeView.ExpandAll();
                }

                //折叠
                var Collapse = EditorGUIUtility.IconContent("d_scrollup");
                if (GUILayout.Button(new GUIContent("收缩", Collapse.image), Styles.toolbarButtonGUIStyle, GUILayout.Width(50)))
                {
                    assetTreeView.CollapseAll();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion 绘制
    }
}