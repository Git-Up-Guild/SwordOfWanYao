using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using ReferenceFinder;
using UnityEngine.UI;
using XGame;
using TMPro;
using XGame.I18N;

public class CustomImageReplace : EditorWindow
{
    static private BuildinResourcesConfig m_BuildInResource;


    string m_detectPrefabPath;
    List<Texture2D> m_replaceTexList;


    List<Font> m_fontList;
    List<TMP_FontAsset> m_tmpFontList;

    //替换材质
    List<Material> m_matList;


    string m_strSrcDir = "";
    string m_strDstDir = "";

    [MenuItem("XGame/资源工具/手动替换预制体相同依赖图片")]
    static void ShowWindow()
    {
        GetWindow<CustomImageReplace>();
    }

    public CustomImageReplace()
    {
        this.titleContent = new GUIContent("自定义替换预制体相同依赖图片");
        this.minSize = new Vector2(750f, 512f);
    }

    private void OnEnable()
    {
        m_replaceTexList = new List<Texture2D>();
        ReferenceFinderData.CollectDependenciesInfo();

        m_fontList = new List<Font>();
        m_fontList.Add(null);
        m_fontList.Add(null);

        m_tmpFontList = new List<TMP_FontAsset>();
        m_tmpFontList.Add(null);
        m_tmpFontList.Add(null);

        m_matList = new List<Material>();
        m_matList.Add(null);
        m_matList.Add(null);


    }






