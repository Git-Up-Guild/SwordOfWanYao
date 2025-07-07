using XGameEditor.ResourceTools;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace XGameEditor.AssetImportTool
{
    public class SpriteSetting
    {
        static private string[] s_arySpriteDir = new string[] { /*"G_Resources/Artist/Icon, */ "G_Resources/Artist/UI", "G_Resources/Artist/UI1.0", "G_Resources/Artist/UI_I18N" };

        //打包所有图集
        public static void DoSetSpriteAltas(HashSet<string> targetABSet = null)
        {
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            var folderList = NewGAssetBuildConfig.Instance.spriteAltasDirList;
            for (int i = 0; i < folderList.Count; i++)
            {
                SpriteAtlasChecker.CreateAtlasByDir(folderList[i], true, false, true, targetABSet);
            }
            AssetDatabase.Refresh();
        }

        //设置打包图集的目录列表
        static public void SetupBuildSpriteAltasCfg()
        {
            int nLen = s_arySpriteDir.Length;
            for(int i=0;i<nLen;++i)
            {
                SetTagByDir(Application.dataPath+"/"+s_arySpriteDir[i]);
            }
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOn;
        }

        //设置单个目录的tag
        static public void ClearTagByDirByConfig()
        {
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            var folderList = NewGAssetBuildConfig.Instance.spriteAltasDirList;
            for (int k = 0; k < folderList.Count; k++)
            {
                string dir = folderList[k];
                if (Directory.Exists(dir) == false)
                {
                    Debug.LogError("不存在的图集目录:" + dir);
                    continue;
                }

                string[] filePaths = Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories);
                int nLen = filePaths.Length;
                for (int i = 0; i < nLen; ++i)
                {
                    filePaths[i] = filePaths[i].Replace("\\", "/");
                    string relPath = filePaths[i].Substring(filePaths[i].LastIndexOf("Assets/"));
                    TextureImporter texture = AssetImporter.GetAtPath(relPath) as TextureImporter; //通过路径得到资源
                    if (texture != null && !string.IsNullOrEmpty(texture.spritePackingTag))
                    {
                        texture.spritePackingTag = "";
                        AssetDatabase.ImportAsset(relPath);
                    }
                    else
                    {
                        Debug.LogError("清除传统图集打包 Tag 失败！path=" + relPath);
                    }
                }
            }
            AssetDatabase.Refresh();
        }

        //设置单个目录的tag
        static public void SetTagByDir(string dir)
        {

            if(Directory.Exists(dir)==false)
            {
                Debug.LogError("不存在的图集目录:" + dir);
                return;
            }

            string []filePaths = Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories);
            int nLen = filePaths.Length;
            for(int i=0;i<nLen;++i)
            {

                filePaths[i] = filePaths[i].Replace("\\", "/");
                string relPath = filePaths[i].Substring(filePaths[i].LastIndexOf("Assets/"));
                TextureImporter texture = AssetImporter.GetAtPath(relPath) as TextureImporter; //通过路径得到资源

                if(texture != null)
                {
                    string dirName = Path.GetDirectoryName(filePaths[i]);
                    string packName = dirName.Substring(dirName.LastIndexOf("Artist") + 7);
                    packName = packName.Replace("\\", "/");
                    packName = packName.Replace("/", "_");
                    texture.spritePackingTag = packName;
                    AssetDatabase.ImportAsset(relPath);
                }
                else
                {
                    Debug.LogError("SetTagByDir 失败！path=" + relPath);
                }


            }

        }

    }
}
