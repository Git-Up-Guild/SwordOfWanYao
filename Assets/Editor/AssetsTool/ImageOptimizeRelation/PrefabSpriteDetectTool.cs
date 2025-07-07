
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.U2D;
using System.Text;

public class PrefabDetectTool : EditorWindow
{

    [MenuItem("XGame/资源工具/UI窗口输出依赖图集列表工具")]
    static void ShowWindow()
    {
        GetWindow<PrefabDetectTool>();
    }

    public PrefabDetectTool()
    {
        this.titleContent = new GUIContent("UI窗口输出依赖图集列表工具");
        //m_prefabsDir = Application.dataPath;
        //m_outputPath = Application.dataPath;
    }

    string m_prefabsDir = "";
    string m_outputPath = "";
    string[] m_imagePatterns = new string[]
    {
        ".png"
    };
    Dictionary<string, string> m_image2spriteAtlasDict;//图片对应图集的字典
    Dictionary<string, List<string>> m_image2MultiSpriteAtlasesDict;//单个图片被多个图集引用的字典（图片路径-图集路径数组）

    private void OnGUI()
    {
        using (var hor = new GUILayout.HorizontalScope())
        {
            m_prefabsDir = EditorGUILayout.TextField(new GUIContent("预制体文件夹路径：", "绝对路径"), m_prefabsDir);
            //if(m_prefabsDir == "")
            //    m_prefabsDir = Application.dataPath;
            if (GUILayout.Button("浏览", GUILayout.Height(20f), GUILayout.Width(64f)))
            {
                m_prefabsDir = EditorUtility.OpenFolderPanel("选择 预制体文件夹路径", Application.dataPath, "");
            }
        }
        using (var hor = new GUILayout.HorizontalScope())
        {
            m_outputPath = EditorGUILayout.TextField(new GUIContent("输出文本文件路径：", "绝对路径"), m_outputPath);
            //if (m_outputPath == "")
            //    m_outputPath = Application.dataPath;
            if (GUILayout.Button("浏览", GUILayout.Height(20f), GUILayout.Width(64f)))
            {
                m_outputPath = EditorUtility.OpenFolderPanel("选择 输出文本文件路径", Application.dataPath, "");
            }
        }

        if (GUILayout.Button("计算所选择的预制体的依赖"))
        {
            SetupImage2SpriteAtlasDict();
            DebugSpriteAtlasInfo();
            Dictionary<string,List<string>> allDependencies = GetSelectPrefabDependencies();
            if (allDependencies != null)
            {
                Dictionary<string, List<string>> allImageDependencies = GetImageDependenciesInAllPrefabs(allDependencies, m_imagePatterns);
                Dictionary<string, List<List<string>>> result = GetPrefabsDependenciesResult(allImageDependencies);

                WritePrefabInfo(result, m_outputPath);
            }

        }
        if (GUILayout.Button("计算所选择的文件夹中的所有预制体的依赖"))
        {
            SetupImage2SpriteAtlasDict();
            DebugSpriteAtlasInfo();
            Dictionary<string, List<string>> allDependencies = GetPathPrefabDependencies(m_prefabsDir);
            if(allDependencies != null)
            {
                Dictionary<string, List<string>> allImageDependencies = GetImageDependenciesInAllPrefabs(allDependencies, m_imagePatterns);
                DebugDependenciesInfo(allImageDependencies);
                Dictionary<string, List<List<string>>> result = GetPrefabsDependenciesResult(allImageDependencies);
                WritePrefabInfo(result, m_outputPath);
            }
            
        }
    }

    #region Debug 

