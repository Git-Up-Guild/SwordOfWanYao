using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Linq;
using System;
using DG.Tweening.Plugins.Core.PathCore;

public class UnReferenceImageDeleter : EditorWindow
{
    [MenuItem("XGame/资源工具/删除未引用图片")]
    static void ShowWindow()
    {
        GetWindow<UnReferenceImageDeleter>();
    }

    public UnReferenceImageDeleter()
    {
        this.titleContent = new GUIContent("删除未引用图片");
        this.minSize = new Vector2(750f, 512f);
    }

    List<string> m_detectPrefabPaths;//检查预制体 目录
    List<string> m_processImageDirectories;//处理图片文件夹
    List<string> m_ignoreImageDirectories;//忽略 处理图片文件夹
    Dictionary<string, List<GameObject>> m_allImage2PrefabListDict;//图片 - 预制体 字典
    HashSet<string> m_allImagSet;//处理图片文件夹下 所有图片集合
    HashSet<string> m_unReferenceImageSet;//没有引用到的图片集合
    HashSet<string> m_unReferenceImageWithIgnoreDirectorySet;//执行忽略目录后 没有引用到的图片集合

    string m_deleteImageDestPath;//等待删除 的图片将要移动到的目录

    private void OnEnable()
    {
        m_detectPrefabPaths = new List<string>()
        {
            $"{Application.dataPath}/Game/ImmortalFamily/GameResources",
            $"{Application.dataPath}/G_Resources/Game/Prefab",
        };
            
            
        m_processImageDirectories = new List<string>()
        {
            $"{Application.dataPath}/G_Resources" ,
            $"{Application.dataPath}/G_Artist" ,
            $"{Application.dataPath}/Game/ImmortalFamily/GameResources" ,

        };

        m_ignoreImageDirectories = new List<string>()
        {
            //$"{Application.dataPath}/G_Resources/Artist/Icon" ,
            //$"{Application.dataPath}/G_Resources/Artist/Artist/Atlas" ,
            //$"{Application.dataPath}/G_Resources/Artist/UI4.0/Common/Reserve" ,
            $"{Application.dataPath}/Game/ImmortalFamily/GameResources/Artist/Icon" ,
           
    
        };

    }

