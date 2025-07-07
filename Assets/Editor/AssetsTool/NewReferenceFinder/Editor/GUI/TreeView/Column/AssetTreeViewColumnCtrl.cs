using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ReferenceFinder
{
    public interface ITreeViewColumnCtrl<in View, in Item> where View : TreeView where Item : TreeViewItem
    {
        bool Create(View treeView);

        MultiColumnHeader BuildHeader();

        void RowGUI(Rect rowRect, Item item, float contentIndent = 0);
    }

    public interface ITreeViewEnumColumnCtrl<EnumType, out Column, in Item> where EnumType : Enum where Column : ITreeViewColumn<Item> where Item : TreeViewItem
    {
        IEnumerable<EnumType> GetComlumnType();

        Column GetColumn(EnumType columnType);

        Comparison<Item> GetSortComparison(EnumType enumType, bool isSortedAscending);

        bool DoesItemMatchSearch(EnumType enumType, Item item, string search);
    }

    public interface IAssetTreeViewColumnCtrl : ITreeViewColumnCtrl<AssetTreeView, AssetViewItem>, ITreeViewEnumColumnCtrl<EAssetTreeColumns, IAssetTreeViewColumn, AssetViewItem>
    {
    }

    public class AssetTreeViewColumnCtrl : IAssetTreeViewColumnCtrl
    {
        private AssetTreeView treeView;

        private class EnumComparer<T> : IEqualityComparer<T> where T : Enum
        {
            public bool Equals(T first, T second)
            {
                return Convert.ToInt32(first).Equals(Convert.ToInt32(second));
            }

            public int GetHashCode(T instance)
            {
                return Convert.ToInt32(instance);
            }
        }

        private Dictionary<EAssetTreeColumns, IAssetTreeViewColumn> m_columns;
        private List<IAssetTreeViewColumn> m_columnList;

        public bool Create(AssetTreeView treeView)
        {
            this.treeView = treeView;
            m_columnList = new List<IAssetTreeViewColumn>();
            m_columnList.Add(new AssetTreeColumnName());
            m_columnList.Add(new AssetTreeColumnPath());
            m_columnList.Add(new AssetTreeColumnType());
            m_columnList.Add(new AssetTreeColumnState());
            m_columnList.Add(new AssetTreeColumnCount());
            m_columnList.Add(new AssetTreeColumnNoRepeatLeaf());
            m_columnList.Add(new AssetTreeColumnLeaf());

            var comparer = new EnumComparer<EAssetTreeColumns>();
            EAssetTreeColumns keySelector(IAssetTreeViewColumn column) => column.type;
            IAssetTreeViewColumn valueSelector(IAssetTreeViewColumn column) => column;
            m_columns = m_columnList.ToDictionary(keySelector, valueSelector, comparer);

            m_comparision = new List<Comparison<AssetViewItem>>();
            return true;
        }

        public IEnumerable<EAssetTreeColumns> GetComlumnType()
        {
            if (m_columns != null)
            {
                return m_columns.Keys;
            }
            return null;
        }

        public IAssetTreeViewColumn GetColumn(EAssetTreeColumns columnType)
        {
            if (m_columns != null && m_columns.ContainsKey(columnType))
            {
                return m_columns[columnType];
            }
            return null;
        }

        private List<Comparison<AssetViewItem>> m_comparision;
        private bool m_isSortedAscending;
        private List<IAssetTreeViewColumn> tempList = new List<IAssetTreeViewColumn>();

        public Comparison<AssetViewItem> GetSortComparison(EAssetTreeColumns type, bool isSortedAscending)
        {
            m_comparision.Clear();
            tempList.Clear();
            var topColumn = GetColumn(type);

            foreach (var column in m_columnList)
            {
                if (column.SortPriority < topColumn.SortPriority)
                {
                    tempList.Add(column);
                }
            }
            int order(IAssetTreeViewColumn column) { return column.SortPriority; };

            m_comparision.Add(topColumn.SortComparsion);
            foreach (var column in tempList.OrderBy(order))
            {
                m_comparision.Add(column.SortComparsion);
            }

            m_isSortedAscending = isSortedAscending;
            return Comparision;
        }

        private int Comparision(AssetViewItem f, AssetViewItem s)
        {
            int index = 0;
            foreach (var pre in m_comparision)
            {
                int result = pre(f, s);
                if (result != 0)
                    return index == 0 ? m_isSortedAscending ? -result : result : result;
                index++;
            }
            return 0;
        }

        public MultiColumnHeader BuildHeader()
        {
            MultiColumnHeaderState.Column selectColumn(IAssetTreeViewColumn treeColumn) => treeColumn.GetColumn();
            MultiColumnHeaderState.Column[] columns = m_columnList.Select(selectColumn).ToArray();
            var state = new MultiColumnHeaderState(columns);
            var hearder = new MultiColumnHeader(state);
            return hearder;
        }

        public void RowGUI(Rect rowRect, AssetViewItem item, float contentIndent = 0)
        {
            MultiColumnHeaderState.Column[] columns = treeView.multiColumnHeader.state.columns;
            for (int i = 0; i < columns.Length; i++)
            {
                if (!treeView.multiColumnHeader.IsColumnVisible(i)) continue;
                int visableIndex = treeView.multiColumnHeader.GetVisibleColumnIndex(i);
                Rect cellRect = treeView.multiColumnHeader.GetCellRect(visableIndex, rowRect);
                m_columnList[i].CellGUI(cellRect, item, contentIndent);
            }
        }

        public bool DoesItemMatchSearch(EAssetTreeColumns enumType, AssetViewItem item, string search)
        {
            var column = GetColumn(enumType);
            if (column != null)
            {
                return column.DoesItemMatchSearch(item, search);
            }
            return true;
        }
    }
}