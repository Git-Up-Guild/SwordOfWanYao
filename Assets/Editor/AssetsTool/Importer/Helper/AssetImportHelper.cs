using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace XGameEditor.AssetImportTool
{
    //资源导入工具帮助类
    public static class AssetImportHelper
    {
        //通过反射设置属性或者字段
        public static bool SetPropertyOrField(object entityClass, string propName, object newPropValue)
        {
            bool isSuc = SetProperty(entityClass, propName, newPropValue);
            if (!isSuc)
                isSuc = SetField(entityClass, propName, newPropValue);
            return isSuc;
        }

        /// <summary>
        /// 通过反射设置属性
        /// </summary>
        /// <param name="entityClass">实体类</param>
        /// <param name="propName">属性名</param>
        /// <param name="value">新属性值(类型要正确，不然Error)</param>
        /// <returns></returns>
        public static bool SetProperty(object entityClass, string propName, object newPropValue)
        {
            PropertyInfo info = GetProperty(entityClass, propName);
            if (info != null)
            {
                info.SetValue(entityClass, newPropValue);
                return true;
            }
            return false;
        }

        //通过反射设置字段
        public static bool SetField(object entityClass, string fieldName, object newFieldValue)
        {
            FieldInfo info = GetFieldInfo(entityClass, fieldName);
            if (info != null)
            {
                info.SetValue(entityClass, newFieldValue);
                return true;
            }
            return false;
        }

        //通过反射获取属性值或者字段值
        public static object GetPropertyOrFieldValue(object entityClass, string propName)
        {
            object value = GetPropertyValue(entityClass, propName);
            if (value == null)
                value = GetFieldValue(entityClass, propName);
            return value;
        }

        //通过反射获取属性值
        public static object GetPropertyValue(object entityClass, string propName)
        {
            PropertyInfo info = GetProperty(entityClass, propName);
            if (info != null)
                return info.GetValue(entityClass, null);
            else
                return null;
        }

        //通过反射获取字段值
        public static object GetFieldValue(object entityClass, string fieldName)
        {
            FieldInfo info = GetFieldInfo(entityClass, fieldName);
            if (info != null)
                return info.GetValue(entityClass);
            else
                return null;
        }

        //获取属性
        public static PropertyInfo GetProperty(object entityClass, string propName)
        {
            if (entityClass != null)
            {
                Type mType = entityClass.GetType();
                return mType.GetProperty(propName);
            }
            return null;
        }

        //获取字段
        public static FieldInfo GetFieldInfo(object entityClass, string fieldName)
        {
            if (entityClass != null)
            {
                Type mType = entityClass.GetType();
                return mType.GetField(fieldName);
            }
            return null;
        }

        //通过反射调用公有静态方法(无重载)
        public static object InvokePublicStaticMethod(Type type, string methodName, params object[] args)
        {
            object result = type.InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, args);
            return result;
        }

        //通过反射调用非静态方法
        public static object InvokeNonStaticMethod(object entityClass, string methodName, params object[] args)
        {
            Type type = entityClass.GetType();
            //设置参数类型，避免有重载时报错
            int length = args.Length;
            Type[] paramTypes = new Type[length];
            for (int i = 0; i < length; i++)
            {
                paramTypes[i] = args[i].GetType();
            }
            MethodInfo method = type.GetMethod(methodName, paramTypes);
            if (method != null)
            {
                return method.Invoke(entityClass, args);
            }
            return null;
        }

        /// <summary>
        /// 通过枚举类名和枚举值获取到相应的枚举类型
        /// </summary>
        /// <param name="enumFullName">枚举类完整路径（命名空间.类名,程序集名，例：UnityEditor.ModelImporterIndexFormat,UnityEditor）</param>
        /// <param name="enumValue">枚举值字符串</param>
        /// <param name="enumType">推出的枚举类型</param>
        /// <returns>是否获取成功</returns>
        public static bool TryGetEnumType(string enumFullName, string enumValue, out object enumType)
        {
            Type tType = Type.GetType(enumFullName);
            if (tType == null)
            {
                enumType = null;
                return false;
            }
            else
            {
                enumType = Enum.Parse(tType, enumValue);
                return true;
            }
        }

        //更新资源导入配置,通过xml配置信息
        public static bool UpdateAssetImporterByXmlNode(AssetImporter importer, XmlNode RootNode)
        {
            if (importer == null || RootNode == null) return false;

            //获取资源后缀
            string postfix = Path.GetExtension(importer.assetPath);
            if (string.IsNullOrEmpty(postfix)) return false;
            try
            {
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
                        foreach (XmlNode item in PropNodeList)
                        {
                            UpdateImporter(importer, item); //更新属性
                        }
                        return true;
                    }
                }
                else
                {
                    Debug.Log($"{ImportDefine.ImportSettingXmlPath}:找不到属性：{ImportDefine.PostfixName} - {postfix}的节点,跳过导入配置");
                }
            }
            catch (XmlException e)
            {
                Debug.LogError($"XML解析有误：{e.Message} \n堆栈：{e.StackTrace} \n错误源：{e.Source}");
            }
            catch (Exception e)
            {
                Debug.LogError($"资源导入配置设置有误：{e.Message} \n堆栈：{e.StackTrace} \n错误源：{e.Source}");
            }
            finally
            {
                AssetDatabase.ImportAsset(importer.assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return false;
        }

        //更新资源导入配置
        public static void UpdateImporter(AssetImporter importer, XmlNode propNode)
        {
            if (propNode.Attributes.Count <= 0)
            {
                Debug.LogWarning($"{propNode.ParentNode.Name}有未配置属性的{propNode.Name}");
                return;
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
                if (!CheckPreConditions(importer, propNode, ref propValue)) return;

                //再检查赋值判断
                CheckAdditionConditions(importer, propNode, ref propValue);

                //获取枚举类地址
                XmlNode enumNode = propNode.SelectSingleNode(ImportDefine.EnumFile);
                if (enumNode != null) enumName = enumNode.InnerText;
            }

            //开始转换数据类型，设置新属性
            if (TryParse(ref propValue, ref propType, out realValue, enumName))
            {
                //重定向节点列表
                XmlNodeList redirectNodeList = XmlHelper.GetXmlNodeList(propNode, ImportDefine.Redirect);

                //有需要重定向节点
                if (redirectNodeList.Count > 0)
                {
                    foreach (XmlNode redNode in redirectNodeList)
                    {
                        //先设置重定向类的属性
                        object entityClass = GetRedirectEntity(redNode, importer);
                        if (entityClass == null)
                        {
                            string redirName = XmlHelper.GetXmlAttributeValue(redNode, ImportDefine.PName);
                            string redirParam = XmlHelper.GetXmlAttributeValue(redNode, ImportDefine.Param);
                            Debug.LogError($"属性节点[{propName}]重定向类[{redirName} - {redirParam}]失败，已跳过!");
                            continue;
                        }

                        if (!SetPropertyOrField(entityClass, propName, realValue))
                        {
                            Debug.LogError($"{importer.name}-属性设置失败：{propName} - {realValue}");
                        }

                        //在判断是否要将重定向类赋值到原类进行覆盖
                        XmlNodeList overrideNodeList = XmlHelper.GetXmlNodeList(redNode, ImportDefine.Override);
                        if (overrideNodeList.Count > 0)
                        {
                            foreach (XmlNode overNode in overrideNodeList)
                            {
                                SetOverrideRedirect(overNode, importer, entityClass);
                            }
                        }

                        //if ("quality" == propName)
                        //{
                        //    entityClass = GetRedirectEntity(redNode, importer);
                        //}
                    }
                }
                else
                {
                    //没有重定向
                    //直接设置新的属性值
                    if (!SetPropertyOrField(importer, propName, realValue))
                    {
                        Debug.LogError($"{importer.assetPath}-属性设置失败：{propName} - {realValue}");
                    }
                }
            }
            else
            {
                Debug.LogError($"数据类型转换失败，属性：{propName}，数据：{propValue}，转换类型：{propType}，枚举：{enumName}");
            }
        }

        //检测前置条件
        public static bool CheckPreConditions(AssetImporter importer, XmlNode propNode, ref string propValue)
        {
            if (propNode.HasChildNodes)
            {
                XmlNode preCheckNode = XmlHelper.GetXmlNode(propNode, ImportDefine.PreCheck);
                if (preCheckNode != null)
                {
                    if (GetCheckResult(preCheckNode, importer))
                    {
                        //满足条件，那么设置新的value
                        propValue = XmlHelper.GetXmlAttributeValue(preCheckNode, ImportDefine.Return);
                    }
                }
            }
            return true;
        }

        //检测附加信息
        public static void CheckAdditionConditions(AssetImporter importer, XmlNode propNode, ref string propValue)
        {
            if (propNode.HasChildNodes)
            {
                XmlNodeList checkNodeList = XmlHelper.GetXmlNodeList(propNode, ImportDefine.Check);
                if (checkNodeList != null)
                {
                    foreach (XmlNode checkNode in checkNodeList)
                    {
                        if (GetCheckResult(checkNode, importer))
                        {
                            //满足条件，那么设置新的value
                            propValue = XmlHelper.GetXmlAttributeValue(checkNode, ImportDefine.Return);
                            break;
                        }
                    }
                }
            }
        }

        //获取Check节点的判断结果
        private static bool GetCheckResult(XmlNode checkNode, AssetImporter importer)
        {
            string logic = XmlHelper.GetXmlAttributeValue(checkNode, ImportDefine.Logic);

            //检测是否通过flag
            bool bChecSuc = false;
            if (EqualIgnoreCase(logic, ImportDefine.LOr))
            {
                bChecSuc = false;
            }
            else if (EqualIgnoreCase(logic, ImportDefine.LAnd))
            {
                bChecSuc = true;
            }

            //开始正式检测Condition
            XmlNodeList conditionNodeList = XmlHelper.GetXmlNodeList(checkNode, ImportDefine.Condition);
            if (conditionNodeList.Count > 0)
            {
                foreach (XmlNode conNode in conditionNodeList)
                {
                    //这里要反射调用静态判断方法 *************************
                    bool bRes = IsConditionMet(conNode, importer);
                    if (IsNeedBreak(logic, bRes))
                    {
                        bChecSuc = bRes;
                        break;
                    }
                }
            }

            //看是否还需要继续往下判断
            if (IsNeedBreak(logic, bChecSuc))
            {
                return bChecSuc;
            }

            //继续检测是否还有Check/PreCheck
            XmlNodeList checkNodeList = XmlHelper.GetXmlNodeList(checkNode, checkNode.Name);
            if (checkNodeList.Count > 0)
            {
                foreach (XmlNode cheNode in checkNodeList)
                {
                    bool bRes = GetCheckResult(cheNode, importer);
                    if (IsNeedBreak(logic, bRes))
                    {
                        bChecSuc = bRes;
                        break;
                    }
                }
            }

            return bChecSuc;
        }

        //是否条件满足
        private static bool IsConditionMet(XmlNode conNode, AssetImporter importer)
        {
            //处理参数
            List<object> paramList = new List<object>();
            paramList.Add(importer);

            //解析xml参数配置
            string paramSrt = XmlHelper.GetXmlAttributeValue(conNode, ImportDefine.Param);
            if (!string.IsNullOrEmpty(paramSrt))
            {
                string[] xmlParams = paramSrt.Split(ImportDefine.SpliteChar);
                foreach (var item in xmlParams)
                {
                    paramList.Add(item);
                }
            }

            //反射法调用静态方法
            object result = InvokePublicStaticMethod(typeof(RpcMethodClass), conNode.InnerText, paramList.ToArray());
            return JudgeConditionResult(conNode, result);
        }

        //判断条件是否满足
        private static bool JudgeConditionResult(XmlNode conNode, object returnValue)
        {
            string pName = XmlHelper.GetXmlAttributeValue(conNode, ImportDefine.PName);
            string pValue = XmlHelper.GetXmlAttributeValue(conNode, ImportDefine.PValue);

            bool res = false;
            switch (pName)
            {
                case ImportDefine.Contain:
                    res = IsContain(returnValue.ToString(), pValue.Replace('\\', '/'));
                    break;
                case ImportDefine.Equal:
                    res = IsEqual(returnValue.ToString(), pValue);
                    break;
                case ImportDefine.Less:
                    res = IsLess(returnValue, pValue);
                    break;
                case ImportDefine.Greater:
                    res = IsGreater(returnValue, pValue);
                    break;
                default:
                    break;
            }
            return res;
        }

        //获取重定向实体
        public static object GetRedirectEntity(XmlNode redirectNode, AssetImporter importer)
        {
            string methodName = XmlHelper.GetXmlAttributeValue(redirectNode, ImportDefine.PName);
            string paramName = XmlHelper.GetXmlAttributeValue(redirectNode, ImportDefine.Param);
            string[] paramArr = paramName.Split(ImportDefine.SpliteChar);
            object redirectEntity = InvokeNonStaticMethod(importer, methodName, paramArr);
            return redirectEntity;
        }

        //覆盖重定向方法
        private static void SetOverrideRedirect(XmlNode overrideNode, AssetImporter importer, object redEntity)
        {
            string methodName = XmlHelper.GetXmlAttributeValue(overrideNode, ImportDefine.PName);
            string paramName = XmlHelper.GetXmlAttributeValue(overrideNode, ImportDefine.Param);
            string[] paramArr = paramName.Split(ImportDefine.SpliteChar);

            int length = paramArr.Length;
            object[] realParams = new object[length];
            for (int i = 0; i < length; i++)
            {
                if (EqualIgnoreCase(paramArr[i], ImportDefine.This))
                {
                    realParams[i] = redEntity;
                }
                else
                {
                    realParams[i] = paramArr[i];
                }
            }
            //调用覆盖方法
            InvokeNonStaticMethod(importer, methodName, realParams);
        }


        #region 数据处理方法
        //是否需要break或return了
        private static bool IsNeedBreak(string logic, bool bResult)
        {
            if (EqualIgnoreCase(logic, ImportDefine.LOr) && bResult)
            {
                return true;
            }
            else if (EqualIgnoreCase(logic, ImportDefine.LAnd) && !bResult)
            {
                return true;
            }

            return false;
        }

        private static bool EqualIgnoreCase(string a, string b)
        {
            return a.Equals(b, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsContain(string result, string value)
        {
            return result.IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool IsEqual(string result, string value)
        {
            return EqualIgnoreCase(result, value);
        }

        private static bool IsLess(object result, string value)
        {
            Type type = result.GetType();
            if (type == typeof(int))
            {
                int iValue;
                if (int.TryParse(value, out iValue))
                {
                    return (int)result < iValue;
                }
                else
                {
                    Debug.LogError($"数据[{value}]转换为Int失败");
                }
            }
            else if (type == typeof(float))
            {
                float fValue;
                if (float.TryParse(value, out fValue))
                {
                    return (float)result < fValue;
                }
                else
                {
                    Debug.LogError($"数据[{value}]转换为Float失败");
                }
            }

            return false;
        }

        private static bool IsGreater(object result, string value)
        {
            Type type = result.GetType();
            if (type == typeof(int))
            {
                int iValue;
                if (int.TryParse(value, out iValue))
                {
                    return (int)result > iValue;
                }
                else
                {
                    Debug.LogError($"数据[{value}]转换为Int失败");
                }
            }
            else if (type == typeof(float))
            {
                float fValue;
                if (float.TryParse(value, out fValue))
                {
                    return (float)result > fValue;
                }
                else
                {
                    Debug.LogError($"数据[{value}]转换为Float失败");
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试按给定类型转换数据
        /// </summary>
        /// <param name="srcValue">源数据</param>
        /// <param name="parseType">转换类型</param>
        /// <param name="destValue">目标数据</param>
        /// <param name="enumName">枚举类名（非枚举转换缺省）</param>
        /// <returns></returns>
        public static bool TryParse(ref string srcValue, ref string parseType, out object destValue, string enumName = null)
        {
            bool isSuc = false;
            destValue = null;
            //按类型转换
            switch (parseType)
            {
                case ImportDefine.Int:
                    int iRes;
                    if (int.TryParse(srcValue, out iRes))
                    {
                        isSuc = true;
                        destValue = iRes;
                    }
                    break;
                case ImportDefine.Float:
                    float fRes;
                    if (float.TryParse(srcValue, out fRes))
                    {
                        isSuc = true;
                        destValue = fRes;
                    }
                    break;
                case ImportDefine.Bool:
                    bool bRes;
                    if (bool.TryParse(srcValue, out bRes))
                    {
                        isSuc = true;
                        destValue = bRes;
                    }
                    break;
                case ImportDefine.Enum:
                    if (!string.IsNullOrEmpty(enumName))
                    {
                        isSuc = TryGetEnumType(enumName, srcValue, out destValue);
                    }
                    break;
                default:
                    break;
            }

            return isSuc;
        }

        #endregion

        #region 设置ab路径
        //ab路径设置
        public static void SetAssetBundleByXmlNode(AssetImporter importer, XmlNode RootNode)
        {
            //先检测后缀
            string assetPath = importer.assetPath.ToLower();
            string postfix = Path.GetExtension(assetPath);
            string postfixXml = XmlHelper.GetXmlAttributeValue(RootNode, AssetBundleDefine.Postfix).ToLower();
            if (!postfixXml.Contains(postfix)) return;

            //获取配置类型
            XmlNode matchNode = null;
            List<XmlNode> DirNodeList = new List<XmlNode>();
            XmlHelper.GetXmlNodeList(RootNode, AssetBundleDefine.Dir, ref DirNodeList, true);
            int matchLen = 0;   //匹配的长度
            string tempPath = null;
            if (DirNodeList.Count > 0)
            {
                foreach (XmlNode DirNode in DirNodeList)
                {
                    tempPath = XmlHelper.GetXmlAttributeValue(DirNode, AssetBundleDefine.dPath).ToLower();
                    if (!(string.IsNullOrEmpty(tempPath)) && assetPath.Contains(tempPath) && tempPath.Length > matchLen)
                    {
                        matchNode = DirNode;
                        matchLen = tempPath.Length;
                    }
                }
            }

            //表示有匹配的ab设置节点
            if (matchNode != null)
            {
                string assetBundleName = null;
                EAssetBundleSetting bundleSetting = (EAssetBundleSetting)Enum.Parse(typeof(EAssetBundleSetting), matchNode.InnerText);
                switch (bundleSetting)
                {
                    case EAssetBundleSetting.SetByPath:
                        assetBundleName = assetPath.Substring(assetPath.IndexOf("/") + 1);
                        break;
                    case EAssetBundleSetting.SetByDir:
                        var dirName = Path.GetDirectoryName(assetPath);
                        assetBundleName = dirName.Substring(dirName.IndexOf("/") + 1);
                        break;
                    default:
                        break;
                }

                //设置ab路径
                if (!string.IsNullOrEmpty(assetBundleName))
                    importer.assetBundleName = assetBundleName;
                else
                    Debug.LogError("ab路径有误，设置失败：" + assetPath);
            }
        }
        #endregion
    }
}
