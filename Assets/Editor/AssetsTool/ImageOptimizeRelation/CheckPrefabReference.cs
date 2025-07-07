using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Linq;
using ReferenceFinder;

public class CheckPrefabReference : EditorWindow
{
    [MenuItem("XGame/资源工具/检测预制体的引用")]
    static void ShowWindow()
    {
        GetWindow<CheckPrefabReference>();
    }

    public CheckPrefabReference()
    {
        this.titleContent = new GUIContent("查找未引用预制体");
        this.minSize = new Vector2(750f, 512f);
    }

    string m_detectPrefabPath;//检查预制体 目录
    List<string> m_processImageDirectories;//处理图片文件夹
    List<string> m_ignoreImageDirectories;//忽略 处理图片文件夹
    Dictionary<string, List<GameObject>> m_allImage2PrefabListDict;//图片 - 预制体 字典
    HashSet<string> m_allImagSet;//处理图片文件夹下 所有图片集合
    HashSet<string> m_unReferenceImageSet;//没有引用到的图片集合
    HashSet<string> m_unReferenceImageWithIgnoreDirectorySet;//执行忽略目录后 没有引用到的图片集合

    string m_deleteImageDestPath;//等待删除 的图片将要移动到的目录

    private void OnEnable()
    {
        m_detectPrefabPath = $"{Application.dataPath}/G_Resources";
        m_processImageDirectories = new List<string>()
        {
            $"{Application.dataPath}/G_Resources/Game/Lua/Sources" ,
            $"{Application.dataPath}/../../../../tools/datapresent/data/csv_script" ,
        };


    }

    private void OnGUI()
    {
        using (var hor = new GUILayout.HorizontalScope())
        {
            m_detectPrefabPath = EditorGUILayout.TextField("扫描预制体目录：", m_detectPrefabPath);
            if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
            {
                m_detectPrefabPath = EditorUtility.OpenFolderPanel("扫描预制体目录", m_detectPrefabPath, $"{Application.dataPath}/G_Resources");
            }
        }

        if (GUILayout.Button("增加搜索目录"))
        {
            m_processImageDirectories.Add("");
        }
        for (int i = 0; i < m_processImageDirectories.Count; i++)
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                this.m_processImageDirectories[i] = EditorGUILayout.TextField("", this.m_processImageDirectories[i]);
                if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    this.m_processImageDirectories[i] = EditorUtility.OpenFolderPanel("目录", this.m_processImageDirectories[i], $"{Application.dataPath}/G_Resources/Artist/UI1.0");
                }
                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    this.m_processImageDirectories.RemoveAt(i);
                }
            }
        }

        if (GUILayout.Button("输出没有引用的预制体"))
        {
            OutputUnRefrencePrefab();
        }


    }



    void OutputUnRefrencePrefab()
    {

        //1.获取没引用的预制体
        ReferenceFinderData.ForceUpdateRefrenceInfo();
        Dictionary<string,string> prefabList = new Dictionary<string,string>();
        HashSet<string> hashUnRefrence = new HashSet<string>(); 
        string[] prefabPaths = Directory.GetFiles(m_detectPrefabPath, "*.prefab", SearchOption.AllDirectories);
        for (int j = 0; j < prefabPaths.Length; ++j)
        {
            string path = prefabPaths[j].Substring(prefabPaths[j].IndexOf("Assets")).Replace("\\", "/");


            string oldTexGuid = AssetDatabase.AssetPathToGUID(path);
            AssetDescription data = ReferenceFinderData.Get(oldTexGuid);
            if (data != null&& data.references.Count==0)
            {
                string filename = path.Substring(path.LastIndexOf("/") + 1);
                //filename = filename.Substring(0, filename.LastIndexOf("."));
                prefabList.Add(filename, path);
            }

  
        }

        //2.获取所有目标文件的内容
        string content = "";
        Dictionary<string, string> dicSearchContent = new Dictionary<string, string>();
        int nCount = m_processImageDirectories.Count;
        for(int i=0;i<nCount;++i)
        {
            string[] Paths = Directory.GetFiles(m_processImageDirectories[i], "*.*", SearchOption.AllDirectories);

            for(int j=0;j< Paths.Length;++j)
            {

                if(Paths[j].EndsWith(".meta"))
                {
                    continue;
                }

                content = File.ReadAllText(Paths[j]);
                hashUnRefrence.Clear();
                foreach(var item in prefabList.Keys)
                {
                    if(content.IndexOf(item)>=0)
                    {
                        hashUnRefrence.Add(item);
                    }
                }

                //删除有引用的预制体
                foreach(var item in hashUnRefrence)
                {
                    prefabList.Remove(item);
                }

            }
        }


        string filePath = Application.dataPath + "/UnRefrencePrefab.csv";

        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }


        FileStream fs = File.Create(filePath);
        StreamWriter sw = new StreamWriter(fs);


        foreach (var item in prefabList.Values)
        {
           sw.WriteLine(item);
        }

        sw.Flush();
        sw.Close();
        fs.Close();

    }

}
