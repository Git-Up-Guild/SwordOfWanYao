using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace XGameEditor.AssetImportTool
{
    public static class XmlHelper
    {
        /// <summary>
        /// 加载Xml文档
        /// </summary>
        /// <param name="xmlPath">文档路径</param>
        /// <param name="ignoreComments">是否忽略注释</param>
        /// <returns></returns>
        public static XmlDocument LoadXmlDocument(string xmlPath, bool ignoreComments = true)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlReader reader = null;
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = ignoreComments;//忽略文档里面的注释
                reader = XmlReader.Create(xmlPath, settings);
                xmlDoc.Load(reader);
            }
            catch (Exception e)
            {
                Debug.LogError($"xml加载失败：{e.Message}, file: {xmlPath}");
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }

            return xmlDoc;
        }

        public static XmlNode GetXmlNode(string xmlPath, string nodePath, bool ignoreComments = true)
        {
            XmlDocument xmlDoc = LoadXmlDocument(xmlPath, ignoreComments);
            return GetXmlNode(xmlDoc, nodePath);
        }

        public static XmlNode GetXmlNode(XmlNode parentNode, string path)
        {
            return parentNode.SelectSingleNode(path);
        }

        public static string GetXmlAttributeValue(XmlNode parentNode, string propName)
        {
            XmlAttribute xmlAttribut = GetXmlAttribute(parentNode, propName);
            if (xmlAttribut != null)
            {
                return xmlAttribut.Value;
            }
            return null;
        }

        public static XmlAttribute GetXmlAttribute(XmlNode parentNode, string propName)
        {
            if (parentNode.Attributes.Count > 0)
            {
                return parentNode.Attributes[propName];
            }
            return null;
        }

        /// <summary>
        /// 根据给定的属性名和值来获取相应的xmlNode，如果有多个，只会返回第一个找到的
        /// </summary>
        /// <param name="parentNode">查找节点的父节点</param>
        /// <param name="nodeName">节点名</param>
        /// <param name="attributeName">属性名</param>
        /// <param name="attributeValue">属性值</param>
        /// <param name="bIgnoreCase">是否忽略大小写</param>
        /// <param name="isFullMatch">属性值是否完全匹配</param>
        /// <returns></returns>
        public static XmlNode GetXmlNodeByAttribute(XmlNode parentNode, string nodeName, string attributeName, string attributeValue, bool bIgnoreCase = true, bool isFullMatch = false)
        {
            if (string.IsNullOrEmpty(attributeValue)) return null;

            XmlNodeList matchNodesList = parentNode.SelectNodes($"//{nodeName}[@{attributeName}]");  //先筛选出带此属性的节点列表
            if (matchNodesList.Count > 0)
            {
                StringComparison stringComparison = bIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                foreach (XmlNode item in matchNodesList)
                {
                    XmlAttribute xmlAttribute = GetXmlAttribute(item, attributeName);
                    if (xmlAttribute != null)
                    {
                        if (isFullMatch)
                        {
                            if (xmlAttribute.Value.Equals(attributeValue, stringComparison)) return item;
                        }
                        else
                        {
                            if (xmlAttribute.Value.IndexOf(attributeValue, stringComparison) != -1) return item;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定名字的节点Node
        /// </summary>
        /// <param name="parentNode">父节点</param>
        /// <param name="nodeName">待查找的节点名字</param>
        /// <returns></returns>
        public static XmlNodeList GetXmlNodeList(XmlNode parentNode, string nodeName)
        {
            return parentNode.SelectNodes(nodeName);
        }

        public static bool HasXmlNode(XmlNode parentNode, string nodeName, bool isRecursion = false)
        {
            if (isRecursion)
            {
                //递归比较耗性能
                List<XmlNode> xmlNodeList = new List<XmlNode>();
                GetXmlNodeList(parentNode, nodeName, ref xmlNodeList, isRecursion);
                return xmlNodeList.Count > 0;
            }
            else
                return GetXmlNodeList(parentNode, nodeName).Count > 0;
        }

        //查找parentNode层下所有名字为nodeName的节点（查到后不继续判断nodeName，性能考虑）
        public static void GetXmlNodeList(XmlNode parentNode, string nodeName, ref List<XmlNode> AllNodeList, bool isRecursion = false)
        {
            XmlNodeList childList = parentNode.ChildNodes;
            foreach (XmlNode item in childList)
            {
                if (item.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    AllNodeList.Add(item);
                }
                else
                {
                    if (isRecursion)
                        GetXmlNodeList(item, nodeName, ref AllNodeList, isRecursion);
                }
            }
        }
    }
}