    /// <summary>
    /// 打印工程中所有图集的依赖信息
    /// </summary>
    void DebugSpriteAtlasInfo()
    {
        StringBuilder messageBuilder = new StringBuilder("图片 - 图集 对应字典\n{\n");
        foreach (var item in m_image2spriteAtlasDict)
        {
            messageBuilder.Append('\t');
            messageBuilder.Append("图片路径：");
            messageBuilder.Append(item.Key);
            messageBuilder.Append("\t\t图集路径：");
            messageBuilder.Append(item.Value); 
            messageBuilder.Append('\n');
        }
        messageBuilder.Append("}\n（单个图片 - 多个图集） 对应字典\n{\n");
        foreach (var item in m_image2MultiSpriteAtlasesDict)
        {
            messageBuilder.Append('\t');
            messageBuilder.Append("图片路径：");
            messageBuilder.Append(item.Key);
            messageBuilder.Append("\n\t{");
            foreach (var spriteAtlasPath in item.Value)
            {
                messageBuilder.Append("\n\t\t图集路径：");
                messageBuilder.Append(spriteAtlasPath);
            }
            messageBuilder.Append("\n\t}");
        }
        messageBuilder.Append("\n}");
        Debug.Log(messageBuilder.ToString());
    }

    /// <summary>
    /// 打印传入函数的所有预制体及其依赖信息
    /// </summary>
    /// <param name="input"></param>
    void DebugDependenciesInfo(Dictionary<string,List<string>> input)
    {
        if (input.Count > 0)
        {
            string message = "";
            foreach (var item in input)
            {
                message += item.Key;
                message += "\t预制体依赖\n{\n";
                foreach (var dependence in item.Value)
                {
                    message += '\t';
                    message += dependence;
                    message += '\n';
                }
                message += "}\n\n";
            }
            Debug.Log(message);
        }
        else
        {
            Debug.Log("未选中预制体");
        }
    }

    #endregion

    /// <summary>
    /// 设置图片对应图集对应字典 以及 图片多图集引用对应字典
    /// </summary>
    void SetupImage2SpriteAtlasDict()
    {
        m_image2spriteAtlasDict = new Dictionary<string, string>();
        m_image2MultiSpriteAtlasesDict = new Dictionary<string, List<string>>();

        string[] arrAtlasPaths = Directory.GetFiles(Application.dataPath, "*.spriteatlas", SearchOption.AllDirectories);//获得所有图集文件的绝对路径
        for (int i = 0; i < arrAtlasPaths.Length; i++)
        {
            arrAtlasPaths[i] = arrAtlasPaths[i].Replace('\\', '/');
            int index = arrAtlasPaths[i].IndexOf("Assets/");
            if (index < 0)
            {
                Debug.LogError("文件路径出错");
            }
            arrAtlasPaths[i] = arrAtlasPaths[i].Substring(index);//因为AssetDataBase.Load相关函数接受路径参数为相对路径
        }

        SpriteAtlas[] arrAtlasObjs = new SpriteAtlas[arrAtlasPaths.Length];

        for (int i = 0; i < arrAtlasPaths.Length; i++)
        {
            SpriteAtlas obj = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(arrAtlasPaths[i]);
            if (obj != null)
                arrAtlasObjs[i] = obj;
        }

        for (int i = 0; i < arrAtlasObjs.Length; i++)
        {
            //Debug.Log(arrAtlasObjs[i].name + " 图集 包含了 图片数量 " + arrAtlasObjs[i].spriteCount);

            Sprite[] sprites = new Sprite[arrAtlasObjs[i].spriteCount];
            arrAtlasObjs[i].GetSprites(sprites);
            for (int j = 0; j < sprites.Length; j++)
            {
                if(sprites[j] != null && sprites[j].texture != null)
                {
                    string textureReferece = AssetDatabase.GetAssetPath(sprites[j].texture);
                    if(!m_image2spriteAtlasDict.ContainsKey(textureReferece))
                    {
                        m_image2spriteAtlasDict.Add(textureReferece, arrAtlasPaths[i]);
                    }
                    else
                    {
                        if (m_image2MultiSpriteAtlasesDict.ContainsKey(textureReferece))
                        {
                            m_image2MultiSpriteAtlasesDict[textureReferece].Add(arrAtlasPaths[i]);
                        }
                        else
                        {
                            List<string> spriteAtlasPaths = new List<string>();
                            spriteAtlasPaths.Add(m_image2spriteAtlasDict[textureReferece]);
                            spriteAtlasPaths.Add(arrAtlasPaths[i]);
                            m_image2MultiSpriteAtlasesDict.Add(textureReferece, spriteAtlasPaths);
                        }

                    }

                }

                //Debug.LogFormat("成功转换成资源对象；对象路径为{0}", AssetDatabase.GetAssetPath(sprites[j].texture));

            }
        }


        foreach (var item in m_image2MultiSpriteAtlasesDict)
        {
            if (item.Value != null)
            {
                //如果一个图片被多个图集引用，error提示，字典value随便选一个图集，并存入输出列表
                string message = "图片\t" + item.Key + "\t被多个图集引用 \n{";
                foreach (var spriteAtlas in item.Value)
                {
                    message += "\n\t";
                    message += spriteAtlas;
                }
                message += "\n}";
                Debug.LogError(message);
            }
        }
    }

