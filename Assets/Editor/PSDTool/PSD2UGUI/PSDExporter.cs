using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhotoshopFile;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using static subjectnerdagreement.psdexport.PSDExportSettings;

namespace subjectnerdagreement.psdexport
{
    public class PSDExporter
    {
        public enum ScaleDown
        {
            Default,
            Half,
            Quarter
        }

        public delegate Color GetPixelColor(int row, int col);

        //公共资源的md5与路径，GUI是根据md5生成的一个对象
        private static Dictionary<System.Guid, string> m_dicCommTextureGuids = new Dictionary<System.Guid, string>();

        public static Dictionary<string, string> Res2CommonDic = new Dictionary<string, string>();

        /// <summary>
        /// 是计算所有公共图片资源的MD5
        /// </summary>
        /// <param name="dir"></param>
        private static void CalcUITextureGuids(DirectoryInfo dir)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            DirectoryInfo[] dii = dir.GetDirectories();
            FileInfo[] fil = dir.GetFiles();
            foreach (DirectoryInfo d in dii)
            {
                CalcUITextureGuids(d);
            }

            foreach (FileInfo f in fil)
            {
                if (Path.GetExtension(f.Name).ToLower() == ".png")
                {
                    //获取文件，计算md5
                    FileStream file = new FileStream(f.FullName, FileMode.Open);
                    byte[] retVal = md5.ComputeHash(file);
                    file.Close();
                    System.Guid guid = new System.Guid(retVal);
                    string curFileRelPath = FileUtil.GetProjectRelativePath(f.FullName.Replace("\\", "/"));
                    string filepath;
                    if (m_dicCommTextureGuids.TryGetValue(guid, out filepath))
                    {
                        Debug.Log("图：" + curFileRelPath + "  和图：" + filepath + " 是同一张图");
                    }
                    else
                    {
                        m_dicCommTextureGuids.Add(guid, curFileRelPath);
                    }
                }
            }
        }

        public static List<int> GetExportLayers(PSDExportSettings settings, PSDFileInfo fileInfo)
        {
            List<int> exportLayers = new List<int>();
            foreach (var keypair in settings.layerSettings)
            {
                PSDExportSettings.LayerSetting layerSetting = keypair.Value;
                // Don't export if not set to export,如果没设置导出,则不导出
                if (!layerSetting.doExport)
                    continue;

                // Don't export if group is off, 如果组关闭,不导出
                var groupInfo = fileInfo.GetGroupByLayerIndex(layerSetting.layerIndex);
                if (groupInfo != null && !groupInfo.visible)
                    continue;

                //通过标记指定要跳过
                if (layerSetting.importInfo != null && layerSetting.importInfo.isSkip)
                    continue;

                exportLayers.Add(layerSetting.layerIndex);
            }
            return exportLayers;
        }

        public static int GetExportCount(PSDExportSettings settings, PSDFileInfo fileInfo)
        {
            var exportLayers = GetExportLayers(settings, fileInfo);
            return exportLayers.Count;
        }

        public static void Export(PSDExportSettings settings, PSDFileInfo fileInfo, bool exportExisting = true)
        {
            Res2CommonDic.Clear();
            m_dicCommTextureGuids.Clear();

            //构建导入信息
            settings.BuildLayerImportInfo();
   
            //创建公共资源目录
            string commonDirPath = Path.Combine(Application.dataPath, PSDSetting.Instance.importBaseDir, PSDExportSettings.CommonDirName);
            if (!Directory.Exists(commonDirPath)) 
                Directory.CreateDirectory(commonDirPath);

            //计公共资源的MD5
            CalcUITextureGuids(new DirectoryInfo(commonDirPath));

            List<int> layerIndices = GetExportLayers(settings, fileInfo);
            // If not going to export existing, filter out layers with existing files 如果不导出现有的，用现有文件过滤层
            if (exportExisting == false)
            {
                layerIndices = layerIndices.Where(delegate (int layerIndex)
                {
                    string filePath = GetLayerFileRelPath(settings, layerIndex);
                    // If file exists, don't export
                    return !File.Exists(filePath);
                }).ToList();
            }

            //导入其他层
            int exportCount = 0;
            foreach (int layerIndex in layerIndices)
            {
                string infoString = string.Format("Importing {0} / {1} Layers", exportCount, layerIndices.Count);
                string fileString = string.Format("Importing PSD Layers: {0}", settings.Filename);

                float progress = exportCount / (float)layerIndices.Count;
                EditorUtility.DisplayProgressBar(fileString, infoString, progress);
                exportCount++;

                var layer = settings.Psd.Layers[layerIndex];
                if (layer.IsText) continue;

                var layerSetting = settings.layerSettings[layerIndex];
                if (HasMarkFlag(layerSetting, EImportMarkType.Zoom) || HasMarkFlag(layerSetting, EImportMarkType.Lucency))
                {
                    //过滤同名资源
                    string assetPath = GetFileSavePathBySetting(settings, layerIndex);
                    string layerName = Path.GetFileNameWithoutExtension(assetPath);

                    foreach (var item in m_dicCommTextureGuids.Values)
                    {
                        if (Path.GetFileNameWithoutExtension(item) == layerName)
                        {
                            if (!Res2CommonDic.ContainsKey(assetPath))
                                Res2CommonDic.Add(assetPath, item);
                            continue;
                        }
                    }
                }

                CreateSprite(settings, layerIndex, fileInfo);
            }

            EditorUtility.ClearProgressBar();
            settings.SaveMetaData();
            settings.SaveLayerMetaData();
        }

