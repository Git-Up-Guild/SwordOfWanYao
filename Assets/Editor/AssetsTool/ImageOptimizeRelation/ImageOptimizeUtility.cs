using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using ReferenceFinder;

public class ImageOptimizeUtility
{


    /// <summary>
    /// 将绝对路径 转换为 Assets相对路径
    /// </summary>
    /// <param name="absolutePath"></param>
    /// <returns></returns>
    public static string AbsolutePath2AssetPath(string absolutePath)
    {
        absolutePath = absolutePath.Replace('\\','/');
        int sybolIndex = absolutePath.IndexOf("/Assets");
        if(sybolIndex > 0)
        {
            return absolutePath.Substring(sybolIndex + 1);
        }
        Debug.LogError($"绝对路径转换为Assets相对路径失败 {absolutePath}");
        return null;
    }


    /// <summary>
    /// 替换 预制体 的依赖
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <param name="oldDependencyGUID">旧依赖GUID</param>
    /// <param name="newDependencyGUID">新依赖GUID</param>
    public static void ReplacePrefabDependencies(GameObject prefab, string oldDependencyGUID, string newDependencyGUID,string path)
    {
        string prefabPath = null;

        if(string.IsNullOrEmpty(path))
        {
            prefabPath = AssetDatabase.GetAssetPath(prefab);
        }
        else
        {
            prefabPath = path;
        }
        string[] allLines = File.ReadAllLines(prefabPath);

        for (int i = 0; i < allLines.Length; i++)
        {
            if (allLines[i].Contains(oldDependencyGUID))
            {
                allLines[i] = allLines[i].Replace(oldDependencyGUID, newDependencyGUID);
            }

        }

        File.WriteAllLines(prefabPath, allLines);
    }

    public static void ReplacePrefabDependencies(GameObject prefab, string[] oldDependencyGUID, string newDependencyGUID)
    {
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        string[] allLines = File.ReadAllLines(prefabPath);

        for (int i = 0; i < allLines.Length; i++)
        {
            for (int j = 0; j < oldDependencyGUID.Length; j++)
            {
                if (allLines[i].Contains(oldDependencyGUID[j]))
                {
                    allLines[i] = allLines[i].Replace(oldDependencyGUID[j], newDependencyGUID);
                }
            }
        }

        File.WriteAllLines(prefabPath, allLines);
    }

    /// <summary>
    /// 移动资源文件（包含.meta文件，如果目标目录下存在同名文件，将会重命名文件)（相对路径）
    /// </summary>
    /// <param name="src">源文件</param>
    /// <param name="destDir">目标目录</param>
    public static bool MoveAsset(string src, string destDir)
    {

        string fileName = src;
        int lastSymbolIndex = src.LastIndexOf('/');
        if (lastSymbolIndex > 0)
        {
            fileName = src.Substring(lastSymbolIndex + 1);
        }


        string destPath = $"{destDir}/{fileName}";

        if (src == destPath)//如果移动前 文件路径 与 移动后文件路径一样 不执行
            return false;


        if (File.Exists(destPath))
        {
            //该目录下已存在同名文件
            int index = 1;
            string[] fileNameSplit = fileName.Split('.');
            if (fileNameSplit.Length == 2)
            {
                while (File.Exists($"{destDir}/{fileNameSplit[0]}_{index}.{fileNameSplit[1]}"))
                {
                    index++;
                }

          
                File.Move(src, $"{destDir}/{fileNameSplit[0]}_{index}.{fileNameSplit[1]}");
                File.Move($"{src}.meta", $"{destDir}/{fileNameSplit[0]}_{index}.{fileNameSplit[1]}.meta");
                Debug.Log($"由于目标目录下存在同名文件，重命名 {src} ==> {destDir}/{fileNameSplit[0]}_{index}.{fileNameSplit[1]}");
            }

        }
        else
        {

            File.Move(src, destPath);
            File.Move($"{src}.meta", $"{destPath}.meta");

            //Debug.Log($"移动文件{src} ==> {destPath}");
        }
        return true;
    }

