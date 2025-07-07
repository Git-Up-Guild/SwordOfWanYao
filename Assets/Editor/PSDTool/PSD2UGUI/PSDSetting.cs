#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
#endif
#if !SLUA_STANDALONE
using UnityEngine;
using UnityEngine.UI;
using XGameEditor;
#endif

namespace subjectnerdagreement.psdexport
{
    [System.Serializable]
    public class ImportTextStyle
    {
        public string name;

        public bool enableOut;
        public float outX;
        public float outY;
        public Color outColor;
        public float outWidth;

        public bool enableShadow;
        public float shadowX;
        public float shadowY;
        public Color shadowColor;
    }

    /// <summary>
    /// 导入字体的映射关系
    /// </summary>
    [System.Serializable]
    public class ImportFontMap
    {
        [Header("字体说明")]
        public string desc;

        [Header("PSD字体名字")]
        public string sourceName;

        [Header("项目字体名字")]
        public string mapName;

        [Header("项目字体SDF名字")]
        public string mapSDFName;

        [Header("SDF字体风格配置")]
        public PSDTMPTextStyleSettings sdfStyleSettings;
    }

    [System.Serializable]
    public class ImportPathMap
    {
        [Header("资源路径命名")]
        public string matchName;
        [Header("对应文件夹名字")]
        public string DirectoryName;
    }

    [System.Serializable]
    public class ImportPropertyMap
    {
        [Header("属性说明")]
        public string desc;
        [Header("资源属性命名")]
        public string matchName;
        [Header("对应属性类型")]
        public EImportPropType propType;
    }

    [System.Serializable]
    public class ImportMarkMap
    {
        [Header("标记说明")]
        public string desc;
        [Header("资源后缀标记")]
        public string matchName;
        [Header("对应处理方式")]
        public EImportMarkType funcType;
    }

    [System.Serializable]
    public enum EImportPropType
    {
        None,
        Button,
        Slider,
        Image,
        Text,
        InputField,
    }

    public enum EImportMarkType
    {
        None = 0x0,

        [Header("九宫格")]
        Slice9Piece = 0x1,

        [Header("缩放")]
        Zoom = 0x2,

        [Header("透明度调整")]
        Lucency = 0x4,
    }

    public class ImportResInfo
    {
        //是否跳过
        public bool isSkip;

        //是否为dsd资源
        public bool isDsd;

        //是否有效
        public bool isValid = false;

        //资源真正名字
        public string sourceName;

        //是否为更新
        public bool isUpdate = false;

        //是否为公共资源
        public bool isCommRes;

        //导入的名称
        public string importName;

        //存放文件夹相对路径(例：Assets/UI/xxx.png)
        public string dirCommRelPath;

        //资源属性类型
        public EImportPropType propType = EImportPropType.None;

        //资源标记类型
        public EImportMarkType funcTypeFlag = EImportMarkType.None;

        //文本风格
        public ImportTextStyle textStyle = null;                  
    }

    public class PSDSetting : ScriptableObject
    {
        private const string DEFAULT_IMPORT_PATH = "G_Resources/Artist/UI/Temp";
        private readonly char spliteChar = '_';      //特效命名的分隔字符

        [Header("PSD资源默认文件夹")]
        public string PsdPath = "";

        public string DefaultImportPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_DefaultImportPath))
                    return DEFAULT_IMPORT_PATH;
                return m_DefaultImportPath;
            }
        }

        [Header("默认导入资源位置")]
        [SerializeField]
        protected string m_DefaultImportPath;

        [Header("字体存放目录")]
        public string fontDir;

        [Header("导入图片资源的根目录")]
        public string importBaseDir;

        [Header("导出公共资源存放目录")]
        public string exportCommResDir;

        [Header("可选文本风格列表")]
        public List<ImportTextStyle> txtStyles = new List<ImportTextStyle>();

        [Header("字体映射表")]
        public List<ImportFontMap> fontsMap = new List<ImportFontMap>();

        [Header("跳过标记")]
        public string skipFlag = "skip_";

        [Header("废弃资源标记")]
        public string dsdFlag = "dsd";

        [Header("资源更新标记")]
        public string updateFlag = "upd";

        [Header("资源路径(基于Common)")]
        public List<ImportPathMap> pathMap = new List<ImportPathMap>();

        [Header("资源属性")]
        public List<ImportPropertyMap> propertyMap = new List<ImportPropertyMap>();

        [Header("资源标记")]
        public List<ImportMarkMap> funcMarkMap = new List<ImportMarkMap>();

        private static PSDSetting _instance = null;
        public static PSDSetting Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<PSDSetting>("PSDSetting");

