


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;

namespace TextureFMT
{
    public class TextureFMTSetting
    {

        static private TextureFMTSetting _Instance = null;

        // 纹理格式配置
        private TextureFMTConfig _fmtConfig;

        //纹理对应的列表
        private Dictionary<TextureImporterFormat, List<string>> _dicFmt = new Dictionary<TextureImporterFormat, List<string>>();

        private List<string> _listReadWrite = new List<string>();

        //实例化一个单例
        static public TextureFMTSetting Instance()
        {
            if(null== _Instance)
            {
                _Instance = new TextureFMTSetting();
                
            }

            if (_Instance._fmtConfig == null)
            {
                _Instance.Init();
            }

            return _Instance;
        }

        //初始化
        private void Init()
        {
            string path = "Assets/XGameEditor/Editor/AssetsTool/TextureFormat/TextureFMTConfig.asset";
            _Instance._fmtConfig = AssetDatabase.LoadAssetAtPath<TextureFMTConfig>(path);

            if(null== _Instance._fmtConfig)
            {
                return;
            }

            _dicFmt.Clear();
            _dicFmt.Add(TextureImporterFormat.ASTC_4x4, _fmtConfig.m_listASTC4X4);
            _dicFmt.Add(TextureImporterFormat.ASTC_5x5, _fmtConfig.m_listASTC5X5);
            _dicFmt.Add(TextureImporterFormat.ASTC_6x6, _fmtConfig.m_listASTC6X6);
            _dicFmt.Add(TextureImporterFormat.ASTC_8x8, _fmtConfig.m_listASTC8X8);

            //转换可读写路径的大小写
            List<string> fmtList = null;
            foreach (TextureImporterFormat fmt in _dicFmt.Keys)
            {
                fmtList = _dicFmt[fmt];
                for (int i = 0; i < fmtList.Count; ++i)
                {
                    fmtList[i] = fmtList[i].Replace('\\', '/').ToLower();
                }
            }

            //转换可读写的路径大小写
            _listReadWrite.Clear();
            foreach ( string folder in _fmtConfig.m_textureReadWriteFolder)
            {
                _listReadWrite.Add(folder.Replace('\\', '/').ToLower());
            }

            

        }

        //获取图集格式
        public TextureImporterFormat GetTextureFormat(string path)
        {

            //TextureImporterFormat.ASTC_6x6
            //测试一次4X4格式
            //return TextureImporterFormat.ASTC_4x4;

            path = path.Replace('\\', '/').ToLower();
            List<string> fmtList = null;
            foreach (TextureImporterFormat fmt in _dicFmt.Keys)
            {
                fmtList = _dicFmt[fmt];
                for(int i=0;i<fmtList.Count;++i)
                {
                    if(path.IndexOf(fmtList[i])>=0)
                    {
                        return fmt;
                    }
                }
            }

            //默认使用6X6
            return TextureImporterFormat.ASTC_4x4;
        }