    /// <summary>
    /// 获得预制体的 所有图片依赖
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static HashSet<string> GetAllImageFileDependencies(GameObject go)
    {
        HashSet<string> result = new HashSet<string>();
        if (go != null)
        {
            string[] arr = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(go));
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].EndsWith(".png"))
                {
                    result.Add(arr[i]);
                }
            }
        }
        return result;
    }



    public static List<GameObject> GetPrefabsInDirectory(string directory)
    {
        List<string> directorys = new List<string>()
        {
            directory,
        };

        return GetPrefabsInDirectory(directorys);

    }
    /// <summary>
    /// 获得 目录下所有预制体
    /// </summary>
    /// <param name="directorys"></param>
    /// <returns></returns>
    public static List<GameObject> GetPrefabsInDirectory(List<string> directorys)
    {
        List<GameObject> prefabs = new List<GameObject>();


        for(int j=0;j<directorys.Count;++j)
        {
            if (!Directory.Exists(directorys[j]))
            {
                Debug.LogError($"文件夹路径不存在 {directorys}");
                return prefabs;
            }
            string[] prefabFiles = Directory.GetFiles(directorys[j], "*.prefab", SearchOption.AllDirectories);//绝对路径
            for (int i = 0; i < prefabFiles.Length; i++)
            {
                prefabFiles[i] = AbsolutePath2AssetPath(prefabFiles[i]);//绝对路径 转 Assets 相对路径
            }


            for (int i = 0; i < prefabFiles.Length; i++)
            {
                EditorUtility.DisplayProgressBar("获取预制体", prefabFiles[i], (float)(i + 1) / prefabFiles.Length);

                GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabFiles[i], typeof(GameObject)) as GameObject;
                if (prefab != null)
                    prefabs.Add(prefab);
                else
                    Debug.LogError("预制体获取失败" + prefabFiles[i]);
            }
            EditorUtility.ClearProgressBar();
        }


       
        
        return prefabs;
    }

    /// <summary>
    /// 获取预制体 图片依赖 从而获得 图片 对应 预制体 字典
    /// </summary>
    /// <param name="prefabsPath"></param>
    /// <param name="image2PrefabListDict">图片 对应 预制体列表 字典</param>
    /// 
    public static Dictionary<string, List<GameObject>> GetPrefasImageDependData(string prefabsPath)
    {
        List<string> prefabsPaths = new List<string>()
        {
            prefabsPath,
        };

        return GetPrefasImageDependData(prefabsPaths);  
    }
    public static Dictionary<string, List<GameObject>> GetPrefasImageDependData(List<string> prefabsPaths)
    {
        Dictionary<string, List<GameObject>> image2PrefabListDict = new Dictionary<string, List<GameObject>>();

        List<GameObject> allPrefabList = GetPrefabsInDirectory(prefabsPaths);

        float index = 1f;

        ReferenceFinderData.CollectDependenciesInfo();

        foreach (var prefab in allPrefabList)
        {
            EditorUtility.DisplayProgressBar("获取预制体依赖图片", prefab.name, index / allPrefabList.Count);

            /*
            if (prefab.name.IndexOf("UITowerStageResult")>=0)
            {
                Debug.LogError(prefab.name);
            }*/

            var allImageFileSet = GetAllImageFileDependencies(prefab);


            EditorUtility.DisplayProgressBar("构建依赖图片对应预制体列表字典", prefab.name, index / allPrefabList.Count);

            foreach (var imageFilePath in allImageFileSet)
            {
                /*
                if(imageFilePath.IndexOf("ty_lv") >=0)
                {
                    Debug.LogError(imageFilePath);
                }*/

                if (image2PrefabListDict.ContainsKey(imageFilePath))
                {
                    continue;
                    
                }

                List<GameObject> listPathObject = new List<GameObject>();
                image2PrefabListDict.Add(imageFilePath, listPathObject);
                string guid = AssetDatabase.AssetPathToGUID(imageFilePath);
                AssetDescription ad = ReferenceFinderData.Get(guid);
                if(ad!=null)
                {
                    int count = ad.references.Count;
                    for(int i=0;i<count;++i)
                    {
                        string goPath = AssetDatabase.GUIDToAssetPath(ad.references[i]);
                        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(goPath);
                        if(null!=go)
                        {
                            listPathObject.Add(go);
                        }

                       
                    }
                }else
                {
                    Debug.LogError(imageFilePath+" 没有GameObject 引用");
                }

                
            }

            ++index;
        }
        EditorUtility.ClearProgressBar();
        return image2PrefabListDict;
    }

    /// <summary>
    /// 返回 相同图片MD5 对应 同名图片文件路径列表 字典
    /// </summary>
    /// <param name="allFiles">所有文件的相对路径</param>
    public static Dictionary<string, List<string>> GetSameImage2PathDict(string[] allFiles)
    {
        Dictionary<string, List<string>> sameImage2FilePathDict = new Dictionary<string, List<string>>();//相同图片 对应 图片路径列表

        for (int i = 0; i < allFiles.Length; i++)
        {
            EditorUtility.DisplayProgressBar("构建图片MD5对应图片路径字典", allFiles[i], 1f * i / allFiles.Length);
            string fileMD5 = MD5.GetMD5(allFiles[i]);
            if (!sameImage2FilePathDict.ContainsKey(fileMD5))
            {
                sameImage2FilePathDict.Add(fileMD5, new List<string>() { allFiles[i] });
            }
            else
            {
                sameImage2FilePathDict[fileMD5].Add(allFiles[i]);
            }
        }

        EditorUtility.ClearProgressBar();
        return sameImage2FilePathDict;
    }


    /// <summary>
    /// 返回 相同图片MD5 对应 同名图片文件路径列表 字典
    /// </summary>
    /// <param name="imageFilesDirectory"></param>
    public static Dictionary<string, List<string>> GetSameImage2PathDict(string imageFilesDirectory)
    {
        Dictionary<string, List<string>> sameImage2FilePathDict = new Dictionary<string, List<string>>();//相同图片 对应 图片路径列表

        EditorUtility.DisplayProgressBar("获取目录图片文件", "", 0f);
        string[] allImageFilesArr = Directory.GetFiles(imageFilesDirectory, "*.png", SearchOption.AllDirectories);
        EditorUtility.ClearProgressBar();

        for (int i = 0; i < allImageFilesArr.Length; i++)
        {
            allImageFilesArr[i] = allImageFilesArr[i].Replace('\\', '/');
            int index = allImageFilesArr[i].IndexOf("Assets/");

            EditorUtility.DisplayProgressBar("构建图片MD5对应图片路径字典", allImageFilesArr[i], 1f * i / allImageFilesArr.Length);

            if (index >= 0)
            {

                allImageFilesArr[i] = allImageFilesArr[i].Substring(index);
                string fileMD5 = MD5.GetMD5(allImageFilesArr[i]);
                if (!sameImage2FilePathDict.ContainsKey(fileMD5))
                {
                    sameImage2FilePathDict.Add(fileMD5, new List<string>() { allImageFilesArr[i] });
                }
                else
                {
                    sameImage2FilePathDict[fileMD5].Add(allImageFilesArr[i]);
                }
            }


        }

        EditorUtility.ClearProgressBar();
        return sameImage2FilePathDict;
    }
}
