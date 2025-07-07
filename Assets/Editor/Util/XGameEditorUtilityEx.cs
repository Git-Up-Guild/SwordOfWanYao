/*****************************************************
** 文 件 名：Q1EditorUtlity
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/6/19 17:04:20
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;

#endif
using UnityEngine;
using XGame.AssetScript.Util;

namespace XGameEditor
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public static class XGameEditorUtilityEx
    {
        /// <summary>
        /// 克隆辅助类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class SerializableObjectClone<T> : ScriptableObject
        {
            public T obj;

            public static T Clone(T obj)
            {
                SerializableObjectClone<T> temp1 = CreateInstance<SerializableObjectClone<T>>();
                temp1.obj = obj;
                SerializableObjectClone<T> temp2 = Instantiate(temp1);
                T obj2 = temp2.obj;
                DestroyImmediate(temp1);
                DestroyImmediate(temp2);
                return obj2;
            }
        }

        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T CloneSerializableObject<T>(T obj)
        {
            return SerializableObjectClone<T>.Clone(obj);
        }

        /// <summary>
        /// 获取编辑器下的配置资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ScriptableObject GetEditorScriptableObject(Type type, bool autoCreate = true, string path = null)
        {
#if UNITY_EDITOR
            //没有指定路径，就取脚本路径进行创建
            if (string.IsNullOrEmpty(path))
            {
                path = GetScriptDir(type.Name + ".cs");
            }

            //检查路径
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("只有编辑器脚本才支持此接口！！");
                return null;
            }

            //转换成对应的路径
            path = path + "/" + type.Name + ".asset";

            //尝试加载
            ScriptableObject t = AssetDatabase.LoadAssetAtPath(path, type) as ScriptableObject;

            //加载不到，就尝试创建
            if (t == null)
            {
                if (autoCreate)
                {
                    t = ScriptableObject.CreateInstance(type);
                    AssetDatabase.CreateAsset(t, path);
                }
            }

            return t;
#else
            Debug.LogError("该函数只能在编辑器环境下使用！！");
            return null;
#endif
        }

        /// <summary>
        /// 获取编辑器下的配置资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetEditorScriptableObject<T>(bool autoCreate = true, string path = null) where T: ScriptableObject
        {
            return GetEditorScriptableObject(typeof(T), autoCreate, path) as T;
        }

        /// <summary>
        /// 获取自定义attribute 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumObj"></param>
        /// <returns></returns>
        public static T GetEnumAttribute<T>(Enum enumValue) where T : Attribute
        {
            string value = enumValue.ToString();

            FieldInfo field = enumValue.GetType().GetField(value);
            if (field == null)
            {
                return null;
            }

            string[] arrNames = enumValue.GetType().GetEnumNames();
            T attr = field.GetCustomAttribute<T>(false);

            return attr;
        }

        /// <summary>
        /// 显示某个对象
        /// </summary>
        /// <param name="UnityObj"></param>
        public static void PingObject(UnityEngine.Object obj)
        {
           // XGameEditorUtility.PingObject(obj);
        }

        /// <summary>
        /// 获取相对于Assets的路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RelativeToAssetsPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("错误的路径！");
                return path;
            }

            path = path.Replace("\\", "/");

            if (path.StartsWith(Application.dataPath))
            {
                return path.Substring(Application.dataPath.Length);
            }
            if (path.StartsWith("Assets"))
            {
                return path.Substring("Assets/".Length);
            }
            if (path.StartsWith("assets"))
            {
                return path.Substring("assets/".Length);
            }
            return path;
        }

        /// <summary>
        /// 绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AssetPathInApplication(string path)
        {
            return "";
           // return XGameEditorUtility.ConvertToApplicationPath(path);
        }

        /// <summary>
        /// Data路径的父目录
        /// </summary>
        /// <returns></returns>
        public static string ApplicationDataPathParent()
        {
            return Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
        }

        /// <summary>
        /// 获取指定目录下的所有文件
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="includeSurdir"></param>
        /// <param name="lsPath"></param>
        public static void GetFilesPath(string dir, bool includeSurdir, List<string> lsPath, string[] fileExts = null)
        {
            //XGameEditorUtility.GetFilesPath(dir, includeSurdir, lsPath, fileExts);
        }

        /// <summary>
        /// 是否为Unity中的路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsUnityPath(string path)
        {
            return true;
           // return XGameEditorUtility.IsUnityPath(path);
        }

        /// <summary>
        /// 基于Assets的路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string BaseOnAssetsPath(string path)
        {
            return "";
           // return XGameEditorUtility.ConvertToBaseOnAssetsPath(path);
        }

        /// <summary>
        /// 获取父路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public string GetParentDir(string path)
        {
            return "";
           // return XGameEditorUtility.GetParentDir(path);
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="dir">要删除的目录</param>
        public static void DeleteDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                string[] fileSystemEntries = Directory.GetFileSystemEntries(dir);
                for (int i = 0; i < fileSystemEntries.Length; i++)
                {
                    string text = fileSystemEntries[i];
                    if (File.Exists(text))
                    {
                        File.Delete(text);
                    }
                    else
                    {
                        DeleteDir(text);
                    }
                }
                Directory.Delete(dir);
            }
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="dir"></param>
        public static void CreateDir(string dir)
        {
            //如果第二个字符是":"则是绝对路径
            if(dir.IndexOf(":") != 1)
            {
                dir = AssetPathInApplication(dir);
            }

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <param name="overwrite"></param>
        public static void CopyDirIntoDestDir(string sourceDir, string destDir, bool overwrite, string[] excludes, Action<int, int, string> callback = null)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            bool bExclude = false;
            string[] files = Directory.GetFiles(sourceDir);
            int iCur = 0;
            int iTotal = files.Length;
            foreach (var file in files)
            {
                bExclude = false;

                foreach (var ex in excludes)
                {
                    if (file.EndsWith(ex))
                    {
                        bExclude = true;
                        break;
                    }
                }

                if (!bExclude)
                    File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite);

                callback?.Invoke(iCur++, iTotal, file);

            }

            foreach (var d in Directory.GetDirectories(sourceDir))
            {
                CopyDirIntoDestDir(d, Path.Combine(destDir, Path.GetFileName(d)), overwrite, excludes, callback);
            }
        }

 
        /// <summary>
        /// 获取对象的层次路径
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        static public string GetHierarchyPath(Transform trans)
        {
            string path = trans.name;
            while (trans.parent != null)
            {
                path = trans.parent.name + "/" + path;
                trans = trans.parent;
            }

            return path;
        }

        /// <summary>
        /// 查找指定类型的所有子对象
        /// </summary>
        /// <param name="lsFindTypes"></param>
        public static void FindAllEnumTypes(List<Type> lsFindTypes)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var ass in assemblies)
            {
                Type[] allTypes = ass.GetTypes();
                foreach (var type in allTypes)
                {
                    if (type.IsEnum)
                    {
                        lsFindTypes.Add(type);
                    }
                }
            }
        }

        /// <summary>
        /// 获取所有具有某个特性的类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void FindAllTypesWithAttribute<T>(List<Type> lsRet, bool bEnumOnly = true) where T:Attribute
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var ass in assemblies)
            {
                Type[] allTypes = ass.GetTypes();
                foreach (var type in allTypes)
                {
                    if(type.GetCustomAttribute<T>() != null)
                    {
                        if(bEnumOnly)
                        {
                            if(type.IsEnum)
                            {
                                lsRet.Add(type);
                            }
                        }
                        else
                        {
                            lsRet.Add(type);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判断是否实现了某个泛型接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static bool IsImplementedRawGenericInterface(Type type, Type generic)
        {
            // 遍历类型实现的所有接口，判断是否存在某个接口是泛型，且是参数中指定的原始泛型的实例。
            return type.GetInterfaces().Any(x => generic == (x.IsGenericType ? x.GetGenericTypeDefinition() : x));
        }

        /// <summary>
        /// 判断是否为某个泛型的子类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static bool IsSubClassOfRawGeneric(Type type, Type generic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (generic == null) throw new ArgumentNullException(nameof(generic));

            while (type != null && type != typeof(object))
            {
                bool isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }

            return false;

            bool IsTheRawGenericType(Type test)
                => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }

        /// <summary>
        /// 判断指定的类型 <paramref name="type"/> 是否是指定泛型类型的子类型，或实现了指定泛型接口。
        /// </summary>
        /// <param name="type">需要测试的类型。</param>
        /// <param name="generic">泛型接口类型，传入 typeof(IXxx&lt;&gt;)</param>
        /// <returns>如果是泛型接口的子类型，则返回 true，否则返回 false。</returns>
        public static bool IsImplementedRawGeneric(Type type, Type generic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (generic == null) throw new ArgumentNullException(nameof(generic));

            // 测试接口。
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
            if (isTheRawGenericType) return true;

            // 测试类型。
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }

            // 没有找到任何匹配的接口或类型。
            return false;

            // 测试某个类型是否是指定的原始接口。
            bool IsTheRawGenericType(Type test)
                => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }

        /// <summary>
        /// 查找所有的继承了某个泛型的子类
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="lsFindTypes"></param>
        public static void FindAllRawGenericSubClassTypes(Type baseType, List<Type> lsFindTypes)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var ass in assemblies)
            {
                Type[] allTypes = ass.GetTypes();
                foreach (var type in allTypes)
                {
                    if(!type.IsGenericType && IsImplementedRawGeneric(type, baseType))
                    {
                        lsFindTypes.Add(type);
                    }
                }
            }
        }

        /// <summary>
        /// 查找所有接口的实现类
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="lsFindTypes"></param>
        public static void FindAllImplementsOfType(Type baseType, List<Type> lsFindTypes, List<string> lsNames = null)
        {
            XGameEditorUtility.FindAllImplementsOfType(baseType, lsFindTypes, lsNames);
        }

        /// <summary>
        /// 查找指定类型的所有子对象
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="lsFindTypes"></param>
        public static void FindAllSubClassTyps(Type baseType, List<Type> lsFindTypes, List<string> lsNames = null)
        {
            XGameEditorUtility.FindAllSubClassTyps(baseType, lsFindTypes, lsNames);
        }

        /// <summary>
        /// 获取当前选择对象的路径（包括目录等）
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentSelectPath()
        {
            return XGameEditorUtility.GetCurrentSelectPath();
        }

        /// <summary>
        /// 获取脚本所在目录
        /// </summary>
        /// <returns></returns>
        public static string GetCSharpScriptDir(Type type)
        {
            return XGameEditorUtility.GetCSharpScriptDir(type);
        }

        /// <summary>
        /// 获取脚本所在目录
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public static string GetCSharpScriptDir<T>()
        {
            return XGameEditorUtility.GetCSharpScriptDir(typeof(T));
        }

        /// <summary>
        /// 获取脚本所在目录
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public static string GetScriptDir(string scriptName)
        {
           return XGameEditorUtility.GetScriptDir(scriptName);
        }

        /// <summary>
        /// 获取脚本的路径
        /// </summary>
        /// <param name="_scriptName"></param>
        /// <returns></returns>
        public static string GetScriptPath(string scriptName)
        {
            return XGameEditorUtility.GetScriptPath(scriptName);
        }

        /// <summary>
        /// 创建脚本资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static public T CreateScriptableObject<T>(string saveDir = null) where T : ScriptableObject
        {
            T scriptableObject = ScriptableObject.CreateInstance<T>();
#if UNITY_EDITOR
            string path = null;
            if (string.IsNullOrEmpty(saveDir))
                path = EditorUtility.SaveFilePanelInProject("保存资源", typeof(T).Name, "asset", "选择要保存的文件目录！");
            else
                path = saveDir + "/" + typeof(T).Name + ".asset";
            AssetDatabase.CreateAsset(scriptableObject, path);
            EditorGUIUtility.PingObject(scriptableObject);
#endif
            return scriptableObject;

        }

        /// <summary>
        /// 绝对路径转相对路径
        /// </summary>
        /// <param name="strBasePath">基本路径</param>
        /// <param name="strFullPath">绝对路径</param>
        /// <returns>strFullPath相对于strBasePath的相对路径</returns>
        public static string GetRelativePath(string strBasePath, string strFullPath)
        {
            if (strBasePath == null)
                throw new ArgumentNullException("strBasePath");

            if (strFullPath == null)
                throw new ArgumentNullException("strFullPath");

            strBasePath = Path.GetFullPath(strBasePath);
            strFullPath = Path.GetFullPath(strFullPath);

            var DirectoryPos = new int[strBasePath.Length];
            int nPosCount = 0;

            DirectoryPos[nPosCount] = -1;
            ++nPosCount;

            int nDirectoryPos = 0;
            while (true)
            {
                nDirectoryPos = strBasePath.IndexOf('\\', nDirectoryPos);
                if (nDirectoryPos == -1)
                    break;

                DirectoryPos[nPosCount] = nDirectoryPos;
                ++nPosCount;
                ++nDirectoryPos;
            }

            if (!strBasePath.EndsWith("\\"))
            {
                DirectoryPos[nPosCount] = strBasePath.Length;
                ++nPosCount;
            }

            int nCommon = -1;
            for (int i = 1; i < nPosCount; ++i)
            {
                int nStart = DirectoryPos[i - 1] + 1;
                int nLength = DirectoryPos[i] - nStart;

                if (string.Compare(strBasePath, nStart, strFullPath, nStart, nLength, true) != 0)
                    break;

                nCommon = i;
            }

            if (nCommon == -1)
                return strFullPath;

            var strBuilder = new StringBuilder();
            for (int i = nCommon + 1; i < nPosCount; ++i)
                strBuilder.Append("..\\");

            int nSubStartPos = DirectoryPos[nCommon] + 1;
            if (nSubStartPos < strFullPath.Length)
                strBuilder.Append(strFullPath.Substring(nSubStartPos));

            string strResult = strBuilder.ToString();
            return strResult == string.Empty ? ".\\" : strResult;
        }

        /// <summary>
        /// 获取文本的宽度
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static float GetTextWidth(Font font, string text, int fontSize)
        {
            CharacterInfo characterInfo = new CharacterInfo();
            char[] arr = text.ToCharArray();
            int totalLength = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                font.GetCharacterInfo(arr[i], out characterInfo, fontSize);
                totalLength += characterInfo.advance;
            }
            return totalLength;
        }
    }
}
