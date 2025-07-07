using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace ReferenceFinder
{
    //资源引用树
    public class AssetTreeView : TreeView
    {
        public AssetTreeView(TreeViewState state) : base(state)
        {
            columnCtrl = new AssetTreeViewColumnCtrl();
            columnCtrl.Create(this);
            multiColumnHeader = columnCtrl.BuildHeader();
            multiColumnHeader.sortingChanged += OnSortingChanged;
            m_predicate = columnCtrl.GetSortComparison(EAssetTreeColumns.Name, false);

            rowHeight = Styles.rowHeights;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = false;
            depthIndentWidth = 10f;
            customFoldoutYOffset = (Styles.rowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
            extraSpaceBeforeIconAndLabel = Styles.iconWidth;
        }

        private static class Styles
        {
            //图标宽度
            public const float iconWidth = 18f;
            //列表高度
            public const float rowHeights = 20f;

            public static GUIStyle ping = new GUIStyle("TV Ping") { clipping = TextClipping.Clip };
            public static GUIStyle lineStyle = "TV Line";
        }

        private static string prefabExtension = ".prefab";

        //记录Event防止被TreeView Use
        private EventType m_curEventType;
        //鼠标是否物体上
        private bool m_isHaveCurRowArgs = false;
        private bool m_isCanReplace = false;
        private RowGUIArgs m_curPosRowArgs;
        private Rect m_curPosRowRect;

        private bool m_needResetExpand = false;
        private bool m_needExpandAll = false;

        private ReferenceFinderConfig config { get { return ReferenceFinderConfig.Instance; } }
        private List<string> selectedAssetGuid => config.selectedAssetGuid;
        private bool IsDepend => config.isDepend;
        private HashSet<string> canReplaceExtension => config.canReplaceExtension;

        public IAssetTreeViewColumnCtrl columnCtrl;
        public EAssetTreeColumns searchColumns;
        public Rect TreeViewRect => treeViewRect;
        public bool needUpdateAssetTree { get; set; }

        #region 创建节点

        protected override TreeViewItem BuildRoot()
        {
            return CreateRootItem();
        }

        public void AddSelectAssetGUID(string guid)
        {
            if (!selectedAssetGuid.Contains(guid) && !Directory.Exists(AssetDatabase.GUIDToAssetPath(guid)))
            {
                selectedAssetGuid.Add(guid);
                needUpdateAssetTree = true;
            }
        }

        public void RemoveSelectAssetGUID(string guid)
        {
            if (selectedAssetGuid.Contains(guid))
            {
                selectedAssetGuid.Remove(guid);
                needUpdateAssetTree = true;
            }
        }

        public void ClearSelectAssetGUID()
        {
            selectedAssetGuid.Clear();
            needUpdateAssetTree = true;
        }

        //生成root相关
        private HashSet<string> updatedAssetSet = new HashSet<string>();

        //通过选择资源列表生成TreeView的根节点
        private AssetViewItem CreateRootItem()
        {
            updatedAssetSet.Clear();
            int elementCount = 0;
            var root = new AssetViewItem 
            {
                id = elementCount, 
                depth = -1, 
                displayName = "Root",
                data = null 
            };

            int depth = 0;
            var stack = new Stack<string>();
            var childItems = new List<AssetViewItem>();
            foreach (var childGuid in selectedAssetGuid)
            {
                var child = CreateTree(childGuid, ref elementCount, depth, stack);

                //根不做过滤
                if (child != null)
                {
                    child.isRoot = true;
                    childItems.Add(child);
                }
            }
            //排序
            if (m_predicate != null)
            {
                childItems.Sort(m_predicate);
            }
            foreach (var child in childItems)
            {
                root.AddChild(child);
            }

            updatedAssetSet.Clear();
            return root;
        }

        //通过每个节点的数据生成子节点
        private AssetViewItem CreateTree(string guid, ref int elementCount, int _depth, Stack<string> stack)
        {
            if (stack.Contains(guid))
            {
                Debug.Log($"有循环依赖：{AssetDatabase.GUIDToAssetPath(guid)}");
                return null;
            }

            stack.Push(guid);
            //if (config.needUpdateState && !updatedAssetSet.Contains(guid))
            if (!updatedAssetSet.Contains(guid))
            {
                ReferenceFinderData.UpdateAssetState(guid);
                updatedAssetSet.Add(guid);
            }
            ++elementCount;

            var referenceData = ReferenceFinderData.Get(guid);
            if (referenceData == null)
            {
                Debug.Log($"资源引用查找库有数据丢失：{guid}");
                return null;
            }

            var root = new AssetViewItem 
            {
                id = elementCount,
                displayName = referenceData.name,
                data = referenceData,
                depth = _depth 
            };

            //显示依赖还是引用
            var childGuids = IsDepend ? referenceData.dependencies : referenceData.references;
            var childItems = new List<AssetViewItem>();
            foreach (var childGuid in childGuids)
            {
                var child = CreateTree(childGuid, ref elementCount, _depth + 1, stack);
                //过滤
                if (child != null && FilterItem(child))
                {
                    childItems.Add(child);
                }
            }

            //排序
            if (m_predicate != null)
            {
                childItems.Sort(m_predicate);
            }
            foreach (var child in childItems)
            {
                root.AddChild(child);
            }

            stack.Pop();
            return root;
        }

        #endregion 创建节点

        #region 绘制

        public override void OnGUI(Rect rect)
        {
            Event p = Event.current;
            m_curEventType = p.type;
            if (selectedAssetGuid != null && selectedAssetGuid.Count > 0)
            {
                UpdateTreeView();
                base.OnGUI(rect);
            }
            else
            {
                EditorGUILayout.HelpBox("拖拽资源或文件夹到此区域，或对资源右键 Find Refrence In Project", MessageType.Info);
            }
            OnUserEventHandle(rect);
        }

        protected override void BeforeRowsGUI()
        {
            m_isHaveCurRowArgs = false;
            base.BeforeRowsGUI();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (AssetViewItem)args.item;

            Event p = Event.current;

            if (args.rowRect.Contains(p.mousePosition))
            {
                m_isHaveCurRowArgs = true;
                m_curPosRowArgs = args;
            }

            columnCtrl.RowGUI(args.rowRect, item, GetContentIndent(item));
            GUI.color = Color.white;
        }

        protected override void AfterRowsGUI()
        {
            base.AfterRowsGUI();
        }

        private void OnUserEventHandle(Rect rect)
        {
            Event p = Event.current;
            DragEventHandle(rect);
            if (m_curEventType == EventType.Repaint)
            {
                if (m_isHaveCurRowArgs)
                {
                    var rowRect = m_curPosRowArgs.rowRect;
                    m_curPosRowRect.x = treeViewRect.x - state.scrollPos.x + rowRect.x;
                    m_curPosRowRect.y = treeViewRect.y - state.scrollPos.y + rowRect.y;
                    m_curPosRowRect.width = rowRect.width;
                    m_curPosRowRect.height = rowRect.height;
                    if (m_isCanReplace)
                    {
                        Styles.ping.Draw(m_curPosRowRect, GUIContent.none, true, true, false, false);
                    }
                    else
                    {
                        Styles.lineStyle.Draw(m_curPosRowRect, GUIContent.none, true, true, false, false);
                    }
                }
                if (m_isCanReplace)
                {
                    m_isCanReplace = false;
                }
                else if (rect.Contains(p.mousePosition))
                {
                    Repaint();
                }
            }
        }

        #endregion 绘制

        #region 更新

        private const int MAX_SELECT_PATH = 300;
        private HashSet<string> isNeedExpandGUIDList = new HashSet<string>();

        public void UpdateTreeView()
        {
            if (needUpdateAssetTree)
            {
                isNeedExpandGUIDList.Clear();
                if (rootItem != null && rootItem.children != null)
                {
                    foreach (AssetViewItem child in rootItem.children)
                    {
                        if (IsExpanded(child.id))
                        {
                            isNeedExpandGUIDList.Add(child.data.path);
                        }
                    }
                }
                Reload();
                if (m_needResetExpand)
                {
                    m_needResetExpand = false;
                }

                if (rootItem.children.Count == 1 || m_needExpandAll)
                {
                    ExpandAll();
                    m_needExpandAll = false;
                }
                else
                {
                    CollapseAll();
                    foreach (AssetViewItem child in rootItem.children)
                    {
                        if (isNeedExpandGUIDList.Contains(child.data.path))
                        {
                            SetExpanded(child.id, true);
                        }
                    }
                    isNeedExpandGUIDList.Clear();
                }
                needUpdateAssetTree = false;
            }
        }

        public void OnSelectionChange()
        {
            Object curSelect = Selection.activeObject;
            string[] guids = Selection.assetGUIDs;
            if (curSelect == null || guids.Length <= 0) return;

            if (config.focusMode)
            {
                if (guids.Length > MAX_SELECT_PATH)
                {
                    Debug.LogError($"资源引用查找 >> 防止误操作，当前限制最多只能选中{MAX_SELECT_PATH}个资源");
                    return;
                }

                ClearSelectAssetGUID();

                UpdateSelectedAssets();

                needUpdateAssetTree = true;
                m_needExpandAll = true;

                Repaint();
            }
        }

        //更新选中资源列表
        public void UpdateSelectedAssets()
        {
            foreach (var obj in Selection.objects)
            {
                AddGuidByPath(AssetDatabase.GetAssetPath(obj));
            }
            m_needResetExpand = true;
            needUpdateAssetTree = true;
        }

        //添加选取路径
        public void AddPaths(string[] paths)
        {
            foreach (var path in paths)
            {
                AddGuidByPath(path);
            }
            needUpdateAssetTree = true;
        }

        //添加选取物体
        public void AddObject(Object[] objects)
        {
            List<string> pathList = new List<string>();

            foreach (var obj in objects)
            {
                if (obj is GameObject)
                {
                    var path = GetPrefabAssetPath(obj);
                    if (!string.IsNullOrEmpty(path))
                        pathList.Add(path);
                }
            }

            var paths = pathList.ToArray();
            foreach (var path in paths)
            {
                AddGuidByPath(path);
            }
            needUpdateAssetTree = true;
        }

        private bool IsPrefabAsset(Object obj)
        {
            return !string.IsNullOrEmpty(GetPrefabAssetPath(obj));
        }

        //获取预制体的资源路径
        private string GetPrefabAssetPath(Object obj)
        {
            if (!(obj is GameObject)) return null;

            GameObject gameObject = obj as GameObject;

            // Project中的Prefab是Asset不是Instance, 预制体资源就是自身
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
                return UnityEditor.AssetDatabase.GetAssetPath(gameObject);

            // Scene中的Prefab Instance是Instance不是Asset
            if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                // 获取预制体资源
                var prefabAsset = UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
                return UnityEditor.AssetDatabase.GetAssetPath(prefabAsset);
            }

            // PrefabMode中的GameObject既不是Instance也不是Asset
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);
            if (prefabStage != null)
            {
                return prefabStage.prefabAssetPath;
            }

            // 不是预制体
            return null;
        }

        public void AddGuidByPath(string path)
        {
            //如果是文件夹
            if (Directory.Exists(path))
            {
                string[] folder = new string[] { path };
                //将文件夹下所有资源作为选择资源
                string[] guids = AssetDatabase.FindAssets(null, folder);

                if (guids.Length > MAX_SELECT_PATH)
                {
                    Debug.LogError($"资源引用查找 >> 防止误操作，当前限制最多只能选中{MAX_SELECT_PATH}个资源");
                    return;
                }

                foreach (var guid in guids)
                {
                    AddSelectAssetGUID(guid);
                }
            }
            //如果是文件资源
            else
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                AddSelectAssetGUID(guid);
            }
        }

        //移除Guid
        public void DeleteGuidByPath(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            RemoveSelectAssetGUID(guid);
        }

        #endregion 更新

        #region 拖拽

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return base.CanStartDrag(args); ;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            base.SetupDragAndDrop(args);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            return base.HandleDragAndDrop(args);
        }

        //鼠标拖拽事件
        private void DragEventHandle(Rect rect)
        {
            Event p = Event.current;
            if (m_curEventType == EventType.DragUpdated && rect.Contains(p.mousePosition))
            {
                if ((
                    DragAndDrop.paths != null 
                    && DragAndDrop.paths.Length > 0
                    ) || (
                    DragAndDrop.objectReferences != null 
                    && DragAndDrop.objectReferences.Length > 0
                    && DragAndDrop.objectReferences.Any(IsPrefabAsset)
                    ))
                {
                    if (m_isHaveCurRowArgs)
                    {
                        var item = (AssetViewItem)m_curPosRowArgs.item;
                        var parent = (AssetViewItem)item.parent;
                        if (
                            canReplaceExtension.Contains(item.data.extension)
                            && parent.data != null
                            && DragAndDrop.paths.Any(path => path.EndsWith(item.data.extension))
                            )
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                            m_isCanReplace = true;
                        }
                        else
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                        }
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    }
                }
            }
            else if (m_curEventType == EventType.DragPerform && rect.Contains(p.mousePosition))
            {
                if ((
                    DragAndDrop.paths != null 
                    && DragAndDrop.paths.Length > 0
                    ) || (
                    DragAndDrop.objectReferences != null 
                    && DragAndDrop.objectReferences.Length > 0
                    && DragAndDrop.objectReferences.Any(IsPrefabAsset)
                    ))
                {
                    if (m_isHaveCurRowArgs)
                    {
                        var item = (AssetViewItem)m_curPosRowArgs.item;
                        var parent = (AssetViewItem)item.parent;
                        if (canReplaceExtension.Contains(item.data.extension)
                            && parent.data != null
                            && DragAndDrop.paths.Any(path => path.EndsWith(item.data.extension)))
                        {
                            foreach (string path in DragAndDrop.paths)
                            {
                                string extension = Path.GetExtension(path);
                                if (extension.CompareTo(item.data.extension) == 0)
                                {
                                    ReplaceAsset(parent.data.path, item.data.path, path);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            AddPaths(DragAndDrop.paths);
                        }
                    }
                    else
                    {
                        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                        {
                            AddPaths(DragAndDrop.paths);
                        }
                        else
                        {
                            AddObject(DragAndDrop.objectReferences);
                        }
                    }
                }
            }
        }

        #endregion 拖拽

        #region 点击事件

        //响应右击事件
        protected override void ContextClickedItem(int id)
        {
            var item = (AssetViewItem)FindItem(id, rootItem);
            rightClickItem = item;
            ShowMenuContext();
        }

        //响应双击事件
        protected override void DoubleClickedItem(int id)
        {
            var item = (AssetViewItem)FindItem(id, rootItem);
            SelectObj(item);
        }

        #endregion 点击事件

        #region 排序

        //排序方法
        private Comparison<AssetViewItem> m_predicate;

        //排序选项修改时
        private void OnSortingChanged(MultiColumnHeader header)
        {
            EAssetTreeColumns column = (EAssetTreeColumns)header.sortedColumnIndex;
            bool isSortedAscending = header.IsSortedAscending((int)column);
            m_predicate = columnCtrl.GetSortComparison(column, isSortedAscending);
            needUpdateAssetTree = true;
        }

        #endregion 排序

        #region 过滤

        //过滤item
        private bool FilterItem(AssetViewItem item)
        {
            var typeDic = ReferenceFinderConfig.Instance.typeFilterDic;
            var stateDic = ReferenceFinderConfig.Instance.stateFilterDic;
            var folderDic = ReferenceFinderConfig.Instance.folderFilterDic;

            //如果子节点包含筛选项，不过滤
            if (item.children != null)
            {
                return true;
            }

            if (typeDic.ContainsKey(item.data.extension) && !typeDic[item.data.extension].isFilter)
            {
                return false;
            }
            else if (stateDic.ContainsKey(item.data.state) && !stateDic[item.data.state].isFilter)
            {
                return false;
            }

            string[] split = item.data.path.Split('/');
            int min = Mathf.Min(1, split.Length);
            string folder = string.Empty;
            for (int i = 0; i < min; i++)
            {
                folder += split[i] + "/";
            }

            if (folderDic.ContainsKey(folder) && !folderDic[folder].isFilter)
            {
                return false;
            }
            return true;
        }

        #endregion 过滤

        #region 搜索

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            AssetViewItem aitem = (AssetViewItem)item;
            return columnCtrl.DoesItemMatchSearch(searchColumns, aitem, search);
        }

        #endregion 搜索

        #region 右键菜单

        private AssetViewItem rightClickItem;
        //显示右键菜单
        private void ShowMenuContext()
        {
            var item = rightClickItem;
            GenericMenu menu = new GenericMenu();
            if (item.parent == rootItem)
            {
                menu.AddItem(new GUIContent("移除显示"), false, DeleteObj);
            }
            else
            {
                menu.AddItem(new GUIContent("添加显示"), false, AddObj);
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("选中文件"), false, SelectObj);
            if (item.data.extension.CompareTo(prefabExtension) == 0)
            {
                menu.AddItem(new GUIContent("打开预制体"), false, OpenPrefab);
            }
            menu.AddItem(new GUIContent("在场景中选中"), false, FindReferencesInScenes);
            menu.AddItem(new GUIContent("在场景中搜索"), false, SearchForReferencesInScenes);
            menu.ShowAsContext();
        }

        //选中文件
        private void SelectObj()
        {
            SelectObj(rightClickItem);
        }

        private void SelectObj(AssetViewItem item)
        {
            //在ProjectWindow中高亮双击资源
            if (item != null)
            {
                var assetObject = AssetDatabase.LoadAssetAtPath(item.data.path, typeof(UnityEngine.Object));
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = assetObject;
                EditorGUIUtility.PingObject(assetObject);
            }
        }

        //移除显示
        private void DeleteObj()
        {
            if (rightClickItem != null)
            {
                DeleteGuidByPath(rightClickItem.data.path);
            }
        }

        //添加显示
        private void AddObj()
        {
            if (rightClickItem != null)
            {
                AddGuidByPath(rightClickItem.data.path);
            }
        }

        //搜索场景中依赖
        private void SearchForReferencesInScenes()
        {
            if (rightClickItem != null)
            {
                var assetObject = AssetDatabase.LoadAssetAtPath(rightClickItem.data.path, typeof(UnityEngine.Object));
                int instanceId = assetObject.GetInstanceID();

                BindingFlags flag = BindingFlags.IgnoreCase | BindingFlags.Static
                    | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod
                    | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Default;

                //反射调用Find References In Scene
                var searchableType = typeof(SearchableEditorWindow);
                searchableType.InvokeMember("SearchForReferencesToInstanceID", flag, null, null, new object[] { instanceId });
            }
        }

        //选中场景中依赖
        private void FindReferencesInScenes()
        {
            if (rightClickItem != null)
            {
                var assetObject = AssetDatabase.LoadAssetAtPath(rightClickItem.data.path, typeof(UnityEngine.Object));
                int instanceId = assetObject.GetInstanceID();

                BindingFlags flag = BindingFlags.IgnoreCase | BindingFlags.Static
                    | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod
                    | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Default;

                //反射调用Find References In Scene
                var searchableType = typeof(SearchableEditorWindow);
                SearchableEditorWindow sew = EditorWindow.GetWindow<SearchableEditorWindow>();

                searchableType.InvokeMember("SearchForReferencesToInstanceID", flag, null, null, new object[] { instanceId });

                Assembly editorAssembly = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
                Type swType = editorAssembly.GetType("UnityEditor.SceneHierarchyWindow");
                EditorWindow lastSW = swType.GetProperty("lastInteractedHierarchyWindow", flag).GetValue(null, null) as EditorWindow;

                Type swhType = editorAssembly.GetType("UnityEditor.SceneHierarchy");
                object lastSWH = swType.GetProperty("sceneHierarchy", flag).GetValue(lastSW, null);

                Assembly imguiAssembly = Assembly.GetAssembly(typeof(TreeView));
                Type swhtwType = imguiAssembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");
                object lastSWHTW = swhType.GetField("m_TreeView", flag).GetValue(lastSWH);

                Type swhtwdsType = imguiAssembly.GetType("UnityEditor.IMGUI.Controls.ITreeViewDataSource");
                object lastSWHTWDS = swhtwType.GetProperty("data", flag).GetValue(lastSWHTW);

                Type gameObjectTreeViewItem = editorAssembly.GetType("UnityEditor.GameObjectTreeViewItem");

                //获取搜索后结果
                IList<TreeViewItem> lastSWHTWDSD = swhtwdsType.InvokeMember("GetRows", flag, null, lastSWHTWDS, new object[] { }) as IList<TreeViewItem>;
                int count = lastSWHTWDSD.Count;
                List<int> targets = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    var treeItem = lastSWHTWDSD[i];
                    //去除场景
                    bool isSceneHeader = (bool)gameObjectTreeViewItem.GetProperty("isSceneHeader", flag).GetValue(treeItem);
                    if (!isSceneHeader)
                    {
                        targets.Add(treeItem.id);
                    }
                }

                //清除搜索
                searchableType.InvokeMember("ClearSearchFilter", flag, null, sew, new object[] { });

                //选中目标
                if (targets.Count == 1)
                {
                    Selection.activeInstanceID = targets[0];
                    EditorGUIUtility.PingObject(targets[0]);
                }
                else if (targets.Count > 1)
                {
                    Selection.instanceIDs = targets.ToArray();
                }
                else
                {
                    Debug.LogError($"资源引用查找 >> 场景中找不到 {rightClickItem.data.name}");
                }
            }
        }

        //打开预制体
        private void OpenPrefab()
        {
            if (rightClickItem != null)
            {
                var assetObject = AssetDatabase.LoadAssetAtPath(rightClickItem.data.path, typeof(UnityEngine.Object));
                int instanceId = assetObject.GetInstanceID();

                typeof(UnityEditor.SceneManagement.PrefabStageUtility).InvokeMember("OpenPrefab",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                    null, null, new object[] { rightClickItem.data.path, });
            }
        }

        //替换资源
        private void ReplaceAsset(string prefabPath, string srcPath, string desPath)
        {
            string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
            string srcImgGuid = AssetDatabase.AssetPathToGUID(srcPath);
            string dstImgGuid = AssetDatabase.AssetPathToGUID(desPath);

            if (!string.IsNullOrEmpty(prefabPath) && !string.IsNullOrEmpty(srcImgGuid) && !string.IsNullOrEmpty(dstImgGuid))
            {
                string[] allLines = File.ReadAllLines(prefabPath);

                for (int i = 0; i < allLines.Length; i++)
                {
                    if (allLines[i].Contains(srcImgGuid))
                    {
                        allLines[i] = allLines[i].Replace(srcImgGuid, dstImgGuid);
                    }
                }

                File.WriteAllLines(prefabPath, allLines);

                AssetDatabase.ImportAsset(prefabPath);

                needUpdateAssetTree = true;
            }
            ReferenceFinderData.UpdateAssetDepencies(prefabGuid);
            ReferenceFinderData.ForceUpdateRefrenceInfo();
        }

        #endregion 右键菜单
    }
}