        bool IsReadWrite(string path)
        {
            path = path.Replace('\\', '/').ToLower();
            int nCount = _listReadWrite.Count;
            for(int i =0;i<nCount;++i)
            {
                if (path.IndexOf(_listReadWrite[i]) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        [MenuItem("XGame/资源工具/一键设置图集&纹理压缩格式")]
        static void SetSpriteAtlasAndTextureFormat()
        {
            SetTextureFormat();
            SetSpriteAtlasFormat();
        }

            [MenuItem("XGame/资源工具/设置纹理压缩格式")]
        static void SetTextureFormat()
        {
            List<string> allTexturePaths = new List<string>();
            string textureType = "*.jpg,*.png,*.tag";

            List<string> listCheckFolder = TextureFMTSetting.Instance()._fmtConfig.m_listCheckFolder;

            //得到所有图片格式
            string[] textureTypeArray = textureType.Split(',');

            for (int i = 0; i < textureTypeArray.Length; i++)
            {
                
                for(int n=0;n< listCheckFolder.Count; ++n)
                {
                    string[] texturePath = Directory.GetFiles(listCheckFolder[n], textureTypeArray[i], SearchOption.AllDirectories);
                    for (int j = 0; j < texturePath.Length; j++)
                    {
                        allTexturePaths.Add(texturePath[j]);
                        //Debug.Log(texturePath[j]);
                    }
                }

               
            }

            for (int k = 0; k < allTexturePaths.Count; k++)
            {

    
                string path = allTexturePaths[k];//
                Texture2D tx = new Texture2D(200, 200);
                tx.LoadImage(getTextureByte(allTexturePaths[k]));
                //如果图片不符合规范
                if (!(isPower(tx.height) && isPower(tx.width)))
                {
                    Debug.Log("不符合规范的图片的尺寸为:" + tx.width + "X" + tx.height);
                }

                if (ProcessTexture(path) == false)
                {

                }


            }

            Debug.Log("图片格式设置完成");


        }

        [MenuItem("XGame/资源工具/设置图集压缩格式")]
        static void SetSpriteAtlasFormat()
        {
            List<string> allTexturePaths = new List<string>();
            string textureType = "*.spriteatlas";

            List<string> listCheckFolder = TextureFMTSetting.Instance()._fmtConfig.m_listCheckFolder;

            //得到所有图片格式
            string[] textureTypeArray = textureType.Split(',');

            for (int i = 0; i < textureTypeArray.Length; i++)
            {

                for (int n = 0; n < listCheckFolder.Count; ++n)
                {
                    string[] texturePath = Directory.GetFiles(listCheckFolder[n], textureTypeArray[i], SearchOption.AllDirectories);
                    for (int j = 0; j < texturePath.Length; j++)
                    {
                        ProcessSpriteAtlas(texturePath[j]);
                    }
                }


            }

        }


        public static bool ProcessSpriteAtlas(string path)
        {

            SpriteAtlas sa = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);

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
            atlasSetting.maxTextureSize = 4096;
            TextureImporterFormat fmt = TextureFMTSetting.Instance().GetTextureFormat(path);
            atlasSetting.format = fmt;
            
            //atlasSetting.format = TextureImporterFormat.ASTC_4x4;
            sa.SetPlatformSettings(atlasSetting);

            //设置图集纹理
            SpriteAtlasTextureSettings textureSet = new SpriteAtlasTextureSettings()
            {
                readable = false,         
                generateMipMaps = false,
                sRGB = false,
                filterMode = FilterMode.Bilinear,
            };
            sa.SetTextureSettings(textureSet);

            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { sa }, EditorUserBuildSettings.activeBuildTarget);
            AssetDatabase.ImportAsset(path);



            return true;
        }

        public static bool ProcessTexture(string path)
        {
            TextureImporter textureImporter = TextureImporter.GetAtPath(path) as TextureImporter;
            //textureImporter.textureType = TextureImporterType.Advanced;

            if (null == textureImporter)
            {
                //Debug.Log(path + " null== textureImporter");
                return false;
            }

            bool bChange = false;
            if (textureImporter.mipmapEnabled)
            {
                bChange = true;
                Debug.Log(path + " mipmapEnabled 需要关闭");
            }


            bool isReadable = TextureFMTSetting.Instance().IsReadWrite(path);
            if (textureImporter.isReadable!= isReadable)
            {
                bChange = true;
                Debug.Log(path + " isReadable 需要关闭");
            }

            TextureImporterFormat fmt = TextureFMTSetting.Instance().GetTextureFormat(path);

            //设置安卓格式
            TextureImporterPlatformSettings setting = textureImporter.GetPlatformTextureSettings("Android");
            if (setting.format != fmt)
            {
                bChange = true;
                //textureImporter.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ASTC_6x6);
                setting.format = fmt;
                setting.overridden = true;
                textureImporter.SetPlatformTextureSettings(setting);
                textureImporter.SaveAndReimport();
            }

            //设置IOS格式
            TextureImporterPlatformSettings iosSetting = textureImporter.GetPlatformTextureSettings("iOS");
            if(iosSetting.format!= fmt)
            {
                bChange = true;
                iosSetting.format = fmt;
                iosSetting.overridden = true;
                textureImporter.SetPlatformTextureSettings(iosSetting);
                textureImporter.SaveAndReimport();
            }

            if (bChange)
            {
                textureImporter.mipmapEnabled = false;
                textureImporter.isReadable = isReadable;
                AssetDatabase.ImportAsset(path);

            }

            return true;


        }


        /// <summary>
        /// 根据图片路径返回字节流
        /// </summary>
        /// <param name="texturePath"></param>
        /// <returns></returns>
        static byte[] getTextureByte(string texturePath)
        {
            FileStream file = new FileStream(texturePath, FileMode.Open);
            byte[] txByte = new byte[file.Length];
            file.Read(txByte, 0, txByte.Length);
            file.Close();
            return txByte;
        }
        /// <summary>
        /// 判断图片尺寸是否为2的n次方
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        static bool isPower(int n)
        {
            if (n < 1)
                return false;
            int i = 1;
            while (i <= n)
            {
                if (i == n)
                    return true;
                i <<= 1;

            }
            return false;
        }

    }

}


