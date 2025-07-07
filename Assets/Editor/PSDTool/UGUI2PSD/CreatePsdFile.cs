using PhotoshopFile;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UGUI2PSD
{
    public class CreatePsdFile
    {
        private enum EColorChannel
        {
            Min = -1,
            A = -1,
            R,
            G,
            B,
            Max = 2,
        }

        public readonly ushort DpiInteger = 72;
        public readonly ushort DpiFraction = 0;

        //图片宽高（默认就填unity的屏幕分辨率）
        private PsdFile psdFile;
        private Ugui2Psd_Setting setting;
        private PrefabLayerInfo prefabLayerInfo;
        private byte[] textPatternBytes;

        public CreatePsdFile(Transform prefab)
        {
            string assetPath = AssetDatabase.GetAssetPath(prefab.gameObject);
            if (!assetPath.Contains("Assets/"))
            {
                Debug.LogError($"{prefab.name}不是预制体根，无法对其进行生成psd操作，请选择根预制体进行操作！");
                return;
            }

            //工具配置路径
            string settingPath = UGUI2PSDTool.SettingPath;
            if (!File.Exists(settingPath))
            {
                UGUI2PSDTool.CreateUgui2PsdSetting();
                Debug.LogError("已生成默认导出配置，请确认配置信息，file：" + settingPath);
                return;
            }

            //加载文本层样板
            string textPatternPath = Application.dataPath + "/" + UGUI2PSDTool.TextPatternPath;
            LoadTextPatternData(textPatternPath);

            Debug.Log("<color=blue>开始进行预制体反向生成PSD文件......</color>");
            LoadSetting(settingPath);

            //解析预制体信息
            prefabLayerInfo = new PrefabLayerInfo(prefab);
            //记录psd信息
            RecordPsdInfo();
            //生成psd文件
            CreateFile();
            //释放资源
            Dispose();
        }

        private void Dispose()
        {
            psdFile = null;
            textPatternBytes = null;
            setting = null;
            prefabLayerInfo = null;
        }

        //加载文本层样板数据
        private void LoadTextPatternData(string path)
        {
            if (File.Exists(path))
            {
                textPatternBytes = File.ReadAllBytes(path);
            }
            else
            {
                Debug.LogError("Failed：加载文字样板失败，将无法保存文本层数据，filePath: " + path);
                throw new System.Exception("文本层数据不存在，加载失败: " + path);
            }
        }

        //加载配置信息
        private void LoadSetting(string path)
        {
            setting = AssetDatabase.LoadAssetAtPath<Ugui2Psd_Setting>(path);
        }

        private void RecordPsdInfo()
        {
            psdFile = new PsdFile();
            SetBaseInfo();
            SetResolutionInfo();
            //SetExtendInfo();
            SetLayers();
        }

        private void CreateFile()
        {
            string outPutPath = Path.Combine(setting.OutputBasePath, prefabLayerInfo.GetPrefabName() + ".psd");
            if (File.Exists(outPutPath))
            {
                File.Delete(outPutPath);
            }
            psdFile.Save(outPutPath, Encoding.Default);
            Debug.Log($"<color=blue>生成psd成功：{outPutPath}</color>");
        }

        private void SetBaseInfo()
        {
            //以下基础字段可写死
            psdFile.ColorMode = PsdColorMode.RGB;
            //psdFile.ChannelCount = 3; //是否需要写死
            psdFile.RowCount = setting.PsdHeight;
            psdFile.ColumnCount = setting.PsdWidth;
            psdFile.ImageCompression = ImageCompression.Raw;
            psdFile.BitDepth = 8;

            psdFile.AbsoluteAlpha = false;
            psdFile.ColorModeData = new byte[0];

            //psdFile.AdditionalInfo = TestPSD.FileAddtionInfos;

            //设置基础信息
            psdFile.BaseLayer.CreateMissingChannels();
            psdFile.BaseLayer.Masks = new MaskInfo();
            psdFile.ChannelCount = (short)psdFile.BaseLayer.Channels.Count;
        }

        private void SetResolutionInfo()
        {
            ResolutionInfo resolutionInfo = new ResolutionInfo();
            resolutionInfo.HDpi = new UFixed16_16(DpiInteger, DpiFraction);
            resolutionInfo.VDpi = new UFixed16_16(DpiInteger, DpiFraction);
            resolutionInfo.HResDisplayUnit = ResolutionInfo.ResUnit.PxPerInch;
            resolutionInfo.HeightDisplayUnit = ResolutionInfo.Unit.Centimeters;
            resolutionInfo.VResDisplayUnit = ResolutionInfo.ResUnit.PxPerInch;
            resolutionInfo.WidthDisplayUnit = ResolutionInfo.Unit.Centimeters;

            psdFile.Resolution = resolutionInfo;
        }

        private void SetExtendInfo()
        {
            ExtendLayer extendLayer = prefabLayerInfo.ExtendLayer;
            RawImageResource rawImage = new RawImageResource(ResourceID.MacPrintInfo, "UserExtend", extendLayer.PrefabData);
            psdFile.ImageResources.Add(rawImage);
        }

        private void SetLayers()
        {
            List<Layer> allLayer = new List<Layer>();
            List<PrefabLayerBase> prefabLayers = prefabLayerInfo.GetPrefabLayerList();
            foreach (PrefabLayerBase item in prefabLayers)
            {
                Layer layer = new Layer(psdFile);
                layer.Opacity = item.Opacity;
                layer.Name = item.LayerName;
                layer.Visible = item.Visible;
                layer.Rect = GetPsRect(item.PsRect, item.trans);
                layer.CreateMissingChannels();

                //添加layerinfo
                LayerInfo info = null;
                switch (item.LayerType)
                {
                    case EPrefabLayerType.GroupStart:
                        info = new LayerSectionInfo(LayerSectionType.OpenFolder);
                        break;
                    case EPrefabLayerType.Image:
                        ImageLayer imgLayer = item as ImageLayer;
                        if (imgLayer != null)
                            SetImageLayerData(layer, imgLayer.pixels);
                        break;
                    case EPrefabLayerType.Text:
                        info = new LayerText(textPatternBytes, Encoding.Default);
                        TextLayer txtLayer = item as TextLayer;
                        if (txtLayer != null)
                            SetTextData(info, txtLayer);
                        break;
                    case EPrefabLayerType.GroupEnd:
                        info = new LayerSectionInfo(LayerSectionType.SectionDivider);
                        break;
                    case EPrefabLayerType.Extend:
                        ExtendLayer extendLayer = item as ExtendLayer;
                        if (extendLayer != null)
                        {
                            layer.Name = extendLayer.Key;
                            SetExtendLayerAsImage(layer, extendLayer.PrefabData);
                        }
                        break;
                    default:
                        break;
                }

                if (info != null)
                    layer.AdditionalInfo.Add(info);

                allLayer.Add(layer);
            }

            ////写入额外信息层
            //Layer extendLayer = GetExtendLayer();
            //if (extendLayer != null)
            //{
            //    allLayer.Insert(1, extendLayer);    //作为ps的第一个child
            //}

            //要翻转一下，psd默认就是翻转的
            allLayer.Reverse();
            psdFile.Layers = allLayer;
        }

        private Layer GetExtendLayer()
        {
            ExtendLayer item = prefabLayerInfo.ExtendLayer;
            if (item.IsResolveSuccess)
            {
                Layer layer = new Layer(psdFile);
                layer.Opacity = item.Opacity;
                layer.Name = item.Key;
                layer.Visible = item.Visible;
                layer.Rect = new Rect(0, 0, setting.PsdWidth, setting.PsdHeight);
                //layer.Rect = GetPsRect(item.PsRect);
                layer.CreateMissingChannels();
                SetExtendLayerAsImage(layer, item.PrefabData);
                return layer;
            }
            else
                return null;
        }

        //获取ps的坐标轴
        private Rect GetPsRect(Rect unityRect, Transform tran)
        {
            float x = tran.position.x - unityRect.width * 0.5f + setting.CanvasX;
            float y = tran.position.y + unityRect.height * 0.5f + setting.CanvasY;
            y = setting.PsdHeight - y;
            return new Rect(x, y, unityRect.width, unityRect.height);
        }

        //额外层数据作为图片存储
        private void SetExtendLayerAsImage(Layer layer, byte[] data)
        {
            foreach (Channel item in layer.Channels)
            {
                item.ImageCompression = ImageCompression.Raw;
            }
            //存入数据
            UGUI2PSDTool.SaveDataToChannel(layer, data);
        }

        #region 文本层数据处理
        //设置文本数据
        private void SetTextData(LayerInfo layer, TextLayer mTxtLayer)
        {
            LayerText lyText = layer as LayerText;
            if (lyText != null)
            {
                lyText.SetText(mTxtLayer.Text);     //文字文本
                lyText.SetFontSize(mTxtLayer.FontSize);     //文字字体大小
                //lyText.SetFauxBold(mTxtLayer.FauxBold);       //加粗和斜体属于ps加效果脚本，要改很多参数，暂时不做
                //lyText.SetFauxItalic(mTxtLayer.FauxItalic);   //加粗和斜体属于ps加效果脚本，要改很多参数，暂时不做
                //lyText.SetOutlineWidth(mTxtLayer.OutlineWidth);   //描边暂时加不了，会有额外效果层数据
                lyText.SetFillColor(mTxtLayer.FillColor);   //文字颜色
                //lyText.SetStrokeColor(mTxtLayer.StrokeColor); //描边颜色暂时加不了，会有额外效果层数据
            }
        }

        //颜色转为uint
        private uint Color2Uint(Color color)
        {
            uint a = ((uint)(color.a * 255f) << 24) & 0xFF000000U;
            uint r = ((uint)(color.r * 255f) << 16) & 0xFF0000U;
            uint g = ((uint)(color.g * 255f) << 8) & 0xFF00U;
            uint b = (uint)(color.b * 255f) & 0xFFU;
            uint result = a | r | g | b;
            return result;
        }

        #endregion

        #region 图片层数据处理
        private void SetImageLayerData(Layer layer, Color32[] pixels)
        {
            foreach (Channel item in layer.Channels)
            {
                item.ImageCompression = ImageCompression.Rle;
            }

            //写入颜色数据
            int pixelCount = pixels.Length;
            int texWidth = (int)layer.Rect.width;
            int texHeight = (int)layer.Rect.height;
            //if (texWidth * texHeight != pixelCount)
            //{
            //    Debug.LogError("颜色数据有缺失：" + layer.Name);
            //}
            //else
            {
                for (int i = 0; i < pixelCount; i++)
                {
                    //int flipIndex = GetFlip_Top2Bottom_Optimized(texWidth, pixelCount, i);
                    int flipIndex = GetFlip_Top2Bottom(texWidth, texHeight, i);
                    if (flipIndex >= 0 && flipIndex < pixelCount)
                        SavePixel(layer, i, pixels[flipIndex]);
                }
            }
        }

        //设置像素信息
        private void SavePixel(Layer layer, int index, Color32 col)
        {
            int a = (int)EColorChannel.A;
            if (layer.Channels.ContainsId(a))
            {
                SavePixel(layer.Channels.GetId(a), index, col.a);
            }

            int r = (int)EColorChannel.R;
            if (layer.Channels.ContainsId(r))
            {
                SavePixel(layer.Channels.GetId(r), index, col.r);
            }

            int g = (int)EColorChannel.G;
            if (layer.Channels.ContainsId(g))
            {
                SavePixel(layer.Channels.GetId(g), index, col.g);
            }

            int b = (int)EColorChannel.B;
            if (layer.Channels.ContainsId(b))
            {
                SavePixel(layer.Channels.GetId(b), index, col.b);
            }
        }

        private void SavePixel(Channel channel, int index, int value)
        {
            channel.ImageData[index] = (byte)value;
        }

        /// <summary>
        /// 上下翻转对称下标
        /// </summary>
        /// <returns></returns>
        private int GetFlip_Top2Bottom(int W, int H, int i)
        {
            int srcRow = i / W;
            int remain = i % W; //余数
            int destRow = H - 1 - srcRow;
            int flipIndex = destRow * W + remain;
            return flipIndex;
        }

        //高效版翻转(采用轴对称)
        private int GetFlip_Top2Bottom_Optimized(int W, int pixelCount, int i)
        {
            int mod = i % W;
            int n = (i - mod) + (W - 1 - mod); //取得这一行的左右对称的下标
            int flipIndex = pixelCount - 1 - n;
            return flipIndex;
        }
        #endregion
    }
}