    /// <summary>
    /// 获取所选的预制体的依赖
    /// </summary>
    /// <returns>预制体的依赖文件（键：预制体；值：预制体所依赖的文件）</returns>
    Dictionary<string, List<string>> GetSelectPrefabDependencies()
    {
        //string[] guids = Selection.assetGUIDs;//获取当前选中的asset的GUID
        Dictionary<string,List<string>> result = new Dictionary<string, List<string>>();
        Object[] selectObjs = Selection.GetFiltered<Object>(SelectionMode.Unfiltered);//获取当前选中的asset的对象
        for (int i = 0; i < selectObjs.Length; i++)
        {
            string prefabPath = AssetDatabase.GetAssetPath(selectObjs[i]);
            List<string> dependencies = GetAllDependencies(prefabPath);
            result.Add(prefabPath, dependencies);
        }
        return result;
    }

    /// <summary>
    /// 获取指定文件夹下的所有预制体的依赖
    /// </summary>
    /// <param name="dir">绝对路径</param>
    /// <returns>预制体的依赖文件（键：预制体；值：预制体所依赖的文件）</returns>
    Dictionary<string, List<string>> GetPathPrefabDependencies(string dir)
    {
        if(!Directory.Exists(dir))
        {
            Debug.LogErrorFormat("文件夹路径不存在；{0}", dir);
            return null;
        }
        string[] prefabNames = Directory.GetFiles(dir,"*.prefab",SearchOption.AllDirectories);//绝对路径
        for (int i = 0; i < prefabNames.Length; i++)
        {
            prefabNames[i] = prefabNames[i].Replace('\\','/');//统一成 斜线
        }
        for (int i = 0; i < prefabNames.Length; i++)//转换成 相对路径（因为AssetDataBase读取路径 为 相对路径）
        {
            int index = prefabNames[i].IndexOf("Assets/");
            if(index < 0)
            {
                Debug.LogErrorFormat("预制体路径错误 {0}", prefabNames[i]);
                return null;
            }
            prefabNames[i] = prefabNames[i].Substring(index);
        }

        List<Object> allPrefabObjs = new List<Object>();

        for (int i = 0; i < prefabNames.Length; i++)
        {
            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabNames[i]);
            if (prefab != null)
                allPrefabObjs.Add(prefab);
        }

        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