        public static void CreateSprite(PSDExportSettings settings, int layerIndex, PSDFileInfo fileInfo)
        {
            var layer = settings.Psd.Layers[layerIndex];
            if (layer.IsText) return;

            Vector4 spriteBorder = Vector4.zero;
            Texture2D tex = null;

            var layerSetting = settings.layerSettings[layerIndex];

            //if (layerSetting.importInfo.importName == "zhuanshibj" || layerSetting.importInfo.importName == "jinbibj")
            //{
            //    Debug.LogError("sdfas");
            //}

            if (layerSetting.importInfo == null)
            {
                Debug.LogError("导入图层出错，导入信息为空：layerIndex=" + layerIndex);
                return;
            }

            //资源库导入
            if (settings.IsCommonPSD)
            {
                tex = CreateTexture(layer, layerSetting, out spriteBorder, true, false, settings.IsNeedAlphaCorrect);
            }
            //非资源库导入，需要对比是否为资源库中的图片
            else
            {
                //是否需要创建一张新的图片
                bool isNeedCreateNew = false;

                //指明了这是一张公共资源
                if (layerSetting.importInfo.isCommRes)
                {
                    //直接从路径中加载
                    string path = $"{layerSetting.importInfo.dirCommRelPath}{layerSetting.importInfo.importName}.png";
                    tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                    if (tex == null)
                    {
                            //不存在，需要检查资源库中是否存在同样的图片了
                    }
                    else
                    {
                        if (layerSetting.importInfo.isUpdate)
                        {
                            tex = null;
                            isNeedCreateNew = true;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (layerSetting.importInfo.isUpdate)
                    {
                        tex = null;
                        isNeedCreateNew = true;
                    }
                }

                //与资源库中匹配的资源路径
                string commResPath = string.Empty;

                //资源自身的存放路径
                string assetPath = GetFileSavePathBySetting(settings, layerIndex);

                //非公资源则需要按照一定的顺序在资源库中进行查找匹配：九宫格匹配->正常匹配；如果能够在公共资源库中找到对应的资源，则说明是公共资源，否则就不是
                if (!isNeedCreateNew)
                {
                    //先尝试使用九宫格的方式查找资源
                    tex = CreateTexture(layer, layerSetting, out spriteBorder, false, true, settings.IsNeedAlphaCorrect);

                    //获取图片的GUID，这个GUID根据图片的md5生成
                    System.Guid guid = GetTextureGUID(tex);

                    //如果按照九宫格方式查找不到
                    if (!m_dicCommTextureGuids.TryGetValue(guid, out commResPath))
                    {
                        Object.DestroyImmediate(tex);

                        //则尝试按照非九宫的方式查找资源
                        tex = CreateTexture(layer, layerSetting, out spriteBorder, false, false, settings.IsNeedAlphaCorrect);
                        guid = GetTextureGUID(tex);

                        //还是查找不到
                        if (!m_dicCommTextureGuids.TryGetValue(guid, out commResPath))
                        {
                            Object.DestroyImmediate(tex);
                            isNeedCreateNew = true;
                        }
                    }
                }

                //在共资源中找了对应的资源，那么就不需要创建了
                if (!isNeedCreateNew)
                {
                    //表示能在公共资源找到同guid的资源，那么标记为公共资源（存入映射关系）
                    if (commResPath != assetPath)
                    {
                        if (Res2CommonDic.ContainsKey(assetPath))
                        {
                            Debug.Log("有同名的图层导入：" + assetPath);
                        }
                        else
                        {
                            Res2CommonDic.Add(assetPath, commResPath);
                        }
                    }

                    //销毁创建的临时图片
                    if(tex != null)
                        Object.DestroyImmediate(tex);

                    //自身路径与资源库中的路径一致，不需要再做处理了
                    return;
                }

                //检测是否有内容不同，但是资源相同的资源导入
                if (layerSetting.importInfo.isCommRes && !layerSetting.importInfo.isUpdate)
                {
                    if (m_dicCommTextureGuids.Values.Contains(assetPath))
                    {
                        Debug.LogError($"存在名字相同但内容不同的图片，保存路径：[{assetPath}]，PSD名称：[{settings.Filename}.psd]，层：[{layer.Name}]。请联系美术同学修改！！");
                        Object.DestroyImmediate(tex);
                        return;
                    }
                }

                //创建图片
                if(tex == null)
                {
                    tex = CreateTexture(layer, layerSetting, out spriteBorder, true, false, settings.IsNeedAlphaCorrect);

                    //新建的图片，需要记录下GUID
                    if (layerSetting.importInfo.isCommRes)
                    {
                        System.Guid guid = GetTextureGUID(tex);
                        if (m_dicCommTextureGuids.ContainsKey(guid))
                            m_dicCommTextureGuids[guid] = assetPath;
                        else
                            m_dicCommTextureGuids.Add(guid, assetPath);
                    }
                }
            }

            if (tex == null) 
                return;

            SaveAsset(settings, tex, spriteBorder, layerIndex);

            Object.DestroyImmediate(tex);
        }

        private static Texture2D CreateTexture(Layer layer, LayerSetting layerSetting, out Vector4 spriteBorder, bool isAuto, bool isSlice9Piece = false, bool isOpenAlphaCorrect = false)
        {
            spriteBorder = Vector4.zero;
            int layerWidth = (int)layer.Rect.width;
            int layerHeight = (int)layer.Rect.height;

            if (layerWidth == 0 || layerWidth == 0)
            {
                return null;
            }

            Channel red = (from l in layer.Channels where l.ID == 0 select l).First();
            Channel green = (from l in layer.Channels where l.ID == 1 select l).First();
            Channel blue = (from l in layer.Channels where l.ID == 2 select l).First();
            Channel alpha = layer.AlphaChannel;
            float layerOpacity = layer.Opacity / 255f;

            byte r0 = red.ImageData[0];
            byte g0 = green.ImageData[0];
            byte b0 = blue.ImageData[0];
            byte a0 = 255;
            if (alpha != null)
            {
                a0 = alpha.ImageData[0];
            }
            bool pureColor = true;

            for (int i = 0; i < layer.Rect.width * layer.Rect.height; i++)
            {
                byte r = red.ImageData[i];
                byte g = green.ImageData[i];
                byte b = blue.ImageData[i];
                byte a = 255;

                if (alpha != null)
                {
                    a = alpha.ImageData[i];
                }

                if (r != r0 || g != g0 || b != b0 || a != a0)
                {
                    pureColor = false;
                    break;
                }
            }

            Texture2D tex = null;
            Color32[] pixels = null;
            if (pureColor)
            {
                //长*宽的值 = 像素个数
                tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                pixels = new Color32[1];

                a0 = GetAlphaCorrection(a0, layerOpacity, isOpenAlphaCorrect);
                //pixels[0] = new Color32(r0, g0, b0, a0);
                pixels[0] = GetCorrectColor(r0, g0, b0, a0);
            }
            else
            {
                // 检测九宫格形式图片
                if (isAuto)
                {
                    //EImportMarkType funcType = GetLayerFuncType(layer.Name);
                    //if (funcType == EImportMarkType.Slice9Piece)    //九宫格处理
                    if (HasMarkFlag(layerSetting, EImportMarkType.Slice9Piece))
                        spriteBorder = CheckLayerBorder(layer);
                }
                else
                {
                    if (isSlice9Piece)
                        spriteBorder = CheckLayerBorder(layer);
                }

                if (spriteBorder == Vector4.zero)
                {
                    tex = new Texture2D(layerWidth, layerHeight, TextureFormat.RGBA32, false);
                    pixels = new Color32[tex.width * tex.height];
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        byte r = red.ImageData[i];
                        byte g = green.ImageData[i];
                        byte b = blue.ImageData[i];
                        byte a = 255;

                        if (alpha != null)
                            a = alpha.ImageData[i];

                        a = GetAlphaCorrection(a, layerOpacity, isOpenAlphaCorrect);

                        int mod = i % tex.width;
                        int n = ((tex.width - mod - 1) + i) - mod;
                        //pixels[pixels.Length - n - 1] = new Color32(r, g, b, a);
                        pixels[pixels.Length - n - 1] = GetCorrectColor(r, g, b, a);
                    }
                }
                else
                {
                    GetPixelColor GetPixelColor = (row, col) =>
                    {
                        int pixelIndex = (layerHeight - col - 1) * layerWidth + row;
                        byte r = red.ImageData[pixelIndex];
                        byte g = green.ImageData[pixelIndex];
                        byte b = blue.ImageData[pixelIndex];
                        byte a = 255;

                        if (alpha != null)
                            a = alpha.ImageData[pixelIndex];

                        a = GetAlphaCorrection(a, layerOpacity, isOpenAlphaCorrect);
                        //return new Color32(r, g, b, a);
                        return GetCorrectColor(r, g, b, a);
                    };

                    int texWidth = (int)spriteBorder.x + 1 + (layerWidth - 1 - (int)spriteBorder.z);
                    int texHeight = (int)spriteBorder.y + 1 + (layerHeight - 1 - (int)spriteBorder.w);
                    tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
                    pixels = new Color32[tex.width * tex.height];
                    int pixelsIndex = 0;
                    for (int i = 0; i < layerHeight; ++i)
                    {
                        if (spriteBorder.y <= i && i <= spriteBorder.w)
                        {
                            i += (int)(spriteBorder.w - spriteBorder.y);
                        }

                        for (int j = 0; j < layerWidth; ++j)
                        {
                            if (spriteBorder.x <= j && j <= spriteBorder.z)
                            {
                                j += (int)(spriteBorder.z - spriteBorder.x);
                            }

                            pixels[pixelsIndex++] = GetPixelColor(j, i);
                        }
                    }

                    // 转换border为Unity的Sprite格式的border
                    // 值有效才做处理，0为无效值
                    if (spriteBorder.z != 0)
                    {
                        spriteBorder.z = layerWidth - spriteBorder.z - 1;
                    }
                    if (spriteBorder.w != 0)
                    {
                        spriteBorder.w = layerHeight - spriteBorder.w - 1;
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        //色差校正
        private static Color32 GetCorrectColor(byte r, byte g, byte b, byte a)
        {
            //只处理有透明的资源
            /*if (a > 0 && a < 255)
            {
                float offset_r = 9;
                float offset_g = 7;
                float offset_b = 4;

                r = (byte)Mathf.Clamp(r + offset_r, 0, 255);
                g = (byte)Mathf.Clamp(g + offset_g, 0, 255);
                b = (byte)Mathf.Clamp(b + offset_b, 0, 255);
            }*/

            //float offset_r = LinearToGammaSpace(r / 255f);
            //float offset_g = LinearToGammaSpace(g / 255f);
            //float offset_b = LinearToGammaSpace(b / 255f);

            /*
            float alpha = a / 255f;
            r = (byte)Mathf.Clamp(Mathf.CeilToInt(r / alpha), 0, 255);
            g = (byte)Mathf.Clamp(Mathf.CeilToInt(g / alpha), 0, 255);
            b = (byte)Mathf.Clamp(Mathf.CeilToInt(b / alpha), 0, 255);
            */

            return new Color32(r, g, b, a);
        }


        //获取gamma转到liner的伽马校正
        private static byte GetAlphaCorrection(float value, float opacity, bool isOpenAlphaCorrect)
        {
            if (!isOpenAlphaCorrect)
            {
                return (byte)Mathf.CeilToInt(value * opacity);
            }

            float sumAlpha = value / 255f * opacity;
            if (sumAlpha <= 0)
                return 0;
            else if (sumAlpha >= 1)
                return (byte)value;
            else
            {

                //sumAlpha = Mathf.Pow(sumAlpha, 0.45f);
                //sumAlpha = Mathf.Pow(sumAlpha, GetGradualPower(sumAlpha, 0.4f, 0.7f));
                sumAlpha = LinearToGammaSpace(sumAlpha);
                //sumAlpha = LinearToGammaSpaceExact(sumAlpha);
                //int resVal = Mathf.CeilToInt(sumAlpha * 255);
                //return (byte)resVal;
                return (byte)(sumAlpha * 255);
            }
        }

        //获取渐变的校正系数Power【透明度底的校正系数高，反之低】
        private static float GetGradualPower(float val, float min, float max)
        {
            //return (min - max) * val + max;   //线性变换
            return (min - max) * val * val + max;  //曲线变换   y=ax^2+c
        }

        private static float LinearToGammaSpaceExact(float value)
        {
            if (value <= 0.0F)
                return 0.0F;
            else if (value <= 0.0031308F)
                return 12.92F * value;
            else if (value < 1.0F)
                return 1.055F * Mathf.Pow(value, 0.4166667F) - 0.055F;
            else
                return Mathf.Pow(value, 0.45454545F);
        }

        private static float LinearToGammaSpace(float value)
        {
            value = Mathf.Clamp01(value);

            value = 1.055f * Mathf.Pow(value, 0.416666667f) - 0.055f;
            value = Mathf.Clamp01(value);

            return value;
        }

        protected static Vector4 CheckLayerBorder(Layer layer)
        {
            int layerWidth = (int)layer.Rect.width;
            int layerHeight = (int)layer.Rect.height;

            Vector4 spriteBorder = Vector4.zero;

            Channel red = (from l in layer.Channels where l.ID == 0 select l).First();
            Channel green = (from l in layer.Channels where l.ID == 1 select l).First();
            Channel blue = (from l in layer.Channels where l.ID == 2 select l).First();
            Channel alpha = layer.AlphaChannel;

            // 检测九宫格形式图片
            Vector2Int centerPoint = new Vector2Int(layerWidth / 2, layerHeight / 2);

            GetPixelColor GetPixelColor = (row, col) =>
            {
                try
                {
                    int pixelIndex = (layerHeight - col - 1) * layerWidth + row;
                    byte r = red.ImageData[pixelIndex];
                    byte g = green.ImageData[pixelIndex];
                    byte b = blue.ImageData[pixelIndex];
                    byte a = 255;

                    if (alpha != null)
                        a = alpha.ImageData[pixelIndex];
                    return new Color32(r, g, b, a);
                }
                catch (System.IndexOutOfRangeException)
                {
                    return Color.black;
                }
            };

            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;

            bool leftPass = true;
            bool rightPass = true;

            for (int widthBias = 1; widthBias < centerPoint.x; ++widthBias)
            {
                int l = centerPoint.x - widthBias;
                int r = centerPoint.x + widthBias;

                //扫描每一行
                for (int j = 0; j < layerHeight; ++j)
                {
                    //每一行的中间位置颜色
                    Color colorMiddle = GetPixelColor(centerPoint.x, j);

                    //需要检查左边界，如果左侧颜色值与中间位置颜色不一样，则说明是左边界
                    if (leftPass)
                    {
                        Color colorLeft = GetPixelColor(l, j);
                        if (colorMiddle != colorLeft)
                        {
                            leftPass = false;
                        }
                    }

                    //右边界检查，如果右侧颜色值与中间位置颜色不一样，则说明是右边界
                    if (rightPass)
                    {
                        Color colorRight = GetPixelColor(r, j);
                        if (colorMiddle != colorRight)
                        {
                            rightPass = false;
                        }
                    }

                    if (!leftPass && !rightPass)
                    {
                        break;
                    }
                }

                if (leftPass)
                {
                    left = l;
                    // 如果左边检测通过，右边的值至少是中间值
                    if (right == 0)
                    {
                        right = centerPoint.x;
                    }
                }

                if (rightPass)
                {
                    right = r;
                    // 如果右边检测通过，左边的值至少是中间值
                    if (left == 0)
                    {
                        left = centerPoint.x;
                    }
                }

                if (!leftPass && !rightPass)
                {
                    break;
                }
            }

            bool topPass = true;
            bool bottomPass = true;

            for (int heightBias = 1; heightBias < centerPoint.y; ++heightBias)
            {
                int t = centerPoint.y - heightBias;
                int b = centerPoint.y + heightBias;

                for (int j = 0; j < layerWidth; ++j)
                {
                    Color colorMiddle = GetPixelColor(j, centerPoint.y);

                    if (topPass)
                    {
                        Color colorTop = GetPixelColor(j, t);
                        if (colorMiddle != colorTop)
                        {
                            topPass = false;
                        }
                    }

                    if (bottomPass)
                    {
                        Color colorBottom = GetPixelColor(j, b);
                        if (colorMiddle != colorBottom)
                        {
                            bottomPass = false;
                        }
                    }

                    if (!topPass && !bottomPass)
                    {
                        break;
                    }
                }

                if (topPass)
                {
                    top = t;
                    // 如果上边检测通过，下边的值至少是中间值
                    if (bottom == 0)
                    {
                        bottom = centerPoint.y;
                    }
                }

                if (bottomPass)
                {
                    bottom = b;
                    // 如果下边检测通过，上边的值至少是中间值
                    if (top == 0)
                    {
                        top = centerPoint.y;
                    }
                }

                if (!topPass && !bottomPass)
                {
                    break;
                }
            }

            //左右各保留一个像素，防止出错
            if(right - left >= 3)
            {
                left++;
                right--;
            }

            //上下各保留一个像素，防止出错
            if (bottom - top >= 3)
            {
                top++;
                bottom--;
            }

            // left border
            spriteBorder.x = left;

            // right border
            spriteBorder.z = right;

            // top border
            spriteBorder.y = top;

            // bottom border
            spriteBorder.w = bottom;

            return spriteBorder;
        }

        public static EImportMarkType GetLayerFuncTypeFlag(string layerName)
        {
            ImportResInfo resInfo = PSDSetting.Instance.GetImportResInfo(Path.GetFileName(layerName));
            if (resInfo != null)
            {
                return resInfo.funcTypeFlag;
            }
            return EImportMarkType.None;
        }

        public static bool HasMarkFlag(LayerSetting layerSetting, EImportMarkType markType)
        {
            if (layerSetting.importInfo == null)
                return false;
            return (layerSetting.importInfo.funcTypeFlag & markType) == markType;
        }

        public static string GetLayerFileName(PSDExportSettings settings, int layerIndex)
        {
            string layerName = settings.Psd.Layers[layerIndex].Name;
            ImportResInfo resInfo = PSDSetting.Instance.GetImportResInfo(Path.GetFileName(layerName));
            if (resInfo != null)
                layerName = resInfo.sourceName;
            return layerName;
        }

        //获取资源导入的路径
        public static string GetLayerFileRelPath(PSDExportSettings settings, int layerIndex)
        {
            // Strip out invalid characters from the file name从文件名中删除无效字符
            string layerName = settings.Psd.Layers[layerIndex].Name;
            var layerSetting = settings.layerSettings[layerIndex];

            string filePath;

            //没有信息，则按照旧的方式处理
            if (!layerSetting.importInfo.isCommRes)
            {
                filePath = settings.GetLayerPath(layerSetting.importInfo.importName);
            }
            else
            {
                filePath = layerSetting.importInfo.dirCommRelPath + layerSetting.importInfo.importName;
            }

            return filePath.Replace("\\", "/");
        }

        public static string GetFileSaveName(string path)
        {
            //目录名称
            string assetDirectory = Path.GetDirectoryName(path);

            //去掉文件名中的Auto前缀
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName.ToLower().StartsWith("auto") && fileName.IndexOf("_") != -1)
            {
                fileName = fileName.Substring(fileName.IndexOf("_") + 1);
            }
            return assetDirectory + "/" + fileName + ".png";
        }

        private static Texture2D ScaleTexture(PSDExportSettings settings, Texture2D tex, int layer)
        {
            //应用全局缩放，如果有的话
            if (settings.ScaleBy > 0)
            {
                tex = ScaleTextureByMipmap(tex, settings.ScaleBy);
            }

            PSDExportSettings.LayerSetting layerSetting = settings.layerSettings[layer];

            // Then scale by layer scale
            if (layerSetting.scaleBy != ScaleDown.Default)
            {
                // By default, scale by half
                int scaleLevel = 1;

                // Setting is actually scale by quarter
                if (layerSetting.scaleBy == ScaleDown.Quarter)
                {
                    scaleLevel = 2;
                }

                // Apply scaling
                tex = ScaleTextureByMipmap(tex, scaleLevel);
            }
            return tex;
        }

        //计算图片的GUID（根据md5生成）
        private static System.Guid GetTextureGUID(Texture2D tex)
        {
            if (tex)
            {
                byte[] buf = tex.EncodeToPNG();
                System.Security.Cryptography.MD5 _md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = _md5.ComputeHash(buf);
                System.Guid guid = new System.Guid(retVal);
                return guid;
            }
            return System.Guid.Empty;
        }

        public static string GetFileSavePathBySetting(PSDExportSettings settings, int layer)
        {
            string assetPath = GetLayerFileRelPath(settings, layer);
            string assetDirectory = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(assetDirectory))
            {
                Directory.CreateDirectory(assetDirectory);
            }

            assetPath = GetFileSaveName(assetPath);
            return assetPath.Replace("\\", "/");
        }

        private static void SaveAsset(PSDExportSettings settings, Texture2D tex, Vector4 spriteBorder, int layer)
        {
            string assetPath = GetFileSavePathBySetting(settings, layer);

            // 设置缩放变量
            float pixelsToUnits = settings.PixelsToUnitSize;
            PSDExportSettings.LayerSetting layerSetting = settings.layerSettings[layer];

            // Then scale by layer scale
            if (layerSetting.scaleBy != ScaleDown.Default)
            {
                // By default, scale by half
                pixelsToUnits = Mathf.RoundToInt(settings.PixelsToUnitSize / 2f);
                spriteBorder = spriteBorder / 2f;

                // Setting is actually scale by quarter
                if (layerSetting.scaleBy == ScaleDown.Quarter)
                {
                    pixelsToUnits = Mathf.RoundToInt(settings.PixelsToUnitSize / 4f);
                    spriteBorder = spriteBorder / 2f;
                }
            }

            //把所有图片的MD5码和Guid与 正在导入的层（生成在内存中的png进行比较;
            tex = ScaleTexture(settings, tex, layer);

            byte[] buf = tex.EncodeToPNG();
            File.WriteAllBytes(assetPath, buf);
            //AssetDatabase.CreateAsset(tex, assetPath);
            AssetDatabase.Refresh();
            //Debug.Log("创建图片，path=" + assetPath);

            // 加载纹理，以便我们可以更改类型
            var textureObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));

            // 获取资产的纹理导入器
            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(assetPath);

            if (null == textureImporter)
            {
                Debug.LogError("保存纹理失败" + assetPath);
                //return new Sprite();
                return;
            }

            // 读出纹理导入设置，以便更改导入轴点
            TextureImporterSettings importSetting = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(importSetting);

            // 设置枢轴导入设置
            importSetting.spriteAlignment = (int)settings.Pivot;

            // 但是，如果图层设置具有不同的数据透视，请将其设置为新透视
            if (settings.Pivot != layerSetting.pivot)
                importSetting.spriteAlignment = (int)layerSetting.pivot;

            // 枢轴设置是相同的，但自定义，设置向量
            //  else if (settings.Pivot == SpriteAlignment.Custom)
            //	importSetting.spritePivot = settings.PivotVector;
            importSetting.mipmapEnabled = false;
            importSetting.spritePixelsPerUnit = pixelsToUnits;

            // 写入纹理导入设置
            textureImporter.SetTextureSettings(importSetting);

            // 设置纹理设置的其余部分
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.spritePackingTag = settings.PackingTag;
            textureImporter.spriteBorder = spriteBorder;

            //设置srgb
            textureImporter.sRGBTexture = !PSDExportSettings.IsCloseSRGB;

            EditorUtility.SetDirty(textureObj);
            AssetDatabase.WriteImportSettingsIfDirty(assetPath);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            //return (Sprite)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite));
            return;
        }

        private static Texture2D ScaleTextureByMipmap(Texture2D tex, int mipLevel)
        {
            if (mipLevel < 0 || mipLevel > 2)
                return null;
            int width = Mathf.RoundToInt(tex.width / (mipLevel * 2));
            int height = Mathf.RoundToInt(tex.height / (mipLevel * 2));

            // Scaling down by abusing mip maps
            Texture2D resized = new Texture2D(width, height);
            resized.SetPixels32(tex.GetPixels32(mipLevel));
            resized.Apply();
            return resized;
        }
    }
}