#if UNITY_EDITOR
                    if (_instance == null)
                    {
                        PSDSetting setting = PSDSetting.CreateInstance<PSDSetting>();
                        string path = XGameEditorUtilityEx.GetScriptDir("PSDSetting.cs");
                        if (!string.IsNullOrEmpty(path))
                        {
                            string dir = path + "/Resources";
                            dir = XGameEditorUtilityEx.AssetPathInApplication(dir);
                            XGameEditorUtilityEx.CreateDir(dir);
                            dir = XGameEditorUtilityEx.BaseOnAssetsPath(dir);
                            AssetDatabase.CreateAsset(setting, dir + "/PSDSetting.asset");
                            _instance = setting;
                        }
                    }
#endif
                }

                return _instance;
            }
        }

#if UNITY_EDITOR && !SLUA_STANDALONE
        [MenuItem("XGame/PSD工具/配置项")]
        public static void Open()
        {
            Selection.activeObject = Instance;
        }
#endif

        public ImportFontMap GetImportFontMapByMapSDFName(string fontName)
        {
            foreach(var f in fontsMap)
            {
                if(f.mapSDFName == fontName)
                {
                    return f;
                }
            }
            return null;
        }

        public ImportTextStyle GetTextStyle(string styleName)
        {
            foreach (var style in txtStyles)
            {
                if (style.name == styleName)
                {
                    return style;
                }
            }
            return null;
        }

        //是否为共资源
        public bool IsCommRes(string prefix)
        {
            foreach (var style in pathMap)
            {
                if (style.matchName == prefix)
                    return true;
            }
            return false;
        }

        //获取相对于Assets的文件夹保存路径
        public string GetCommRelSaveDirPath(string prefix)
        {
            foreach (var style in pathMap)
            {
                if (style.matchName == prefix)
                {
                    string assetPath = $"{importBaseDir}/Common/{style.DirectoryName}/";
                    if (!assetPath.StartsWith("Assets/")) assetPath = "Assets/" + assetPath;
                    return assetPath;
                }
            }
            return null;
        }

        public EImportPropType GetPropertyType(string szMatch)
        {
            foreach (var style in propertyMap)
            {
                if (style.matchName == szMatch)
                {
                    return style.propType;
                }
            }
            return EImportPropType.None;
        }


        public EImportMarkType GetFuncType(string postfix)
        {
            foreach (var style in funcMarkMap)
            {
                if (style.matchName == postfix)
                {
                    return style.funcType;
                }
            }
            return EImportMarkType.None;
        }

        private EImportMarkType GetSumMarkTypeFlag(string[] arrSplite, int index)
        {
            EImportMarkType nFlag = EImportMarkType.None;
            if (arrSplite != null && arrSplite.Length > 0)
            {
                for (int i = index; i < arrSplite.Length; i++)
                {
                    EImportMarkType funcType = GetFuncType(arrSplite[i]);
                    if (funcType != EImportMarkType.None)
                    {
                        nFlag |= funcType;
                    }
                }
            }
            return nFlag;
        }

        /// <summary>
        /// 解析dsd标记
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private bool ParseDsdFlag(ImportResInfo result, string flag)
        {
            if (flag.ToLower().StartsWith(dsdFlag.ToLower()))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 解析跳过标记
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private bool ParseSkipFlag(ImportResInfo result, string flag)
        {
            //跳过的不需要处理了
            if (flag.ToLower().StartsWith(skipFlag.ToLower()))
            {
                result.isSkip = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 解析公共资源标记
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private bool ParseCommonResFlag(ImportResInfo result, string flag)
        {
            //是否为共资源
            result.isCommRes = IsCommRes(flag);

            //解析保存路径，这个公共资源的路径
            if (result.isCommRes)
                result.dirCommRelPath = GetCommRelSaveDirPath(flag);

            return result.isCommRes;
        }

        /// <summary>
        /// 解析类型标标记，比如btn,slider,input等
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private bool ParsePropertyTypeFlag(ImportResInfo result, string flag)
        {
            EImportPropType propType = GetPropertyType(flag);
            if(propType != EImportPropType.None)
                result.propType = propType;
            return result.propType != EImportPropType.None;
        }

        /// <summary>
        /// 解析功能标记
        /// </summary>
        /// <param name="result"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        private bool ParseFuncTypeFlag(ImportResInfo result, string flag)
        {
            bool isFind = false;
            string[] arrFlags = flag.Split('|');
            for(var i = 0; i < arrFlags.Length; ++i)
            {
                EImportMarkType funcType = GetFuncType(arrFlags[i]);
                if (funcType != EImportMarkType.None)
                {
                    result.funcTypeFlag |= funcType;
                    isFind = true;
                }
            }
            return isFind;
        }

        /// <summary>
        /// 解析是否为更新模式
        /// </summary>
        /// <param name="result"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        private bool ParseUpdateFlag(ImportResInfo result, string flag)
        {
            bool isFind = false;
            if (flag == PSDSetting.Instance.updateFlag)
            {
                result.isUpdate = flag == PSDSetting.Instance.updateFlag;
                isFind = true;
            }
            return isFind;
        }

        /// <summary>
        /// 解析文本风格标记
        /// </summary>
        /// <param name="result"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        private bool ParseTextStyleFlag(ImportResInfo result, string flag)
        {
            bool isFind = false;
            var style = GetTextStyle(flag);
            if(style != null)
            {
                result.textStyle = style;
                isFind = true;
            }
            return isFind;
        }

        //检查是否有命名标记配置
        public ImportResInfo GetImportResInfo(string srcName, string extraInfo = null)
        {
            ImportResInfo result = new ImportResInfo();
            result.sourceName = srcName;

            //跳过的不需要处理了
            if (ParseSkipFlag(result, srcName))
                return result;

            //解析名称规则
            string[] arrSplite = srcName.Split(spliteChar);

            if (srcName == "ctb_shenjue_upd")
            {
                Debug.DebugBreak();
            }

            //过滤掉空的字符，对连续分割符进行兼容
            List<string> lsSplite = new List<string>();
            foreach(var s in arrSplite)
            {
                if(!string.IsNullOrEmpty(s))
                {
                    lsSplite.Add(s);
                }
            }

            int nFlagIndex = 0;

            //再次转换回来
            arrSplite = lsSplite.ToArray();

            if (arrSplite.Length > 1)
            {
                int nNameFlagEndIndex = arrSplite.Length - 1;

                //解析dsd标记
                if (ParseDsdFlag(result, arrSplite[nFlagIndex]))
                {
                    result.isDsd = true;
                    nFlagIndex++;
                }

                //解析公共资源
                if (nFlagIndex < arrSplite.Length && ParseCommonResFlag(result, arrSplite[nFlagIndex]))
                    nFlagIndex++;

                //解析类型（按钮，图片等）
                if(nFlagIndex < arrSplite.Length && ParsePropertyTypeFlag(result, arrSplite[nFlagIndex]))
                    nFlagIndex++;

                //名称的起始索引
                int nNameFlagBeginIndex = nFlagIndex;

                //从末尾开始判断
                for (var i = arrSplite.Length - 1; i >= nFlagIndex; i--)
                {
                    string flag = arrSplite[i];

                    //如果是特殊标记，则名称结束索引就需要减少1
                    if (ParseFuncTypeFlag(result, flag) || ParseTextStyleFlag(result, flag) || ParseUpdateFlag(result, flag))
                    {
                        nNameFlagEndIndex--;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                //是否有auto标记
                bool hasAutoFlag = false;

                //导入名称
                string importName = string.Empty;

                //开始组装导入名称
                for (var i = nNameFlagBeginIndex; i <= nNameFlagEndIndex; i++)
                {
                    string flag = arrSplite[i];
                    if(i == nNameFlagBeginIndex)
                    {
                        if (flag.ToLower() == "auto")
                        {
                            hasAutoFlag = true;
                            continue;
                        }
                    }

                    //拼接名称
                    if (string.IsNullOrEmpty(importName))
                        importName = flag;
                    else
                        importName += "_" + flag;
                }

                //如果解析不出来名称，则使用下划线代替
                if(string.IsNullOrEmpty(importName))
                {
                    importName = "_";
                }
                else
                {
                    //dsd对象
                    if(result.isDsd)
                    {
                        importName = $"dsd_{importName}";
                    }
                    else
                    {
                        //auto对象
                        if(hasAutoFlag)
                            importName = $"auto_{importName}";
                    }
                }

                result.importName = importName;
            }
            else
            {
                result.importName = srcName;
            }

            if (string.IsNullOrEmpty(result.importName))
            {
                result.isValid = false;
                Debug.LogError($"构建导入信息失败，层：[{srcName}]，附加信息：{extraInfo}");
            }

            return result;
        }
    }
}