        foreach (Object item in allPrefabObjs)
        {
            string prefabPath = AssetDatabase.GetAssetPath(item);
            List<string> dependencies = GetAllDependencies(prefabPath);
            result.Add(prefabPath, dependencies);
        }
        return result;
    }

    /// <summary>
    /// 获取一个预制体的所有依赖
    /// </summary>
    /// <param name="prefab">预制体对象</param>
    /// <returns>一个预制体的所有依赖（依赖文件路径）</returns>
    List<string> GetAllDependencies(string path)
    {


        List<string> result = new List<string>();


        string[] arr = AssetDatabase.GetDependencies(path);
        for (int i = 0; i < arr.Length; i++)
        {
            result.Add(arr[i]);
        }
        return result;

            /*
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string guid, out long localid))
        {
            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(prefab);
            if (prefabType == PrefabAssetType.Regular)
            {
                Queue<string> queue = new Queue<string>();
                queue.Enqueue(AssetDatabase.GUIDToAssetPath(guid));
                while (queue.Count > 0)
                {
                    string root = queue.Dequeue();
                    string[] dependencies = AssetDatabase.GetDependencies(root);
                    if (dependencies != null && dependencies.Length > 0)
                    {
                        for (int j = 0; j < dependencies.Length; j++)
                        {
                            if (dependencies[j] == root)
                                continue;
                            queue.Enqueue(dependencies[j]);
                            result.Add(dependencies[j]);
                        }
                    }
                }
            }
        }

        return result;
            */
    }

    /// <summary>
    /// 获取所有预制体中的图片依赖
    /// </summary>
    /// <param name="input"></param>
    /// <param name="imagePatterns"></param>
    /// <returns></returns>
    Dictionary<string, List<string>> GetImageDependenciesInAllPrefabs(Dictionary<string, List<string>> input, string[] imagePatterns)
    {

        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
        foreach (var item in input)
        {
            List<string> imageFilesList = new List<string>();
            foreach (var file in item.Value)
            {
                bool isImageFile = false;
                for (int i = 0; i < imagePatterns.Length && isImageFile == false; i++)
                {
                    int index = file.IndexOf(imagePatterns[i]);
                    if (index > 0)
                    {
                        isImageFile = true;
                    }
                }
                if (isImageFile)
                    imageFilesList.Add(file);
            }

            result.Add(item.Key, imageFilesList);
        }
        return result;
    }

    /// <summary>
    /// 根据输入所有预制体生成预制体对应的三个需要的列表
    /// 预制体引用的图集的列表(0)
    /// 预制体依赖图片被多个图集依赖的列表(1)
    /// 预制体散图（即所依赖的图片中未被打入图集的图片）的列表）(2)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Dictionary<string, List<List<string>>> GetPrefabsDependenciesResult(Dictionary<string, List<string>> input)
    {
        Dictionary<string, List<List<string>>> result = new Dictionary<string, List<List<string>>>();

        foreach (var item in input)
        {
            List<string> spriteAtlasDependenciesList = new List<string>();//预制体引用的图集的列表(0)
            List<string> imageMultiSpriteAtlasList = new List<string>();//预制体依赖图片被多个图集依赖的列表(1)
            List<string> singleImageList = new List<string>();//预制体散图（即所依赖的图片中未被打入图集的图片）的列表）(2)
            foreach (var imageDeendence in item.Value)
            {
                if(m_image2spriteAtlasDict.ContainsKey(imageDeendence))
                {
                    string spriteAtlas = m_image2spriteAtlasDict[imageDeendence];
                    if (!spriteAtlasDependenciesList.Contains(spriteAtlas))//如果预制体图集依赖不存在该图集依赖
                        spriteAtlasDependenciesList.Add(spriteAtlas);//预制体图集依赖加入该图集依赖(1)
                }
                else
                {
                    singleImageList.Add(imageDeendence);//(3)
                    //散图列表
                }

                if(m_image2MultiSpriteAtlasesDict.ContainsKey(imageDeendence))
                {
                    string spriteAtlas = m_image2MultiSpriteAtlasesDict[imageDeendence][0];
                    imageMultiSpriteAtlasList.Add(imageDeendence);
                }
            }
            List<List<string>> prefabInfoList = new List<List<string>>();//任务所需要的三个列表
            prefabInfoList.Add(spriteAtlasDependenciesList);//(0)预制体引用的图集的列表
            prefabInfoList.Add(imageMultiSpriteAtlasList);//(1)预制体依赖图片被多个图集依赖的列表
            prefabInfoList.Add(singleImageList);//(2)预制体散图（即所依赖的图片中未被打入图集的图片）的列表）
            result.Add(item.Key, prefabInfoList);
        }
        return result;
    }

    void WritePrefabInfo(Dictionary<string,List<List<string>>> input, string outputPath)
    {
        if(!Directory.Exists(outputPath))
        {
            Debug.LogErrorFormat("输出路径不存在：{0}",outputPath);
            return;
        }

        //因为 要 排序 预制体 图集引用 所以只能先取出来了
        Dictionary<string, int> prefab2spriteAtlasDependenciesCount = new Dictionary<string, int>();
        foreach (var prefabInfo in input)
        {
            prefab2spriteAtlasDependenciesCount.Add(prefabInfo.Key, prefabInfo.Value[0].Count);
        }
        List<string> prefabNameSortList = new List<string>();

        int count = prefab2spriteAtlasDependenciesCount.Count;

        for (int i = 0; i < count; i++)
        {
            string maxPrefab = null;
            int minCount = -1;
            foreach (var item in prefab2spriteAtlasDependenciesCount)
            {
                if (item.Value > minCount)
                {
                    maxPrefab = item.Key;
                    minCount = item.Value;
                }
            }
            prefabNameSortList.Add(maxPrefab);
            prefab2spriteAtlasDependenciesCount.Remove(maxPrefab);

        }



        StringBuilder firstBuilder = new StringBuilder("预制体引用图集列表\n");
        StringBuilder secondBuilder = new StringBuilder("预制体依赖图片被多图集引用列表\n");
        StringBuilder thirdBuilder = new StringBuilder("预制体依赖散图列表\n");

        for (int i = 0; i < prefabNameSortList.Count; i++)
        {
            firstBuilder.Append(prefabNameSortList[i]);
            firstBuilder.Append("\n{\n");
            foreach (var spriteAtlas in input[prefabNameSortList[i]][0])//第1个列表
            {
                firstBuilder.Append('\t');
                firstBuilder.Append(spriteAtlas);
                firstBuilder.Append('\n');
            }
            firstBuilder.Append("}\n");
        }


        foreach (var prefabInfo in input)
        {
            //firstBuilder.Append(prefabInfo.Key);
            //firstBuilder.Append("\n{\n");
            //foreach (var spriteAtlas in prefabInfo.Value[0])//第1个列表
            //{
            //    firstBuilder.Append('\t');
            //    firstBuilder.Append(spriteAtlas);
            //    firstBuilder.Append('\n');
            //}
            //firstBuilder.Append("}\n");

            secondBuilder.Append(prefabInfo.Key);
            secondBuilder.Append("\n{\n");
            foreach (var imageMultiSpriteAtlasReference in prefabInfo.Value[1])//第2个列表
            {
                secondBuilder.Append('\t');
                secondBuilder.Append(imageMultiSpriteAtlasReference);
                secondBuilder.Append('\n');
            }
            secondBuilder.Append("}\n");

            thirdBuilder.Append(prefabInfo.Key);
            thirdBuilder.Append("\n{\n");
            foreach (var singleImage in prefabInfo.Value[2])//第3个列表
            {
                thirdBuilder.Append('\t');
                thirdBuilder.Append(singleImage);
                thirdBuilder.Append('\n');
            }
            thirdBuilder.Append("}\n");
        }
        try
        {
            if (File.Exists(outputPath + "\\预制体引用图集列表.txt"))
                File.Delete(outputPath + "\\预制体引用图集列表.txt");
            FileStream firstFS = File.Create(outputPath + "\\预制体引用图集列表.txt");
            if (File.Exists(outputPath + "\\预制体依赖图片被多图集引用列表.txt"))
                File.Delete(outputPath + "\\预制体依赖图片被多图集引用列表.txt");
            FileStream secondFS = File.Create(outputPath + "\\预制体依赖图片被多图集引用列表.txt");
            if (File.Exists(outputPath + "\\预制体依赖散图列表.txt"))
                File.Delete(outputPath + "\\预制体依赖散图列表.txt");
            FileStream thirdFS = File.Create(outputPath + "\\预制体依赖散图列表.txt");

            StreamWriter sw;
            sw = new StreamWriter(firstFS);
            sw.Write(firstBuilder.ToString());
            sw.Flush();
            sw.Dispose();
            sw.Close();
            firstFS.Dispose();
            firstFS.Close();
            sw = new StreamWriter(secondFS);
            sw.Write(secondBuilder.ToString());
            sw.Flush();
            sw.Dispose();
            sw.Close();
            secondFS.Dispose();
            secondFS.Close();
            sw = new StreamWriter(thirdFS);
            sw.Write(thirdBuilder.ToString());
            sw.Flush();
            sw.Dispose();
            sw.Close();
            thirdFS.Dispose();
            thirdFS.Close();
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.ToString());
        }

    }
}