    private void OnGUI()
    {
        /*
        using (var hor = new GUILayout.HorizontalScope())
        {
            m_detectPrefabPaths = EditorGUILayout.TextField("扫描预制体目录：", m_detectPrefabPaths);
            if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
            {
                m_detectPrefabPaths = EditorUtility.OpenFolderPanel("扫描预制体目录", m_detectPrefabPaths, $"{Application.dataPath}/G_Resources");
            }
        }
        */


        if (GUILayout.Button("增加扫描预制体的目录"))
        {
            m_detectPrefabPaths.Add("");
        }
        for (int i = 0; i < m_detectPrefabPaths.Count; i++)
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                this.m_detectPrefabPaths[i] = EditorGUILayout.TextField("", this.m_detectPrefabPaths[i]);
                if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    this.m_detectPrefabPaths[i] = EditorUtility.OpenFolderPanel("图片目录", this.m_detectPrefabPaths[i], $"{Application.dataPath}/G_Resources");
                }
                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    this.m_detectPrefabPaths.RemoveAt(i);
                }
            }
        }



        if (GUILayout.Button("增加处理图片目录（删除的图片都会在这些目录下）"))
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
                    this.m_processImageDirectories[i] = EditorUtility.OpenFolderPanel("图片目录", this.m_processImageDirectories[i], $"{Application.dataPath}/G_Resources/Artist/UI1.0");
                }
                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    this.m_processImageDirectories.RemoveAt(i);
                }
            }
        }

        if (GUILayout.Button("输出相同图片"))
        {
            OutputSameTexture();
        }


        GUILayout.Space(16f);
        if (GUILayout.Button("增加忽略图片目录（在这些目录下的图片即使未被引用也不会被删除）"))
        {
            m_ignoreImageDirectories.Add("");
        }
        for (int i = 0; i < m_ignoreImageDirectories.Count; i++)
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                this.m_ignoreImageDirectories[i] = EditorGUILayout.TextField("", this.m_ignoreImageDirectories[i]);
                if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    this.m_ignoreImageDirectories[i] = EditorUtility.OpenFolderPanel("忽略图片目录", this.m_ignoreImageDirectories[i], $"{Application.dataPath}/G_Resources/Artist/UI1.0");
                }
                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    this.m_ignoreImageDirectories.RemoveAt(i);
                }
            }
        }
        


        if (GUILayout.Button("设置数据"))
        {
            SetupData();
        }
        using (var hor = new GUILayout.HorizontalScope())
        {
            this.m_deleteImageDestPath = EditorGUILayout.TextField("待删除图片目标目录", this.m_deleteImageDestPath);
            if (GUILayout.Button("浏览", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
            {
                this.m_deleteImageDestPath = EditorUtility.OpenFolderPanel("待删除图片目标目录", this.m_deleteImageDestPath, "");
            }
        }
        if (IsDataReady() && GUILayout.Button("写入 Assets 目录下 CSV 信息 && 移动待删除图片到目标目录下"))
        {
            WriteInfoIntoCSV();
            WriteUnreferenceImageInfoIntoCSV();
            WriteUnreferenceImageInfoIntoCSVWithIgnoreDirectory();

            MoveWaitToDeleteImageIntoDirectoty(m_deleteImageDestPath);
        }



        if (IsDataReady() && GUILayout.Button("删除待删除图片及其meta文件"))
        {
            Delete();
        }

        if ( GUILayout.Button("删除空文件"))
        {
            foreach(var dir in m_detectPrefabPaths)
            {
                DeleteEmptyDirectories(dir);
            }    
            
        }
    }

    static void DeleteEmptyDirectories(string path)
    {
        if (!Directory.Exists(path)) return;

        string[] directories = Directory.GetDirectories(path);
        foreach (string dir in directories)
        {
            DeleteEmptyDirectories(dir); // 递归删除子目录中的空目录
        }

        if (Directory.GetFiles(path).Length == 0 && directories.Length == 0) // 如果目录中没有文件和子目录，则删除该目录
        {
            Directory.Delete(path);
            Console.WriteLine($"已删除空目录：{path}");

            string metaPath = path + ".meta";
            if(File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }
        }
    }

    void SetupData()
    {
        m_allImage2PrefabListDict = ImageOptimizeUtility.GetPrefasImageDependData(m_detectPrefabPaths);
        m_allImagSet = GetAllImagSet(m_processImageDirectories);
        m_unReferenceImageSet = GetUnReferenceImageSet();
        m_unReferenceImageWithIgnoreDirectorySet = GetUnReferenceImageSetWithIgnoreDirectory(m_ignoreImageDirectories);
    }
     
    bool IsDataReady()
    {
        if(m_allImage2PrefabListDict != null && m_allImagSet != null && m_unReferenceImageSet != null && m_unReferenceImageWithIgnoreDirectorySet != null)
        {
            return true;
        }
        return false;
    }


    void WriteInfoIntoCSV()
    {
        if(m_allImage2PrefabListDict != null)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in m_allImage2PrefabListDict)
            {
                sb.Append(item.Key);
                sb.Append(',');
                foreach (var prefab in item.Value)
                {
                    sb.Append($"{prefab.name},");
                }
                sb.Append('\n');
            }
            string filePath = $"{Application.dataPath}/1_图片-预制体{System.DateTime.Now:yyyy_MM_dd_HH_mm_ss}.csv";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Close();
            }
        }
    }

    void WriteUnreferenceImageInfoIntoCSV()
    {
        if (m_unReferenceImageSet != null)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in m_unReferenceImageSet)
            {
                sb.Append($"{item},\n");
            }
            string filePath = $"{Application.dataPath}/2_未引用图片{System.DateTime.Now:yyyy_MM_dd_HH_mm_ss}.csv";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Close();
            }
        }
    }

    void WriteUnreferenceImageInfoIntoCSVWithIgnoreDirectory()
    {
        if (m_unReferenceImageSet != null)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in m_unReferenceImageWithIgnoreDirectorySet)
            {
                sb.Append($"{item},\n");
            }
            string filePath = $"{Application.dataPath}/3_待删除图片{System.DateTime.Now:yyyy_MM_dd_HH_mm_ss}.csv";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Close();
            }
        }
    }

    HashSet<string> GetAllImagSet(List<string> imagePaths)
    {
        HashSet<string> result = new HashSet<string>();
        foreach (var path in imagePaths)
        {
            var allImageFiles = (from file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                where file.EndsWith(".png")
                                select file).ToArray();
            for (int i = 0; i < allImageFiles.Length; i++)
            {
                result.Add(ImageOptimizeUtility.AbsolutePath2AssetPath(allImageFiles[i]));
            }

        }
        return result;
    }

    HashSet<string> GetUnReferenceImageSet()
    {
        HashSet<string> result = new HashSet<string>();
        foreach (var file in m_allImagSet)
        {
            if(!m_allImage2PrefabListDict.ContainsKey(file))
            {
                result.Add(file);
            }
        }
        return result;
    }

    HashSet<string> GetUnReferenceImageSetWithIgnoreDirectory(List<string> ignoreaPaths)
    {
        List<string> ToLocalPath(List<string> paths)
        {
            List<string> result2 = new List<string>();
            foreach (var item in paths)
            {
                int index = item.IndexOf("Assets/");
                if(index > 0)
                {
                    result2.Add(item.Substring(index));
                }
            }
            return result2;
        }

        List<string> localPaths = ToLocalPath(ignoreaPaths);
        HashSet<string> result = new HashSet<string>();

        foreach (var file in m_unReferenceImageSet)
        {
            bool ignore = false;
            foreach (var path in localPaths)
            {
                if (file.StartsWith(path))
                {
                    ignore = true;
                    break;
                }
            }
            if(! ignore)
            {
                result.Add(file);
            }
        }
        return result;
    }

    void Delete()
    {
        foreach (var file in m_unReferenceImageWithIgnoreDirectorySet)
        {
            if (File.Exists(file))
                File.Delete(file);
            
            if (File.Exists($"{file}.meta"))
                File.Delete($"{file}.meta");
        }
    }


    void MoveWaitToDeleteImageIntoDirectoty(string dir)
    {
        if(Directory.Exists(dir))
        {
            foreach (var file in m_unReferenceImageWithIgnoreDirectorySet)
            {
                if(File.Exists(file))
                {
                    int lastSybolIndex = file.LastIndexOf('/');
                    if(lastSybolIndex > 0)
                    {
                        string fileReplaceName = file;
                        string destPath = $"{dir}/image_{fileReplaceName.Replace('/','_')}";
                        File.Copy(file, destPath);
                    }
                }
            }
        }
    }

    void OutputSameTexture()
    {
        Dictionary<string, List<string>> dicTextureMd5 = new Dictionary<string, List<string>>();

        List<string> fileList = null;

        int nCount = m_processImageDirectories.Count;
        for(int i=0;i<nCount;++i)
        {
            string[] Paths = Directory.GetFiles(m_processImageDirectories[i], "*.png", SearchOption.AllDirectories);

            for(int j=0;j< Paths.Length;++j)
            {
                string strMd5 = MD5.GetMD5(Paths[j]);
                if(dicTextureMd5.TryGetValue(strMd5,out fileList)==false)
                {
                    fileList = new List<string>();
                    dicTextureMd5.Add(strMd5, fileList);
                }

                fileList.Add(Paths[j]);
            }
        }


        string filePath = Application.dataPath + "/same_texture.csv";

        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }


        FileStream fs = File.Create(filePath);
        StreamWriter sw = new StreamWriter(fs);


        string content = "";
        foreach (var PathList in dicTextureMd5.Values)
        {
            if(PathList.Count>2)
            {
                content = "";
                foreach (string item in PathList)
                {
                    content += item.Substring(item.IndexOf("/Assets/")+8) + ",";
                }

                sw.WriteLine(content);
            }
        }

        sw.Flush();
        sw.Close();
        fs.Close();

    }

}