    private void OnGUI()
    {
        if(GUILayout.Button("增加相同图片（+）"))
        {
            m_replaceTexList.Add(null);
        }
        GUILayout.Space(16f);
        for (int i = 0; i < m_replaceTexList.Count; i++)
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                m_replaceTexList[i] = EditorGUILayout.ObjectField(m_replaceTexList[i], typeof(Texture2D), true) as Texture2D;
                if (i == 0)
                    GUILayout.Label("替换目标图片（其余图片将会被替换成这张图片）");
                if (GUILayout.Button("移除", EditorStyles.miniButtonRight, GUILayout.Height(20f), GUILayout.Width(64f)))
                {
                    m_replaceTexList.RemoveAt(i);
                }
            }

        }

        if (GUILayout.Button("手动替换图片"))
        {
            List<string> allTexPath = new List<string>();
            for (int i = 0; i < m_replaceTexList.Count; i++)
            {
                allTexPath.Add(AssetDatabase.GetAssetPath(m_replaceTexList[i]));
            }

            CustomReplaceResource(allTexPath);
            AssetDatabase.Refresh();
            ReferenceFinderData.CollectDependenciesInfo();
        }


        if (GUILayout.Button("替换内置资源"))
        {
            CustomReplaceSameImageEx();
            AssetDatabase.Refresh();
            ReferenceFinderData.CollectDependenciesInfo();
        }




        


        GUILayout.Space(16f);
        DrawFolderEdit("源资源目录", ref m_strSrcDir);
        DrawFolderEdit("目的资源目录", ref m_strDstDir);

        if (GUILayout.Button("替换目录资源(同名资源)"))
        {
            ReplaceDir(m_strSrcDir,m_strDstDir);
            AssetDatabase.Refresh();
            ReferenceFinderData.CollectDependenciesInfo();
        }

        if (GUILayout.Button("替换目录资源(相同MD5资源)"))
        {
            ReplaceDirMD5(m_strSrcDir, m_strDstDir);
            AssetDatabase.Refresh();
            ReferenceFinderData.CollectDependenciesInfo();
        }


        if (GUILayout.Button("删除空文件夹"))
        {
            ClearEmptyDir(m_strSrcDir);
        }


        //渲染原始字体列表
        for (int i = 0; i < m_fontList.Count; i++)
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                m_fontList[i] = EditorGUILayout.ObjectField(m_fontList[i], typeof(Font), true) as Font;
               
            }
        }

        //替换原始字体
        if (GUILayout.Button("替换原始字体"))
        {
            List<string> allTexPath = new List<string>();
            for (int i = 0; i < m_fontList.Count; i++)
            {
                allTexPath.Add(AssetDatabase.GetAssetPath(m_fontList[i]));
            }

            CustomReplaceResource(allTexPath);

           // CustomReplaceFont();
            AssetDatabase.Refresh();
            ReferenceFinderData.CollectDependenciesInfo();
        }

        //渲染原始字体列表
        for (int i = 0; i < m_tmpFontList.Count; i++)
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                m_tmpFontList[i] = EditorGUILayout.ObjectField(m_tmpFontList[i], typeof(TMP_FontAsset), true) as TMP_FontAsset;

            }
        }

        //替换原始字体
        if (GUILayout.Button("替换TMP字体"))
        {

            List<string> allTexPath = new List<string>();
            for (int i = 0; i < m_tmpFontList.Count; i++)
            {
                allTexPath.Add(AssetDatabase.GetAssetPath(m_tmpFontList[i]));
            }

            CustomReplaceResource(allTexPath);

            //CustomReplaceTMPFont();
            AssetDatabase.Refresh();
            ReferenceFinderData.CollectDependenciesInfo();
        }

        //画材质UI
        for (int i = 0; i < m_matList.Count; i++)
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                m_matList[i] = EditorGUILayout.ObjectField(m_matList[i], typeof(Material), true) as Material;

            }
        }

        //替换原始字体
        if (GUILayout.Button("替换材质"))
        {

            List<string> allTexPath = new List<string>();
            for (int i = 0; i < m_matList.Count; i++)
            {
                allTexPath.Add(AssetDatabase.GetAssetPath(m_matList[i]));
            }

            CustomReplaceResource(allTexPath,false);

            //CustomReplaceTMPFont();
            AssetDatabase.Refresh();
            ReferenceFinderData.CollectDependenciesInfo();
        }


    }

    void SaveTextureToFile(Texture2D texture , string fileName)
    {
        byte[] bytes = null;
        if (texture.isReadable)
        {
            bytes = texture.EncodeToPNG();
        }else
        {
            RenderTexture renderTexture = new RenderTexture(texture.width, texture.height,0,RenderTextureFormat.ARGB32);
            Graphics.Blit(texture, renderTexture);


            int width = renderTexture.width;
            int height = renderTexture.height;
            Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture2D.Apply();
            bytes = texture2D.EncodeToPNG();

        }
        
        
     
        File.WriteAllBytes(Application.dataPath + "/" + fileName,bytes);
    }

    void SavePrafeb(GameObject go,string path)
    {
        string[] oldContents = File.ReadAllLines(path);

        PrefabUtility.SaveAsPrefabAsset(go, path);

        //先刷新，unity自动改属性
        AssetDatabase.Refresh();

        string[] newContents = File.ReadAllLines(path);
        for(int i=0;i< oldContents.Length;++i)
        {
            if(oldContents[i].IndexOf("m_Sprite")>=0)
            {
                //只替换有sprite选项的
                oldContents[i] = newContents[i];
            }
        }

        File.WriteAllLines(path, oldContents);
    }


    void CustomReplaceSameImageEx()
    {
        if (null == m_BuildInResource)
        {
            string path = "Assets/XGameEditor/Editor/AssetsTool/ImageOptimizeRelation/BuildInResources.asset";
            m_BuildInResource = AssetDatabase.LoadAssetAtPath<BuildinResourcesConfig>(path);
        }

        if (null == m_BuildInResource)
        {
            Debug.LogError("null== m_BuildInResource");
            return;
        }


        string ReplaceFold = "G_Resources/Artist/UI2.0/Common/BuiltinExtraResource";
        string[] filePaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);

        
        List<Texture2D> listBuildin = m_BuildInResource.m_listBuildinResources;


        for (int j=0;j< listBuildin.Count;++j)
        {
            for (int i = 0; i < filePaths.Length; ++i)
            {
                GameObject go = PrefabUtility.LoadPrefabContents(filePaths[i]);
                Image[] imgs = go.GetComponentsInChildren<Image>();
                bool bModified = false;
                for (int n = 0; n < imgs.Length; ++n)
                {
                    if (imgs[n].sprite && imgs[n].sprite.texture == listBuildin[j])
                    {
                        string replacePath = ReplaceFold + "/" + imgs[n].sprite.name + ".png";
                        if (File.Exists(Application.dataPath + "/" + replacePath) == false)
                        {
                            SaveTextureToFile(imgs[n].sprite.texture, replacePath);
                        }
                        imgs[n].sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/"+replacePath);

                        Debug.LogError(filePaths[i]+"替换:" + imgs[n].gameObject.name+"替换的图:"+ imgs[n].sprite.name);

                        bModified = true;
                    }
                }

                if (bModified)
                {
                    PrefabUtility.SaveAsPrefabAsset(go, filePaths[i]);
                    //SavePrafeb(go, filePaths[i]);
                   
                }

                PrefabUtility.UnloadPrefabContents(go);
            }
        }



       
    }


    void CustomReplaceResource(List<string> allTexPath,bool bDeleOld = true)
    {

        HashSet<GameObject> relationPrefabs = new HashSet<GameObject>();
        for (int i = 1; i < allTexPath.Count; i++)
        {
            string oldTexGuid = AssetDatabase.AssetPathToGUID(allTexPath[i]);
            var data = ReferenceFinderData.Get(oldTexGuid);
            if (data != null)
            {
                foreach (var referenceGuid in data.references)
                {
                    var referencePath = AssetDatabase.GUIDToAssetPath(referenceGuid);
                    if (!referencePath.EndsWith(".prefab"))
                        continue;

                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(referencePath);
                    relationPrefabs.Add(prefab);
                }
            }
        }


        string newGUID = AssetDatabase.AssetPathToGUID(allTexPath[0]);
        
        List<string> oldGUIDList = new List<string>();
        for (int i = 1; i < allTexPath.Count; i++)
        {
            oldGUIDList.Add(AssetDatabase.AssetPathToGUID(allTexPath[i]));
        }

        string[] oldGUIDs = oldGUIDList.ToArray();

        foreach (var prefab in relationPrefabs)
        {
            ImageOptimizeUtility.ReplacePrefabDependencies(prefab, oldGUIDs, newGUID);
        }


        string msg = "可能影响到的预制体：";
        foreach (var item in relationPrefabs)
        {
            msg +=  $"{item.name},";
        }
        Debug.Log(msg);


        //删除掉旧依赖及其meta文件
        if (bDeleOld)
        {
            for (int i = 1; i < allTexPath.Count; i++)
            {
                if (allTexPath[i] == allTexPath[0])
                {
                    continue;
                }

                if (File.Exists(allTexPath[i]))
                    File.Delete(allTexPath[i]);
                if (File.Exists($"{allTexPath[i]}.meta"))
                    File.Delete($"{allTexPath[i]}.meta");
            }
        }
       
       
    }




    void CustomReplaceFont()
    {
        string[] filePaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);


        for (int i = 0; i < filePaths.Length; ++i)
        {
            bool bModified = false;
            GameObject go = PrefabUtility.LoadPrefabContents(filePaths[i]);
            
            // 替换LocalizeTextMeshPro
            Text [] tmpFonts = go.GetComponentsInChildren<Text>();
            for (int n = 0; n < tmpFonts.Length; ++n)
            {
                if (tmpFonts[n].font == m_fontList[0])
                {
                    tmpFonts[n].font = m_fontList[1];
                    Debug.LogError(filePaths[i] + "替换:" + tmpFonts[n].gameObject.name + "替换的图:" + tmpFonts[n].name);

                    bModified = true;
                }

            }
            
            if (bModified)
            {
                PrefabUtility.SaveAsPrefabAsset(go, filePaths[i]);
                //SavePrafeb(go, filePaths[i]);

            }

            PrefabUtility.UnloadPrefabContents(go);
        }
    }


        void CustomReplaceTMPFont()
        {
           

            string[] filePaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);


        for (int i = 0; i < filePaths.Length; ++i)
        {
            bool bModified = false;
            GameObject go = PrefabUtility.LoadPrefabContents(filePaths[i]);

            // 替换LocalizeTextMeshPro
            LocalizeTextMeshPro[] tmpFonts = go.GetComponentsInChildren<LocalizeTextMeshPro>();
            for (int n = 0; n < tmpFonts.Length; ++n)
            {
                if (tmpFonts[n].font== m_tmpFontList[0])
                {
                    tmpFonts[n].font = m_tmpFontList[1];
                    tmpFonts[n].fontSharedMaterial = m_tmpFontList[1].material;
                    tmpFonts[n].material = m_tmpFontList[1].material;

                    Debug.LogError(filePaths[i] + "替换:" + tmpFonts[n].gameObject.name);

                    bModified = true;
                }
                
            }


            // 替换LocalizeTextMeshPro
            TextMeshProUGUI[] tmpMeshFonts = go.GetComponentsInChildren<TextMeshProUGUI>();
            for (int n = 0; n < tmpMeshFonts.Length; ++n)
            {
                if (tmpMeshFonts[n].font == m_tmpFontList[0])
                {
                    tmpMeshFonts[n].font = m_tmpFontList[1];
                    tmpMeshFonts[n].fontSharedMaterial = m_tmpFontList[1].material;
                    tmpMeshFonts[n].material = m_tmpFontList[1].material;
                    Debug.LogError(filePaths[i] + "替换:" + tmpMeshFonts[n].gameObject.name );

                    bModified = true;
                }

            }


            if (bModified)
            {
                EditorUtility.SetDirty(go);
                PrefabUtility.SaveAsPrefabAsset(go, filePaths[i]);
                //SavePrafeb(go, filePaths[i]);

            }

                PrefabUtility.UnloadPrefabContents(go);
            }


    }

    void ReplaceDir(string src,string dst)
    {
        if(Directory.Exists(src) ==false)
        {
            Debug.LogError("目录不存在：" + src);
            return;
        }

        if (Directory.Exists(src) == false)
        {
            Debug.LogError("目录不存在：" + dst);
            return;
        }

        //获取所有源资源
        string[] srcFiles = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories);
        string srcPath = null;
        string srcName = null;
        List<string> listReplace = new List<string>();
        for (int i=0;i< srcFiles.Length;++i)
        {
            if(srcFiles[i].EndsWith(".meta"))
            {
                continue;
            }

            int nIndex = srcFiles[i].IndexOf("Assets/");
            srcPath = srcFiles[i].Substring(nIndex);
            srcName = Path.GetFileName(srcPath);

            string[] dstFiles = Directory.GetFiles(dst, "*"+ srcName, SearchOption.AllDirectories);
            if(dstFiles.Length>0)
            {
                listReplace.Clear();
                listReplace.Add(srcPath);
                for (int j = 0; j < dstFiles.Length; ++j)
                {
                     nIndex = dstFiles[j].IndexOf("Assets/");
                    listReplace.Add(dstFiles[j].Substring(nIndex));
                }

                CustomReplaceResource(listReplace);
            }

           
        }


    }

    void ReplaceDirMD5(string src, string dst)
    {
        if (Directory.Exists(src) == false)
        {
            Debug.LogError("目录不存在：" + src);
            return;
        }

        if (Directory.Exists(src) == false)
        {
            Debug.LogError("目录不存在：" + dst);
            return;
        }

        //获取所有源资源
        string[] srcFiles = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories);
        string srcPath = null;
        List<string> listReplace = new List<string>();
        Dictionary<string, string> dicMd5 = new Dictionary<string, string>();
        for (int i = 0; i < srcFiles.Length; ++i)
        {
            if (srcFiles[i].EndsWith(".meta"))
            {
                continue;
            }

           
            int nIndex = srcFiles[i].IndexOf("Assets");
            if(nIndex<0)
            {
                Debug.LogError("无法处理(不是Assets目录下) ：" + srcFiles[i]);
                continue;
            }
            srcPath = srcFiles[i].Substring(nIndex);
            string md5 = MD5.GetMD5(srcFiles[i]);
            if(dicMd5.ContainsKey(md5)==false)
            {
                dicMd5.Add(MD5.GetMD5(srcFiles[i]), srcPath);
            }
            else
            {
                Debug.LogError("重复资源:" + srcPath + "    old path =" + dicMd5[md5]);
            }
           

        }

        string[] dstFiles = Directory.GetFiles(dst, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < dstFiles.Length; ++i)
        {
            if (dstFiles[i].EndsWith(".meta"))
            {
                continue;
            }

            string md5 = MD5.GetMD5(dstFiles[i]);
            if(dicMd5.ContainsKey(md5))
            {
                listReplace.Clear();
                listReplace.Add(dicMd5[md5]);
                int nIndex = dstFiles[i].IndexOf("Assets");
                listReplace.Add(dstFiles[i].Substring(nIndex));
                CustomReplaceResource(listReplace);
            }
        }


    }

    //清理空的文件夹
    private void ClearEmptyDir(string src)
    {
        if (Directory.Exists(src) == false)
        {
            Debug.LogError("目录不存在：" + src);
            return;
        }

        string[] dirs = Directory.GetDirectories(src, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; ++i)
        {
            if(Directory.Exists(dirs[i])==false)
            {
                continue;
            }

            string[] files = Directory.GetFiles(dirs[i], "*.*", SearchOption.AllDirectories);
            bool bInEmpty = true;
            for(int j=0;j< files.Length;++j)
            {
                if(files[j].IndexOf(".meta")>=0)
                {
                    continue;
                }

                bInEmpty = false;
                break;
            }

            if(bInEmpty)
            {
                Directory.Delete(dirs[i],true);
            }
        }
    }

    private void DrawFolderEdit(string label, ref string result, bool isFolder = true, bool isCheckExists = true)
    {
        using (var hor = new GUILayout.HorizontalScope())
        {
            result = EditorGUILayout.TextField(label, result);
            if (GUILayout.Button("浏览"))
            {
                result = isFolder ? EditorUtility.OpenFolderPanel(label, result, "") :
                    EditorUtility.OpenFilePanel(label, result, "");
            }
            if (isCheckExists && ((isFolder && !Directory.Exists(result)) || (!isFolder && !File.Exists(result))))
            {
                GUILayout.Box("", "Wizard Error", GUILayout.Width(32), GUILayout.Height(16));
            }
        }

    }


}



