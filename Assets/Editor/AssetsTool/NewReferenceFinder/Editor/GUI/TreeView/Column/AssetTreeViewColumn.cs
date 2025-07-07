using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ReferenceFinder
{
    //资源引用列信息
    public enum EAssetTreeColumns
    {
        Name,
        Path,
        Type,
        State,
        Count,
        NoRepeatLeaf,
        Leaf,
    }

    public interface ITreeViewColumn<in Item> where Item : TreeViewItem
    {
        string ColumnName { get; }

        int ColumnIndex { get; set; }

        MultiColumnHeaderState.Column GetColumn();

        void CellGUI(Rect cellRect, Item item, float contentIndent = 0);
    }

    public interface ITreeViewSortableColumn<in Item> where Item : TreeViewItem
    {
        int SortPriority { get; }

        Comparison<Item> SortComparsion { get; }
    }

    public interface ITreeViewSearchableColumn<in Item> where Item : TreeViewItem
    {
        bool DoesItemMatchSearch(Item item, string search);
    }

    public interface IAssetTreeViewColumn : ITreeViewColumn<AssetViewItem>, ITreeViewSortableColumn<AssetViewItem>, ITreeViewSearchableColumn<AssetViewItem>
    {
        EAssetTreeColumns type { get; }
    }

    public class AssetTreeColumnName : IAssetTreeViewColumn
    {
        public EAssetTreeColumns type => EAssetTreeColumns.Name;

        public string ColumnName => "名称";

        public int ColumnIndex { get; set; }

        public int SortPriority => 0;

        public Comparison<AssetViewItem> SortComparsion => SortCompare;

        private MultiColumnHeaderState.Column column;

        public MultiColumnHeaderState.Column GetColumn()
        {
            if (column == null)
            {
                column = AssetTreeColumnName.CreateDefaultColumn();
                column.headerContent = new GUIContent(ColumnName);
                column.width = 150;
            }
            return column;
        }

        public void CellGUI(Rect cellRect, AssetViewItem item, float contentIndent)
        {
            var rect = cellRect;
            rect.x += contentIndent;
            rect.width = 18f;
            if (rect.x < cellRect.xMax)
            {
                var icon = GetIcon(item.data.path);
                if (icon != null)
                    GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
            }

            rect.x += rect.width;
            rect.width = cellRect.width - rect.x;
            GUI.Label(rect, item.displayName);
        }

        private int SortCompare(AssetViewItem f, AssetViewItem s)
        {
            return string.CompareOrdinal(f.data.name, s.data.name);
        }

        //根据资源信息获取资源图标
        private Texture2D GetIcon(string path)
        {
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            if (obj != null)
            {
                Texture2D icon = AssetPreview.GetMiniThumbnail(obj);
                if (icon == null)
                    icon = AssetPreview.GetMiniTypeThumbnail(obj.GetType());
                return icon;
            }
            return null;
        }

        public static MultiColumnHeaderState.Column CreateDefaultColumn()
        {
            return new MultiColumnHeaderState.Column
            {
                headerTextAlignment = TextAlignment.Center,
                width = 60,
                minWidth = 60,
                autoResize = false,
                allowToggleVisibility = false,
                canSort = true
            };
        }

        public bool DoesItemMatchSearch(AssetViewItem item, string search)
        {
            return item.data.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public class AssetTreeColumnPath : IAssetTreeViewColumn
    {
        public EAssetTreeColumns type => EAssetTreeColumns.Path;

        public string ColumnName => "路径";

        public int ColumnIndex { get; set; }

        public int SortPriority => 1;

        public Comparison<AssetViewItem> SortComparsion => SortCompare;

        private MultiColumnHeaderState.Column column;

        public MultiColumnHeaderState.Column GetColumn()
        {
            if (column == null)
            {
                column = AssetTreeColumnName.CreateDefaultColumn();
                column.headerContent = new GUIContent(ColumnName);
                column.width = 200;
            }
            return column;
        }

        public void CellGUI(Rect cellRect, AssetViewItem item, float contentIndent)
        {
            GUI.Label(cellRect, item.data.path);
        }

        private int SortCompare(AssetViewItem f, AssetViewItem s)
        {
            return string.CompareOrdinal(f.data.path, s.data.path);
        }

        public bool DoesItemMatchSearch(AssetViewItem item, string search)
        {
            bool isMatch = item.data.path.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
            //if (!isMatch)
            //{
            //    foreach(AssetViewItem child in item.children)
            //    {
            //        isMatch &= DoesItemMatchSearch(item, search);
            //    }
            //}
            return isMatch;
        }
    }

    public class AssetTreeColumnType : IAssetTreeViewColumn
    {
        private GUIStyle typeGUIStyle = new GUIStyle { richText = true, alignment = TextAnchor.MiddleCenter };

        public EAssetTreeColumns type => EAssetTreeColumns.Type;

        public string ColumnName => "类型";

        public int ColumnIndex { get; set; }

        public int SortPriority => 2;

        public Comparison<AssetViewItem> SortComparsion => SortCompare;

        private MultiColumnHeaderState.Column column;

        public MultiColumnHeaderState.Column GetColumn()
        {
            if (column == null)
            {
                column = AssetTreeColumnName.CreateDefaultColumn();
                column.headerContent = new GUIContent(ColumnName);
            }
            return column;
        }

        public void CellGUI(Rect cellRect, AssetViewItem item, float contentIndent)
        {
            GUI.Label(cellRect, item.GetInfoByExtension(), typeGUIStyle);
        }

        private int SortCompare(AssetViewItem f, AssetViewItem s)
        {
            return string.CompareOrdinal(f.data.extension, s.data.extension);
        }

        public bool DoesItemMatchSearch(AssetViewItem item, string search)
        {
            return item.data.extension.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public class AssetTreeColumnState : IAssetTreeViewColumn
    {
        private GUIStyle stateGUIStyle = new GUIStyle { richText = true, alignment = TextAnchor.MiddleCenter };

        public EAssetTreeColumns type => EAssetTreeColumns.State;

        public string ColumnName => "状态";

        public int ColumnIndex { get; set; }

        public int SortPriority => 2;

        public Comparison<AssetViewItem> SortComparsion => SortCompare;

        private MultiColumnHeaderState.Column column;

        public MultiColumnHeaderState.Column GetColumn()
        {
            if (column == null)
            {
                column = AssetTreeColumnName.CreateDefaultColumn();
                column.headerContent = new GUIContent(ColumnName);
            }
            return column;
        }

        public void CellGUI(Rect cellRect, AssetViewItem item, float contentIndent)
        {
            GUI.Label(cellRect, item.GetInfoByState(), stateGUIStyle);
        }

        private int SortCompare(AssetViewItem f, AssetViewItem s)
        {
            return f.data.state - s.data.state;
        }

        public bool DoesItemMatchSearch(AssetViewItem item, string search)
        {
            return item.data.state.ToString().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public class AssetTreeColumnCount : IAssetTreeViewColumn
    {
        private GUIStyle countGUIStyle;

        public EAssetTreeColumns type => EAssetTreeColumns.Count;

        public string ColumnName => "数量";

        public int ColumnIndex { get; set; }

        public int SortPriority => 2;

        public Comparison<AssetViewItem> SortComparsion => SortCompare;

        private MultiColumnHeaderState.Column column;

        public MultiColumnHeaderState.Column GetColumn()
        {
            if (column == null)
            {
                column = AssetTreeColumnName.CreateDefaultColumn();
                column.headerContent = new GUIContent(ColumnName);
            }
            return column;
        }

        public void CellGUI(Rect cellRect, AssetViewItem item, float contentIndent)
        {
            if (countGUIStyle == null)
                countGUIStyle = new GUIStyle("Label") { alignment = TextAnchor.MiddleCenter };
            GUI.Label(cellRect, item.ChildCount.ToString(), countGUIStyle);
        }

        private int SortCompare(AssetViewItem f, AssetViewItem s)
        {
            return f.ChildCount - s.ChildCount;
        }

        public bool DoesItemMatchSearch(AssetViewItem item, string search)
        {
            return item.ChildCount.ToString().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public class AssetTreeColumnNoRepeatLeaf : IAssetTreeViewColumn
    {
        private GUIStyle countGUIStyle;

        public EAssetTreeColumns type => EAssetTreeColumns.NoRepeatLeaf;

        public string ColumnName => "非重复叶子数";

        public int ColumnIndex { get; set; }

        public int SortPriority => 2;

        public Comparison<AssetViewItem> SortComparsion => SortCompare;

        private MultiColumnHeaderState.Column column;

        public MultiColumnHeaderState.Column GetColumn()
        {
            if (column == null)
            {
                column = AssetTreeColumnName.CreateDefaultColumn();
                column.headerContent = new GUIContent(ColumnName);
                column.width = 80;
            }
            return column;
        }

        public void CellGUI(Rect cellRect, AssetViewItem item, float contentIndent)
        {
            if (countGUIStyle == null)
                countGUIStyle = new GUIStyle("Label") { alignment = TextAnchor.MiddleCenter };
            GUI.Label(cellRect, item.NoRepeatLeafCount.ToString(), countGUIStyle);
        }

        private int SortCompare(AssetViewItem f, AssetViewItem s)
        {
            return f.NoRepeatLeafCount - s.NoRepeatLeafCount;
        }

        public bool DoesItemMatchSearch(AssetViewItem item, string search)
        {
            return item.NoRepeatLeafCount.ToString().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public class AssetTreeColumnLeaf : IAssetTreeViewColumn
    {
        private GUIStyle countGUIStyle;

        public EAssetTreeColumns type => EAssetTreeColumns.Leaf;

        public string ColumnName => "叶子数";

        public int ColumnIndex { get; set; }

        public int SortPriority => 2;

        public Comparison<AssetViewItem> SortComparsion => SortCompare;

        private MultiColumnHeaderState.Column column;

        public MultiColumnHeaderState.Column GetColumn()
        {
            if (column == null)
            {
                column = AssetTreeColumnName.CreateDefaultColumn();
                column.headerContent = new GUIContent(ColumnName);
            }
            return column;
        }

        public void CellGUI(Rect cellRect, AssetViewItem item, float contentIndent)
        {
            if (countGUIStyle == null)
                countGUIStyle = new GUIStyle("Label") { alignment = TextAnchor.MiddleCenter };
            GUI.Label(cellRect, item.LeafCount.ToString(), countGUIStyle);
        }

        private int SortCompare(AssetViewItem f, AssetViewItem s)
        {
            return f.LeafCount - s.LeafCount;
        }

        public bool DoesItemMatchSearch(AssetViewItem item, string search)
        {
            return item.LeafCount.ToString().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}