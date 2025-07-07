/*******************************************************************
** 文件名: SpriteAtlasChecker.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 郑袖长
** 日  期: 2021/xx/xx
** 版  本: 1.0
** 描  述: 
** 应  用: 
**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TextureFMT;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using XGame.Utils;
using XGameEditor.AssetImportTool;
using Object = UnityEngine.Object;

namespace XGameEditor.ResourceTools
{
    public class SpriteAtlasChecker
    {
        private static List<string> postfixSp = new List<string>() { ".png", ".jpg" };
        private static string spAtlasPostfix = ".spriteatlas";
        private readonly Type foldType = typeof(DefaultAsset);

        [MenuItem("XGame/图集工具/生成所有图集", priority = 101)]
        public static void CreateAllAtlas()
        {
            SpriteSetting.DoSetSpriteAltas();
        }

        [MenuItem("XGame/图集工具/生成图标图集", priority = 102)]
        public static void CreateIconAtlas()
        {
            CreateAtlasByDir("Assets/G_Resources/Artist/Icon", true, false, true);
        }

        [MenuItem("XGame/图集工具/生成UI公共图集", priority = 103)]
        public static void CreateUICommonAtlas()
        {
            //CreateAtlasByDir("Assets/G_Resources/Artist/UI/Common", true, false, true);
            //CreateAtlasByDir("Assets/G_Resources/Artist/UI1.0/Common", true, false, true);
            CreateAtlasByDir("Assets/G_Resources/Artist/UI3.0/Common", true, false, true);
        }

        [MenuItem("XGame/图集工具/清除传统打包Tag", priority = 104)]
        public static void ClearLegacyPackerTag()
        {
            SpriteSetting.ClearTagByDirByConfig();
        }

        [MenuItem("Assets/图集工具/创建图集/选中文件夹下创建一个总图集(包含所有子文件夹)", priority = 101)]
        public static void CreateAtlasByAllDirectory()
        {
            DefaultAsset[] selectObjs = GetSelectAsset<DefaultAsset>(SelectionMode.Assets);
            foreach (var obj in selectObjs)
            {
                string dirPath = AssetDatabase.GetAssetPath(obj);
                CreateAtlasByTopDir(dirPath);
            }
        }

        [MenuItem("Assets/图集工具/创建图集/创建选中文件夹图集(不包含子文件夹)", priority = 102)]
        public static void CreateAtlasByTopDirectory()
        {
            CreateSelectDirectoryAtlas(false);
        }

        [MenuItem("Assets/图集工具/创建图集/递归创建选中文件夹图集(包含子文件夹)", priority = 103)]
        public static void CreateAtlasByDirectory()
        {
            CreateSelectDirectoryAtlas(true);
        }

        [MenuItem("Assets/图集工具/创建图集/递归创建选中文件夹图集(包含子文件夹，按目录，刷新)", priority = 104)]
        public static void CreateAtlasByDirectoryWithFolder()
        {
            CreateSelectDirectoryAtlas(true, true, true);
        }

        [MenuItem("Assets/图集工具/删除图集/删除选中文件夹图集(不包含子文件夹)", priority = 105)]
        public static void DeleteAtlasByTopDirectory()
        {
            DeleteSelectDirectoryAtlas(false);
        }

        [MenuItem("Assets/图集工具/删除图集/递归删除选中文件夹图集(包含子文件夹)", priority = 106)]
        public static void DeleteAtlasByDirectory()
        {
            DeleteSelectDirectoryAtlas(true);
        }

        //检查文件夹是否都打了图集
        [MenuItem("Assets/图集工具/检查图集/递归检查选中文件夹是否打了图集(包含子文件夹)", priority = 107)]
        public static void CheckDirectoryAtlas()
        {
            CheckSelectDirectory(true);
        }

        [MenuItem("Assets/图集工具/检查图集/检查选中文件夹是否打了图集(不包含子文件夹)", priority = 108)]
        public static void CheckTopDirectoryAtlas()
        {
            CheckSelectDirectory(false);
        }

        [MenuItem("Assets/图集工具/检查图集/检查选中文件夹是否有精灵未打进图集(包含子文件夹)", priority = 109)]
        public static void CheckDirResInAtlas()
        {
            CheckSelectDirectoryResInAtlas();
        }

        [MenuItem("Assets/图集工具/刷新图集/递归刷新选中文件夹图集(包含子文件夹)", priority = 110)]
        public static void RefreshDirectoryAtlas()
        {
            RefreshSelectDirectory(true, true);
        }

        [MenuItem("Assets/图集工具/刷新图集/刷新选中文件夹图集(不包含子文件夹)", priority = 111)]
        public static void RefreshTopDirectoryAtlas()
        {
            RefreshSelectDirectory(false, false);
        }

        [MenuItem("Assets/图集工具/打包图集/重新打包工程所有图集", priority = 110)]
        public static void RePackAllAtlas()
        {
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, true);
            Debug.Log("重新打包图集完毕！");
        }

        [MenuItem("Assets/图集工具/打包图集/重新打包选中文件夹图集(包含子文件夹)", priority = 112)]
        public static void RePackSelectAtlas()
        {
            RePackSelectDirectory(true);
        }

        [MenuItem("Assets/图集工具/检查图集同名资源/检查当前选中文件夹(不包含子文件夹)", priority = 113)]
        public static void CheckTopSameNameRes()
        {
            CheckSelectDirResWithSameName(false);
        }

        [MenuItem("Assets/图集工具/检查图集同名资源/递归检查当前选中文件夹(包含子文件夹)", priority = 114)]
        public static void CheckSameNameRes()
        {
            CheckSelectDirResWithSameName(true);
        }

        //验证方法
        [MenuItem("Assets/图集工具/创建图集/选中文件夹下创建一个总图集(包含所有子文件夹)", true)]
        [MenuItem("Assets/图集工具/创建图集/创建选中文件夹图集(不包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/创建图集/递归创建选中文件夹图集(包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/删除图集/删除选中文件夹图集(不包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/删除图集/递归删除选中文件夹图集(包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/检查图集/递归检查选中文件夹是否打了图集(包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/检查图集/检查选中文件夹是否打了图集(不包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/刷新图集/递归刷新选中文件夹图集(包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/刷新图集/刷新选中文件夹图集(不包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/打包图集/重新打包选中文件夹图集(包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/检查图集同名资源/检查当前选中文件夹(不包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/检查图集同名资源/递归检查当前选中文件夹(包含子文件夹)", true)]
        [MenuItem("Assets/图集工具/检查图集/检查选中文件夹是否有精灵未打进图集(包含子文件夹)", true)]
        private static bool IsMenuActive()
        {
            DefaultAsset[] selectObjs = GetSelectAsset<DefaultAsset>(SelectionMode.Assets);
            return selectObjs.Length > 0;
        }

        private static void CreateSelectDirectoryAtlas(bool recursive, bool isFolder = false, bool isUpdate = false)
        {
            DefaultAsset[] selectObjs = GetSelectAsset<DefaultAsset>(SelectionMode.Assets);
            foreach (var obj in selectObjs)
            {
                string dirPath = AssetDatabase.GetAssetPath(obj);
                CreateAtlasByDir(dirPath, recursive, isFolder, isUpdate);
            }
        }

        private static void DeleteSelectDirectoryAtlas(bool recursive)
        {
            DefaultAsset[] selectObjs = GetSelectAsset<DefaultAsset>(SelectionMode.Assets);
            foreach (var obj in selectObjs)
            {
                string dirPath = AssetDatabase.GetAssetPath(obj);
                DeleteAtlasByDir(dirPath, recursive);
            }
        }

        private static void RefreshSelectDirectory(bool recursive, bool isFolder)
        {
            DefaultAsset[] selectObjs = GetSelectAsset<DefaultAsset>(SelectionMode.Assets);
            foreach (var obj in selectObjs)
            {
                string dirPath = AssetDatabase.GetAssetPath(obj);
                RefreshAtlasByDir(dirPath, recursive, isFolder);
            }
        }

        private static void CheckSelectDirectory(bool recursive)
        {
            DefaultAsset[] selectObjs = GetSelectAsset<DefaultAsset>(SelectionMode.Assets);
            foreach (var obj in selectObjs)
            {
                string dirPath = AssetDatabase.GetAssetPath(obj);
                CheckDirHasAtlas(dirPath, recursive);
            }
            Debug.Log("图集检查完毕！");
        }

        private static void CheckSelectDirectoryResInAtlas()
        {
            SelectionMode mode = SelectionMode.DeepAssets;
            List<string> allAtlasSpHash = new List<string>();

            SpriteAtlas[] arrAtlas = GetSelectAsset<SpriteAtlas>(mode);
            foreach (var item in arrAtlas)
            {
                Sprite[] sprites = new Sprite[item.spriteCount];
                item.GetSprites(sprites);

                foreach (var sp in sprites)
                {
                    Texture2D texture = sp.texture;
                    int instID = texture.GetInstanceID();
                    string path = AssetDatabase.GetAssetPath(instID);
                    if (!allAtlasSpHash.Contains(path))
                        allAtlasSpHash.Add(path);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("未加入图集资源如下：");

            bool isHas = false;
            Texture2D[] arrSprites = GetSelectAsset<Texture2D>(mode);
            foreach (var item in arrSprites)
            {
                string path = AssetDatabase.GetAssetPath(item);
                if (!allAtlasSpHash.Contains(path))
                {
                    isHas = true;
                    sb.AppendLine(AssetDatabase.GetAssetPath(item));
                }
            }

            if (!isHas) sb.Append("无");
            Debug.Log(sb.ToString());
        }

        private static void RePackSelectDirectory(bool recursive = true)
        {
            SelectionMode mode = recursive ? SelectionMode.DeepAssets : SelectionMode.Assets;
            SpriteAtlas[] selectObjs = GetSelectAsset<SpriteAtlas>(mode);
            SpriteAtlasUtility.PackAtlases(selectObjs, EditorUserBuildSettings.activeBuildTarget);

            string szAtlas = "";
            foreach (var item in selectObjs)
            {
                szAtlas += $"{item.name}{spAtlasPostfix};  ";
            }
            Debug.Log("重新打包图集完毕: " + szAtlas);
        }

        private static void CheckSelectDirResWithSameName(bool recursive)
        {
            DefaultAsset[] selectObjs = GetSelectAsset<DefaultAsset>(SelectionMode.Assets);
            foreach (var item in selectObjs)
            {
                string dirPath = AssetDatabase.GetAssetPath(item);
                CheckSameNameResInAtlas(dirPath, recursive);
            }

            Debug.Log("图集资源检查完毕! ");
        }

        private static void CheckSameNameResInAtlas(string path, bool recursive)
        {
            string[] arrAtlas = Directory.GetFiles(path, "*" + spAtlasPostfix, SearchOption.TopDirectoryOnly);
            StringBuilder sb = new StringBuilder();
            Dictionary<string, Object> recordDic = new Dictionary<string, Object>();    //name - obj

            foreach (var item in arrAtlas)
            {
                SpriteAtlas sa = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(item);
                if (sa)
                {
                    bool bHas = false;
                    recordDic.Clear();
                    sb.Append($"图集【{sa.name}】同名资源：");
                    Object[] arrSp = sa.GetPackables();
                    foreach (var sp in arrSp)
                    {
                        if (recordDic.ContainsKey(sp.name))
                        {
                            sb.Append($"{sp.name}, ");
                            bHas = true;
                        }
                        else
                        {
                            recordDic.Add(sp.name, sp);
                        }
                    }
                    if (bHas)
                        sb.AppendLine();
                    else
                        sb.AppendLine("无");
                }
            }
            Debug.Log(sb.ToString());

            if (recursive)
            {
                List<string> arrDirs = new List<string>();
                GetDirectories(path, out arrDirs, null, false);
                foreach (var item in arrDirs)
                {
                    CheckSameNameResInAtlas(item, recursive);
                }
            }
        }

        private static void CheckSelectDirResWithSameGUID(bool recursive)
        {
            SelectionMode mode = recursive ? SelectionMode.DeepAssets : SelectionMode.Assets;
            SpriteAtlas[] selectObjs = GetSelectAsset<SpriteAtlas>(mode);

            StringBuilder sb = new StringBuilder();
            Dictionary<string, Object> recordDic = new Dictionary<string, Object>();    //name - obj
            foreach (var sa in selectObjs)
            {
                recordDic.Clear();
                sb.AppendLine($"图集【{sa.name}】同名资源：");
                Object[] arrSp = sa.GetPackables();
                foreach (var item in arrSp)
                {
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(item));
                    if (recordDic.ContainsKey(guid))
                    {

                    }
                    else
                    {
                        recordDic.Add(guid, item);
                    }
                }
            }
            Debug.Log("图集资源检查完毕！");
        }

        private static void CheckDirHasAtlas(string path, bool recursive)
        {
            List<string> arrSprites = new List<string>();
            GetFiles(path, out arrSprites, null, postfixSp, false);
            if (arrSprites.Count > 0)
            {
                string[] arrAtlas = Directory.GetFiles(path, "*.spriteatlas", SearchOption.TopDirectoryOnly);
                if (arrAtlas.Length <= 0)
                {
                    Debug.LogError($"文件夹：{path} >> 未包含图集：");
                }
            }
            else
            {
                Debug.Log("此路径下无精灵：" + path);
            }

            if (recursive)
            {
                List<string> arrDirs = new List<string>();
                GetDirectories(path, out arrDirs, null, false);
                foreach (var item in arrDirs)
                {
                    CheckDirHasAtlas(item, recursive);
                }
            }
        }

        public static void DeleteAtlasByDir(string path)
        {
            //先删除老的图集
            string[] arrAtlas = Directory.GetFiles(path, "*" + spAtlasPostfix, SearchOption.AllDirectories);
            for (int i = 0; i < arrAtlas.Length; ++i)
            {
                if (File.Exists(arrAtlas[i]))
                {
                    File.Delete(arrAtlas[i]);
                }
            }
        }

        public static bool CheckModified(string path, bool isFolder, HashSet<string> targetABSet)
        {
            if (null == targetABSet)
            {
                return true;
            }

            if (targetABSet.Count == 0)
            {
                return false;
            }


            SearchOption op = isFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] files = Directory.GetFiles(path, "*.*", op);
            for (int i = 0; i < files.Length; ++i)
            {
                if (targetABSet.Contains(files[i]))
                {
                    return true;
                }
            }

            return false;

        }


        //创建图集，每个文件夹一个图集
        public static void CreateAtlasByDir(string path, bool recursive, bool isFolder = false, bool isUpdate = false, HashSet<string> targetABSet = null)
        {
            if(!Directory.Exists(path))
            {
                Debug.LogError($"找不到设置的图集文件夹 {path}");
                return;
            }

            //目录给加上/防止那些 Common开头的文件夹处理出错
            path = path.Replace('\\', '/');
            if(!path.EndsWith("/"))
            {
                path = path + "/";
            }

            //先把多余的图集删除掉。这里不再每次都删除所有的图集，而是删除多余的图集，
            //避免每次都生成新的图集从而造成更新时包体大的问题
            bool bDelAtlas = false;
            string[] arrAtlas = Directory.GetFiles(path, "*" + spAtlasPostfix, SearchOption.AllDirectories);
            var folderList = NewGAssetBuildConfig.Instance.spriteAltasSubDirList;
            for (int i = 0; i < arrAtlas.Length; ++i)
            {
                string atlasPathTmp = arrAtlas[i];
                atlasPathTmp = atlasPathTmp.Replace('\\', '/');
                string atlasDir = atlasPathTmp.Substring(0, atlasPathTmp.LastIndexOf("/"));
                for (int j = 0; j < folderList.Count; j++)
                {
                    string folderDir = folderList[j];
                    if (atlasDir.IndexOf(folderDir) >= 0 && atlasDir != folderDir)
                    {
                        AssetDatabase.DeleteAsset(atlasPathTmp);
                        Debug.LogError("与图集生成策略冲突，因此删除已经存在的图集：" + atlasPathTmp);
                        bDelAtlas = true;
                    }
                }
            }

            //删除了图集，就强制刷新一次
            if (bDelAtlas)
                AssetDatabase.Refresh();

            //DeleteAtlasByDir(path);



            //判断是否整个文件夹打一个图集
            if(!isFolder) //如果已经指定了是是按照文件夹来打图集的，就需要再判断了
            {
                for (int i = 0; i < folderList.Count; i++)
                {
                    string dir = folderList[i];
                    if (path.IndexOf(dir) >= 0)
                    {
                        isFolder = true;
                        break;
                    }
                }
            }

            //有改变的，才打图集
            if (CheckModified(path, isFolder, targetABSet))
            {
                //获取当前文件夹的文件
                List<string> arrSprites = new List<string>();
                GetFiles(path, out arrSprites, null, postfixSp, isFolder);

                string name = Path.GetFileName(path.Substring(0, path.Length - 1));
                string atlasPath = $"{path}{name}{spAtlasPostfix}";

                if (arrSprites.Count > 0)
                {
                    SpriteAtlas sa;
                    bool bNewAtlas = false;

                    //没有图集就创建图集
                    if (!File.Exists(atlasPath))
                    {
                        sa = new SpriteAtlas();
                        bNewAtlas = true;
                    }
                    else
                    {
                        sa = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                        sa.Remove(sa.GetPackables());
                    }

                    SpriteAtlasPackingSettings packSet = new SpriteAtlasPackingSettings()
                    {
                        blockOffset = 1,
                        enableRotation = false,
                        enableTightPacking = false,
                        padding = 4,
                    };
                    sa.SetPackingSettings(packSet);

                    //添加指定平台资源格式
                    var atlasSetting = sa.GetPlatformSettings("DefaultTexturePlatform");
                    atlasSetting.overridden = true;
                    //atlasSetting.format = TextureImporterFormat.ASTC_6x6;
                    atlasSetting.maxTextureSize = 4096;

                    TextureImporterFormat fmt = TextureFMTSetting.Instance().GetTextureFormat(path);
                    atlasSetting.format = fmt;
                    sa.SetPlatformSettings(atlasSetting);

                    bool bHasReadableTex = false;

                    //图片的文件夹加入图集
                    List<Object> spList = new List<Object>();
                    foreach (var item in arrSprites)
                    {
                        Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(item);
                        if (sp != null && sp.texture != null)
                        {
                            bHasReadableTex = bHasReadableTex || sp.texture.isReadable;
                            spList.Add(sp);
                        }
                        else
                        {
                            Debug.LogError("获取Sprite失败, path=" + item);

                            Texture2D text = AssetDatabase.LoadAssetAtPath<Texture2D>(item);
                            if(null!=text)
                            {
                                Debug.LogError("获取text2D 成功, path=" + item+"，请设置成 sprite格式");
                            }else
                            {
                                Debug.LogError("请检查文件是否存在, path=" + item);
                            }
                        }
                    }

                    //设置图集纹理
                    SpriteAtlasTextureSettings textureSet = new SpriteAtlasTextureSettings()
                    {
                        readable = bHasReadableTex,         //需要激活读写
                        generateMipMaps = false,
                        sRGB = false,
                        filterMode = FilterMode.Bilinear,
                    };
                    sa.SetTextureSettings(textureSet);

                    //创建图集
                    if (bNewAtlas)
                    {
                        AssetDatabase.CreateAsset(sa, atlasPath);
                        Debug.Log("创建新图集：" + atlasPath);
                    }

                    //添加图片
                    sa.Add(spList.ToArray());

                    /*
                    if (isFolder)
                    {
                        //图片的文件夹加入图集
                        Object texture = AssetDatabase.LoadMainAssetAtPath(path);
                        //SpriteAtlasExtensions.Add(sa, new Object[] { texture });
                        sa.Add(new Object[] { texture });
                    }
                    else
                    {
                        //图片的文件夹加入图集
                        List<Object> spList = new List<Object>();
                        foreach (var item in arrSprites)
                        {
                            spList.Add(AssetDatabase.LoadAssetAtPath<Object>(item));
                        }
                        sa.Add(spList.ToArray());
                    }
                    */

                    RePackSingleAtlas(sa);
                    AssetDatabase.SaveAssets();
                    Debug.Log("更新图集：" + atlasPath);
                }
                else
                {
                    //没有包含任何图片了，就将其删除掉
                    if (File.Exists(atlasPath))
                    {
                        AssetDatabase.DeleteAsset(atlasPath);
                        Debug.LogError("此路径下没有精灵, 因此已经存在的图集：" + atlasPath);
                    }
                    //Debug.LogError("创建失败，此路径下没有精灵：" + path);
                }

            }

          

            if (recursive && (isFolder==false))
            {
                List<string> arrDirs = new List<string>();
                GetDirectories(path, out arrDirs, null, false);
                foreach (var item in arrDirs)
                {
                    CreateAtlasByDir(item, recursive, isFolder, isUpdate);
                }
            }
        }

        /// <summary>
        /// 为指定的文件夹打包其中所有的图片为一个图集
        /// </summary>
        public static void CreateSpecialAtlasByDir(string path, bool isFolder = false, bool isUpdate = false)
        {
            List<string> arrSprites = new List<string>();
            GetFiles(path,out arrSprites,null,postfixSp,true);
            if (arrSprites.Count > 0)
            {
                string[] arrAtlas = Directory.GetFiles(path, "*" + spAtlasPostfix, SearchOption.AllDirectories);
                for (int i = 0; i < arrAtlas.Length; ++i)
                {
                    if (File.Exists(arrAtlas[i]))
                    {
                        File.Delete(arrAtlas[i]);
                    }
                }
            }
            else
            {
                Debug.LogError("创建失败，此路径下没有精灵：" + path);
                return;
            }
            SpriteAtlas sa = new SpriteAtlas();
            SpriteAtlasPackingSettings packSet = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 4,
            };
            sa.SetPackingSettings(packSet);

            SpriteAtlasTextureSettings textureSet = new SpriteAtlasTextureSettings()
            {
                readable = false,
                generateMipMaps = false,
                sRGB = false,
                filterMode = FilterMode.Bilinear,
            };
            sa.SetTextureSettings(textureSet);

            //添加指定平台资源格式
            var atlasSetting = sa.GetPlatformSettings("Android");
            atlasSetting.overridden = true;
            //atlasSetting.format = TextureImporterFormat.ASTC_6x6;
            atlasSetting.format = TextureImporterFormat.ASTC_4x4;
            sa.SetPlatformSettings(atlasSetting);

            string name = Path.GetFileName(path);
            string atlasPath= $"{path}/{name}{spAtlasPostfix}";
            AssetDatabase.CreateAsset(sa,atlasPath);

            if (isFolder)
            {

                //图片的文件夹加入图集
                Object texture = AssetDatabase.LoadMainAssetAtPath(path);
                //SpriteAtlasExtensions.Add(sa, new Object[] { texture });
                sa.Add(new Object[] { texture });
            }
            else
            {
                //图片的文件夹加入图集
                List<Object> spList = new List<Object>();
                foreach (var item in arrSprites)
                {
                    spList.Add(AssetDatabase.LoadAssetAtPath<Object>(item));
                }
                sa.Add(spList.ToArray());
            }
            RePackSingleAtlas(sa);
            AssetDatabase.SaveAssets();
            Debug.Log("创建图集：" + atlasPath);
        }

        private static void DeleteAtlasByDir(string path, bool recursive)
        {
            //先判断是否有图集了
            string[] arrAtlas = Directory.GetFiles(path, "*" + spAtlasPostfix, SearchOption.TopDirectoryOnly);
            foreach (var item in arrAtlas)
            {
                if (AssetDatabase.DeleteAsset(item))
                    Debug.Log($"删除图集成功：{item}");
                else
                    Debug.Log($"删除图集失败：{item}");
            }

            if (recursive)
            {
                List<string> arrDirs = new List<string>();
                GetDirectories(path, out arrDirs, null, false);
                foreach (var item in arrDirs)
                {
                    DeleteAtlasByDir(item, recursive);
                }
            }
        }


        //创建图集，每个文件夹一个图集
        private static void CreateAtlasByTopDir(string path)
        {
            //先判断是否有图集了
            string name = Path.GetFileName(path);
            string atlasPath = $"{path}/{name}{spAtlasPostfix}";
            if (File.Exists(atlasPath))
            {
                if (EditorUtility.DisplayDialog("温馨提示", "此文件夹已有图集，是否重新创建并覆盖？", "确定", "取消"))
                {
                    //删除已有的图集
                    File.Delete(atlasPath);
                }
                else
                {
                    return;
                }
            }

            //没有图集就创建图集
            SpriteAtlas sa = new SpriteAtlas();

            SpriteAtlasPackingSettings packSet = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 4,
            };
            sa.SetPackingSettings(packSet);

            SpriteAtlasTextureSettings textureSet = new SpriteAtlasTextureSettings()
            {
                readable = false,
                generateMipMaps = false,
                sRGB = false,
                filterMode = FilterMode.Bilinear,
            };
            sa.SetTextureSettings(textureSet);

            AssetDatabase.CreateAsset(sa, atlasPath);

            //图片的文件夹加入图集
            Object texture = AssetDatabase.LoadMainAssetAtPath(path);
            //SpriteAtlasExtensions.Add(sa, new Object[] { texture });
            sa.Add(new Object[] { texture });
            RePackSingleAtlas(sa);
            AssetDatabase.SaveAssets();
            Debug.Log("创建图集：" + atlasPath);
        }

        //刷新图集
        private static void RefreshAtlasByDir(string path, bool recursive, bool isFolder)
        {
            string name = Path.GetFileName(path);
            string atlasPath = $"{path}/{name}{spAtlasPostfix}";
            bool isFolderAtlas = false;

            SpriteAtlas sa = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (sa)
            {
                Object[] arrSp = sa.GetPackables();
                if (isFolder || (arrSp.Length == 1 && arrSp[0].GetType() == typeof(DefaultAsset)))
                {
                    sa.Remove(arrSp);
                    Object texture = AssetDatabase.LoadMainAssetAtPath(path);
                    sa.Add(new Object[] { texture });
                    //var packingSetting = sa.GetPackingSettings();
                    //packingSetting.enableRotation = false;
                    //packingSetting.enableTightPacking = false;
                    //sa.SetPackingSettings(packingSetting);
                    isFolderAtlas = true;
                    //Debug.LogError("默认资源(填的文件夹)，不用刷新");
                }
                else
                {
                    List<string> arrSprites = new List<string>();
                    GetFiles(path, out arrSprites, null, postfixSp, false);
                    //图片的文件夹加入图集
                    List<Object> spList = new List<Object>();
                    foreach (var item in arrSprites)
                    {
                        spList.Add(AssetDatabase.LoadAssetAtPath<Object>(item));
                    }
                    sa.Remove(arrSp);
                    sa.Add(spList.ToArray());
                }

                RePackSingleAtlas(sa);
                AssetDatabase.SaveAssets();
                Debug.Log("刷新图集：" + atlasPath);
            }
            else
            {
                Debug.LogError("RefreshDirectoryAtlas >> 获取图集失败，不存在图集：" + atlasPath);
            }

            if (recursive && !isFolderAtlas)
            {
                List<string> arrDirs = new List<string>();
                GetDirectories(path, out arrDirs, null, false);
                foreach (var item in arrDirs)
                {
                    RefreshAtlasByDir(item, recursive, isFolder);
                }
            }
        }

        //重新打包图集
        //private static void RePackAtlasByDir(string path, bool recursive)
        //{
        //}

        //重新打包图集，根据当前平台
        private static void RePackSingleAtlas(SpriteAtlas sa)
        {
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { sa }, EditorUserBuildSettings.activeBuildTarget);
        }

        //是否有文件夹名字的图集了
        private bool IsHasAtlasWithDirName()
        {
            return false;
        }


        private static T[] GetSelectAsset<T>(SelectionMode selectionMode)
        {
            T[] selectObjs = Selection.GetFiltered<T>(selectionMode);
            return selectObjs;
        }


        //获取文件夹所有文件
        private static void GetDirectories(string path, out List<string> pathArr, string cutPath = null, bool recursive = true)
        {
            pathArr = Directory.GetDirectories(path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

            if (!string.IsNullOrEmpty(cutPath))
            {
                pathArr = pathArr.Select(p => p = GetRelativePath(p, cutPath)).ToList();
            }
        }

        //获取文件夹所有文件
        private static void GetFiles(string path, out List<string> pathArr, string cutPath = null, List<string> format = null, bool recursive = true)
        {
            pathArr = Directory.GetFiles(path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

            if (!string.IsNullOrEmpty(cutPath))
            {
                pathArr = pathArr.Select(p => p = GetRelativePath(p, cutPath)).ToList();
            }

            if (format != null && format.Count > 0)
            {
                pathArr = pathArr.Where(p => format.Contains(Path.GetExtension(p).ToLower())).ToList();
            }
        }

        /// <summary>
        /// 获取相对路径（不填基础路径的话，默认就是相对工程的路径Assets上一级）
        /// </summary>
        /// <param name="originalPath"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        private static string GetRelativePath(string originalPath, string basePath = null)
        {
            originalPath = originalPath.Replace("\\", "/");
            basePath = basePath.Replace("\\", "/");
            if (string.IsNullOrEmpty(basePath))
            {
                originalPath = FileUtil.GetProjectRelativePath(originalPath);
            }
            else
            {
                originalPath = originalPath.Replace(basePath, "");
            }
            return originalPath;
        }
    }
}
