using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XGameEditor.AssetImportTool
{
    //资源自动导入设置
    public static class ImportDefine
    {
        private const string xmlRelativePath = @"XGameEditor\Editor\AssetsTool\AssetImporter\AssetImportSetting.xml";
        public static string ImportSettingXmlPath { get { return Path.Combine(Application.dataPath, xmlRelativePath); } }
        public const string Root = "Root";
        public const string Node = "Node";
        public const string PostfixName = "postfix";
        public const char SpliteChar = ';';

        public const string Prop = "Prop";
        public const string PName = "name";
        public const string PValue = "value";
        public const string PType = "type";
        public const string EnumFile = "EnumFile";

        public const string Redirect = "Redirect";
        public const string Override = "Override";
        public const string PreCheck = "PreCheck";
        public const string Check = "Check";
        public const string Return = "return";
        public const string Condition = "Condition";
        public const string Logic = "logic";
        public const string LAnd = "and";
        public const string LOr = "or";

        public const string Int = "int";
        public const string Bool = "bool";
        public const string Enum = "enum";
        public const string Float = "float";

        public const string Param = "param";
        public const string Equal = "equal";
        public const string Less = "less";
        public const string Greater = "greater";
        public const string Contain = "contain";
        public const string This = "this";
    }

    //资源ab路径设置
    public static class AssetBundleDefine
    {
        private const string xmlRelativePath = @"XGameEditor\Editor\AssetsTool\AssetImporter\AssetBundleSetting.xml";
        public static string AssetBundleSettingXmlPath { get { return Path.Combine(Application.dataPath, xmlRelativePath); } }
        public const string Root = "Root";
        public const string Node = "Node";
        public const string Postfix = "postfix";
        public const string Dir = "Dir";
        public const string dPath = "path";
    }


    //资源路径设置方式枚举
    public enum EAssetBundleSetting
    {
        SetByPath,  //按自己路径设置ab
        SetByDir    //按文件夹设置ab
    }
}
