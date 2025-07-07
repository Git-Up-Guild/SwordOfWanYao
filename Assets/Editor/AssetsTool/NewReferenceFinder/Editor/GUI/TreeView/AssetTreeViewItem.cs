using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Events;

namespace ReferenceFinder
{
    public class AssetViewItem : TreeViewItem
    {
        public AssetDescription data;
        public bool isRoot = false;

        public int ChildCount
        {
            get
            {
                return children == null ? 0 : children.Count;
            }
        }

        private int count = 0;

        public int LeafCount
        {
            get
            {
                if (count == 0)
                {
                    if (children != null)
                    {
                        foreach (AssetViewItem child in children)
                        {
                            count += child.ChildCount == 0 ? 1 : child.LeafCount;
                        }
                    }
                }
                return count;
            }
        }

        private HashSet<string> noRepeatLeafPath;

        public HashSet<string> NoRepeatLeafPath
        {
            get
            {
                if (noRepeatLeafPath == null)
                {
                    noRepeatLeafPath = new HashSet<string>();
                    if (children != null)
                    {
                        foreach (AssetViewItem child in children)
                        {
                            if (child.ChildCount == 0)
                            {
                                noRepeatLeafPath.Add(child.data.path);
                            }
                            else
                            {
                                foreach (string leafPath in child.NoRepeatLeafPath)
                                {
                                    noRepeatLeafPath.Add(leafPath);
                                }
                            }
                        }
                    }
                }
                return noRepeatLeafPath;
            }
        }

        public int NoRepeatLeafCount
        {
            get
            {
                return NoRepeatLeafPath.Count;
            }
        }

        public static readonly string normalStateColor = EditorGUIUtility.isProSkin ? "CCCCCCFF" : "000000FF";
        public static readonly string normalStateInfo = $"<color=#{normalStateColor}>Normal</color>";
        public static readonly string changeStateInfo = "<color=#F0672AFF>Changed</color>";
        public static readonly string missingStateInfo = "<color=#FF0000FF>Missing</color>";
        public static readonly string noDataStateInfo = "<color=#FFE300FF>No Data</color>";

        //根据引用信息状态获取状态描述
        public string GetInfoByState()
        {
            switch (data.state)
            {
                case EAssetState.CHANGED:
                    return changeStateInfo;

                case EAssetState.MISSING:
                    return missingStateInfo;

                case EAssetState.NODATA:
                    return noDataStateInfo;

                default:
                    return normalStateInfo;
            }
        }

        public Dictionary<string, string> extensionColorDic = new Dictionary<string, string>()
        {
        };

        public string GetInfoByExtension()
        {
            var extension = data.extension;
            string color = extensionColorDic.ContainsKey(extension) ? extensionColorDic[extension] : normalStateColor;
            return $"<color=#{color}>{extension}</color>";
        }
    }
}