using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace XGameEditor.AssetImportTool
{
    //资源导入器检测器
    public class AssetImporterChecker
    {
        //正式项目（策划正确配置xml）时候才能打开
        public static bool isOpenChecker = false;

        private static XmlNode rootNode;
        public static XmlNode RootNode
        {
            get
            {
                if (rootNode == null)
                    rootNode = XmlHelper.GetXmlNode(ImportDefine.ImportSettingXmlPath, ImportDefine.Root);
                return rootNode;
            }
        }

        /// <summary>
        /// 检测资源导入设置是否和配置表一致
        /// </summary>
        /// <param name="assetPath">资源相对路径</param>
        /// <param name="isShowAllDifferent">是否显示所有的不一致属性,默认false</param>
        /// <returns>有配置且配置一致返回true（配置不一致返回false），或者没配置也返回true</returns>
        public static bool CheckAssetImporterSetting(string assetPath, bool isShowAllDifferent = false)
        {
            if (!isOpenChecker) return true;

            string postfix = Path.GetExtension(assetPath);
            if (string.IsNullOrEmpty(postfix)) return true;

            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
            if (!assetImporter) return true;

            //找到此类资源的配置节点
            XmlNode TypeNode = XmlHelper.GetXmlNodeByAttribute(RootNode, ImportDefine.Node, ImportDefine.PostfixName, postfix);
            if (TypeNode != null)
            {
                //配置属性列表
                List<XmlNode> PropNodeList = new List<XmlNode>();
                XmlHelper.GetXmlNodeList(TypeNode, ImportDefine.Prop, ref PropNodeList, true);
                //Debug.Log($"Node:{XmlHelper.GetXmlAttributeValue(TypeNode, ImportDefine.PName)}:属性Prop个数：{PropNodeList.Count}");
                if (PropNodeList.Count > 0)
                {
                    bool isAllPass = true;
                    foreach (XmlNode item in PropNodeList)
                    {
                        if (!CheckImporterValue(assetImporter, item)) //检查属性
                        {
                            isAllPass = false;

                            if (!isShowAllDifferent)
                                break;
                        }
                    }
                    return isAllPass;
                }
                return false;
            }

            return true;
        }


        /// <summary>
        /// 判断资源导入配置是否和配置表一致
        /// </summary>
        /// <param name="importer">AssetImporter</param>
        /// <param name="propNode">XmlNode</param>
        /// <returns>是否一致</returns>
        public static bool CheckImporterValue(AssetImporter importer, XmlNode propNode)
        {
            if (propNode.Attributes.Count <= 0)
            {
                Debug.LogWarning($"{propNode.ParentNode.Name}有未配置属性的{propNode.Name}");
                return false;
            }

            //配置表属性值
            string propName = XmlHelper.GetXmlAttributeValue(propNode, ImportDefine.PName);
            string propValue = XmlHelper.GetXmlAttributeValue(propNode, ImportDefine.PValue);
            string propType = XmlHelper.GetXmlAttributeValue(propNode, ImportDefine.PType);
            string enumName = null;

            //if ("quality" == propName)
            //{
            //    Debug.Log($"******调试属性[{propName}]专用位置******");
            //}

            //一系列判断后真实需要填入的属性值
            object realValue = null;

            //表示配置了附加信息
            if (propNode.HasChildNodes)
            {
                //先检查前置判断，前置判断不通过就跳过
                if (!AssetImportHelper.CheckPreConditions(importer, propNode, ref propValue)) return false;

                //再检查赋值判断
                AssetImportHelper.CheckAdditionConditions(importer, propNode, ref propValue);

                //获取枚举类地址
                XmlNode enumNode = propNode.SelectSingleNode(ImportDefine.EnumFile);
                if (enumNode != null) enumName = enumNode.InnerText;
            }

            //开始转换数据类型，设置新属性
            if (AssetImportHelper.TryParse(ref propValue, ref propType, out realValue, enumName))
            {
                //重定向节点列表
                XmlNodeList redirectNodeList = XmlHelper.GetXmlNodeList(propNode, ImportDefine.Redirect);

                //有需要重定向节点
                if (redirectNodeList.Count > 0)
                {
                    bool isAllPass = true;
                    foreach (XmlNode redNode in redirectNodeList)
                    {
                        //先设置重定向类的属性
                        object entityClass = AssetImportHelper.GetRedirectEntity(redNode, importer);
                        if (entityClass == null)
                        {
                            string redirName = XmlHelper.GetXmlAttributeValue(redNode, ImportDefine.PName);
                            string redirParam = XmlHelper.GetXmlAttributeValue(redNode, ImportDefine.Param);
                            Debug.LogError($"属性节点[{propName}]重定向类[{redirName} - {redirParam}]失败，已跳过!");
                            continue;
                        }

                        if (!IsValueMatch(entityClass, propName, realValue))
                        {
                            Debug.LogError($"资源属性设置有误，属性：{propName} >> 配置表值：{realValue}, file:{importer.assetPath}");
                            isAllPass = false;
                            break;
                        }
                    }
                    return isAllPass;
                }
                else
                {
                    //没有重定向
                    //直接判断属性值
                    bool bPass = IsValueMatch(importer, propName, realValue);
                    if (!bPass) Debug.LogError($"资源属性设置有误，属性：{propName} >> 配置表值：{realValue}, file:{importer.assetPath}");
                    return bPass;
                }
            }
            else
            {
                Debug.LogError($"数据类型转换失败，属性：{propName}，数据：{propValue}，转换类型：{propType}，枚举：{enumName}");
                return false;
            }
        }

        //属性值是否匹配给定值
        private static bool IsValueMatch(object entityClass, string propName, object cfgValue)
        {
            object curValue = AssetImportHelper.GetPropertyOrFieldValue(entityClass, propName);
            return curValue.Equals(cfgValue);
        }
    }
}
