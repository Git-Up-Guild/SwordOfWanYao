using System;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using XClient.Client;
using XGameEditor;
using TMPro;
using UnityEngine.SceneManagement;
using XClient.Scripts;

namespace subjectnerdagreement.psdexport
{
    public class PsdBuilder
    {
        #region convenience functions
        public static void BuildUiImages(GameObject root, PSDLayerGroupInfo group,
                                    PSDExportSettings settings, PSDFileInfo fileInfo,
                                    SpriteAlignment createAlign)
        {
            BuildPsd(root, group, settings, fileInfo,
                    createAlign, new UiImgConstructor());
        }

        public static void BuildSprites(GameObject root, PSDLayerGroupInfo group,
                                        PSDExportSettings settings, PSDFileInfo fileInfo,
                                        SpriteAlignment createAlign)
        {
            BuildPsd(root, group, settings, fileInfo,
                    createAlign, new SpriteConstructor());
        }
        #endregion

        #region General handler
        public static void BuildPsd(GameObject root, PSDLayerGroupInfo group,
                                    PSDExportSettings settings, PSDFileInfo fileInfo,
                                    SpriteAlignment align, IPsdConstructor constructor)
        {

            // Run the export on non exported layers
            PSDExporter.Export(settings, fileInfo, true);

            // Find all the layers being exported
            var exportLayers = PSDExporter.GetExportLayers(settings, fileInfo);

            // Stores the root object for each encountered group
            Dictionary<PSDLayerGroupInfo, GameObject> groupHeaders = new Dictionary<PSDLayerGroupInfo, GameObject>();

            // Store the last parent, for traversal
            GameObject lastParent = root;

            GameObject rootBase = null;

            int groupVisibleMask = 1;
            int groupDepth = 0;

            // Loop through all the layers of the PSD file
            // backwards so they appear in the expected order
            // Going through all the layers, and not just the exported layers
            // so that the groups can be setup
            for (int i = group.end; i >= group.start; i--)
            {
                if (i >= fileInfo.LayerVisibility.Length)
                {
                    break;
                }

                // Skip if layer is hidden
                if (fileInfo.LayerVisibility[i] == false)
                    continue;

                var groupInfo = fileInfo.GetGroupByLayerIndex(i);
                bool inGroup = groupInfo != null;

                // Skip if layer belongs to a hidden group
                if (inGroup && groupInfo.visible == false)
                    continue;

                // When inside a group...
                if (inGroup)
                {
                    // Inverted because starting backwards
                    bool startGroup = groupInfo.end == i;
                    bool closeGroup = groupInfo.start == i;

                    // Go up or down group depths
                    if (startGroup)
                    {
                        groupDepth++;
                        groupVisibleMask |= ((groupInfo.visible ? 1 : 0) << groupDepth);
                    }
                    if (closeGroup)
                    {
                        // Reset group visible flag when closing group
                        groupVisibleMask &= ~(1 << groupDepth);
                        groupDepth--;
                    }

                    // First, check if parents of this group is visible in the first place
                    bool parentVisible = true;
                    for (int parentMask = groupDepth - 1; parentMask > 0; parentMask--)
                    {
                        bool isVisible = (groupVisibleMask & (1 << parentMask)) > 0;
                        parentVisible &= isVisible;
                    }
                    // Parents not visible, continue to next layer
                    if (!parentVisible)
                        continue;

                    // Finally, check if layer being processed is start/end of group
                    if (startGroup || closeGroup)
                    {
                        // If start or end of the group, call HandleGroupObject
                        // which creates the group layer object and assignment of lastParent
                        HandleGroupObject(groupInfo, groupHeaders,
                                        startGroup, constructor, ref lastParent);

                        // A bunch of book keeping needs to be done at the start of a group
                        if (startGroup)
                        {
                            // If this is the start of the group being constructed
                            // store as the rootBase
                            if (i == group.end)
                            {
                                rootBase = lastParent;
                            }
                            //lastParent.transform.SetAsFirstSibling();
                        }

                        // Start or end group doesn't have visible sprite object, skip to next layer
                        continue;
                    }
                } // End processing of group start/end

                // If got to here, processing a visual layer

                // Skip if the export layers list doesn't contain this index
                if (exportLayers.Contains(i) == false)
                    continue;

                // If got here and root base hasn't been set, that's a problem
                if (rootBase == null)
                {
                    throw new Exception("Trying to create image layer before root base has been set");
                }

                // Get layer info
                Layer layer = settings.Psd.Layers[i];

                //导入信息
                ImportResInfo resInfo;

                //GameObject对象名称
                string gameObjectName;

                //layer导出设置
                PSDExportSettings.LayerSetting layerSetting;
                if (settings.layerSettings.TryGetValue(i, out layerSetting))
                {
                    resInfo = layerSetting.importInfo;
                    gameObjectName = resInfo.importName;
                    gameObjectName = PinYinHelper.Convert(gameObjectName);  //中文转为拼音
                }
                else
                {
                    gameObjectName = GetGameObjectName(layer.Name);
                    if (gameObjectName.Length == 0)
                        continue;

                    gameObjectName = PinYinHelper.Convert(gameObjectName);  //中文转为拼音
                    resInfo = PSDSetting.Instance.GetImportResInfo(gameObjectName);
                    gameObjectName = resInfo.importName;
                }

                //是需要跳过的图层
                if (resInfo.isSkip)
                    continue;

                GameObject spriteObject = constructor.CreateGameObject(gameObjectName, lastParent);

                // Reparent created object to last parent
                if (lastParent != null)
                {
                    spriteObject.transform.SetParent(lastParent.transform, false);
                }

                Vector2 spritePivot = GetPivot(SpriteAlignment.Center);

                if (layer.IsText)
                {
                    if (PSDExportSettings.IsUseTextMeshPro)
                    {
                        AddTextMeshProComponent(layer, spriteObject, resInfo);
                    }
                    else
                    {
                        AddNormalTextComponent(layer, spriteObject, resInfo);
                    }
                }
                else
                {
                    //表示是图片层
                    string sprPath = PSDExporter.GetFileSavePathBySetting(settings, i);
                    if (PSDExporter.Res2CommonDic.ContainsKey(sprPath))
                    {
                        sprPath = PSDExporter.Res2CommonDic[sprPath];
                    }
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(sprPath);

                    TextureImporter sprImporter = (TextureImporter)AssetImporter.GetAtPath(sprPath);
                    TextureImporterSettings sprSettings = new TextureImporterSettings();

                    if (sprImporter == null)
                    {
                        string gameObjectPath = XGame.Utils.Utility.GetGameObjectFullPath(spriteObject);
                        Debug.LogError("导入文理失败：图层路径=" + gameObjectPath + ",  图片保存路径=" + sprPath);
                        continue;
                    }

                    sprImporter.ReadTextureSettings(sprSettings);

                    // Add components to the sprite object for the visuals
                    constructor.AddComponents(i, spriteObject, sprite, sprSettings, settings);

                    //添加交互处理
                    if (resInfo != null)
                    {
                        AddInteractions(spriteObject, resInfo, layer);
                    }else
                    {
                        Image img = spriteObject.GetComponent<Image>();
                        if(null!= img)
                        {
                            img.raycastTarget = false;
                        }
                    }

                    // Reposition the sprite object according to PSD position
                    spritePivot = GetPivot(sprSettings);
                }

                Vector3 layerPos = constructor.GetLayerPosition(layer.Rect, spritePivot, settings.PixelsToUnitSize);
                // reverse y axis
                layerPos.y = fileInfo.height - layerPos.y;

                // Scaling factor, if sprites were scaled down
                float posScale = 1f;
                switch (settings.ScaleBy)
                {
                    case 1:
                        posScale = 0.5f;
                        break;
                    case 2:
                        posScale = 0.25f;
                        break;
                }
                layerPos *= posScale;

                // Sprite position is based on root object position initially
                Transform spriteT = spriteObject.transform;
                spriteT.position = layerPos;
            } // End layer loop

            if (rootBase)
                ReverseGameobjectLayer(rootBase.transform);

            //创建物体完毕后，挂载相关脚本
            string uiRootName = group.name;
            Transform uiRoot = root.transform.Find(uiRootName);
            if (uiRoot)
            {
                if (fileInfo.UserLayerIndex != -1)
                {
                    Layer userLayer = settings.Psd.Layers[fileInfo.UserLayerIndex];
                    //UGUI2PSD.UGUI2PSDTool.UpdateGameObjectByPsdLayer(uiRoot.gameObject, userLayer);
                }
            }
            else
                Debug.LogError("找不到物体：" + uiRootName);
        } // End BuildPsd()

        private static void ReverseGameobjectLayer(Transform tran)
        {
            int length = tran.childCount;
            for (int i = 0; i < length; i++)
            {
                Transform child = tran.GetChild(0);
                child.SetSiblingIndex(length - 1 - i);

                if (child.childCount > 0)
                    ReverseGameobjectLayer(child);
            }
        }

        //[MenuItem("Tools/测试预置体反序")]
        private static void Test()
        {
            GameObject go = Selection.activeGameObject;
            if (go)
                ReverseGameobjectLayer(go.transform);
        }

        //添加Text文本
        private static void AddNormalTextComponent(Layer layer, GameObject spriteObject, ImportResInfo resInfo)
        {
            //是文本层
            var layerText = layer.LayerText;
            Text text = spriteObject.AddComponent<Text>();
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.alignment = TextAnchor.MiddleLeft;     //默认居中左对齐

            //处理字体
            AddTextFont(text, layer);

            text.rectTransform.sizeDelta = new Vector2(layer.Rect.width, layer.Rect.height);
            text.text = layerText.Text.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd('\n'); //去掉结尾的换行

            text.fontSize = (int)(layerText.FontSize);
            //text.rectTransform.SetAsFirstSibling();
            FontStyle fontStyle = FontStyle.Normal;
            if (layerText.FauxBold)
            {
                fontStyle |= FontStyle.Bold;
            }
            if (layerText.FauxItalic)
            {
                fontStyle |= FontStyle.Italic;
            }
            text.fontStyle = fontStyle;

            float a = ((layerText.FillColor & 0xFF000000U) >> 24) / 255f;
            float r = ((layerText.FillColor & 0xFF0000U) >> 16) / 255f;
            float g = ((layerText.FillColor & 0xFF00U) >> 8) / 255f;
            float b = (layerText.FillColor & 0xFFU) / 255f;
            text.color = new Color(r, g, b, a);

            //指定了特定的文本风格
            if (resInfo != null && resInfo.textStyle != null)
            {
                AddTextStyle(text, resInfo.textStyle);
            }
            //else
            //{
            //    AddTextStyle(text, PSDSetting.Instance.defaultTextStyle);
            //}
        }

        //添加TextMeshPro文本
        private static void AddTextMeshProComponent(Layer layer, GameObject spriteObject, ImportResInfo resInfo)
        {
            //是文本层
            var layerText = layer.LayerText;
            TextMeshProUGUI text = spriteObject.AddComponent<TextMeshProUGUI>();
            text.enableWordWrapping = false;
            text.alignment = TextAlignmentOptions.MidlineLeft;     //默认居中左对齐
            text.raycastTarget = false;

            //处理字体
            AddTextFont(text, layer);

            text.rectTransform.sizeDelta = new Vector2(layer.Rect.width, layer.Rect.height);
            text.text = layerText.Text.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd('\n'); //去掉结尾的换行
            
            double fontSize = layerText.FontSize * layer.LayerText.Transform.M11;//(layerText.FontSize / 96) * 72;
            text.fontSize = (float)Math.Round(fontSize) ;// (int)(layerText.FontSize);
            
            // Debug.LogError("原始大小 "+ layerText.FontSize +text.text + " 设置字体大小:  " + fontSize);

            FontStyles fontStyle = FontStyles.Normal;
            if (layerText.FauxBold)
            {
                fontStyle |= FontStyles.Bold;
            }
            if (layerText.FauxItalic)
            {
                fontStyle |= FontStyles.Italic;
            }
            text.fontStyle = fontStyle;

            float a = ((layerText.FillColor & 0xFF000000U) >> 24) / 255f;
            float r = ((layerText.FillColor & 0xFF0000U) >> 16) / 255f;
            float g = ((layerText.FillColor & 0xFF00U) >> 8) / 255f;
            float b = (layerText.FillColor & 0xFFU) / 255f;
            text.color = new Color(r, g, b, a);

            //指定了特定的文本风格
            if (resInfo != null && resInfo.textStyle != null)
            {
                AddTextStyle(text, resInfo.textStyle);
            }
        }

        //添加文本字体
        private static void AddTextFont(Text text, Layer layer)
        {
            string fontName = layer.LayerText.FontName;
            foreach (var f in PSDSetting.Instance.fontsMap)
            {
                if (fontName == f.sourceName)
                {
                    string fontPath = PSDSetting.Instance.fontDir + "/" + f.mapName + ".ttf";
                    fontPath = XGameEditorUtilityEx.BaseOnAssetsPath(fontPath);
                    Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
                    if (font)
                        text.font = font;
                    else
                        Debug.LogError("字体加载错误：" + fontPath);
                    return;
                }
            }

            Debug.LogError(layer.Name + " - 字体配置里找不到字体名：" + fontName);
        }

        //添加文本字体
        private static void AddTextFont(TextMeshProUGUI text, Layer layer)
        {
            string fontName = layer.LayerText.FontName;
            foreach (var f in PSDSetting.Instance.fontsMap)
            {
                if (fontName == f.sourceName)
                {
                    if(f.sdfStyleSettings != null)
                    {
                        text.font = f.sdfStyleSettings.sdfFont;
                    }
                    else
                    {
                        Debug.LogError($"{layer.Name} - 使用了不支持的字体：text={layer.LayerText.Text}, font={fontName}");
                    }
                    return;
                }
            }

            Debug.LogError(layer.Name + " - 字体配置里找不到字体名：" + fontName);
        }

        //添加文本风格
        private static void AddTextStyle(Text text, ImportTextStyle style)
        {
            if (null != style)
            {
                if (style.enableOut)
                {
                    Outline outline = text.gameObject.AddComponent<Outline>();
                    outline.effectDistance = new Vector2(style.outX, style.outY);
                    outline.effectColor = style.outColor;
                }

                if (style.enableShadow)
                {
                    Shadow shadow = text.gameObject.AddComponent<Shadow>();
                    shadow.effectDistance = new Vector2(style.shadowX, style.shadowY);
                    shadow.effectColor = style.shadowColor;
                }
            }
        }

        private static void AddTextStyle(TextMeshProUGUI text, ImportTextStyle style)
        {
            if (null != style)
            {
                var info = PSDSetting.Instance.GetImportFontMapByMapSDFName(text.font.name);
                if (info == null)
                {
                    Debug.LogError("通过映射字体名称查找字体映射配置失败：mapSDFName=" + text.font.name);
                    return;
                }

                var styleSetting = info.sdfStyleSettings.GetStyle(style.name);
                if (styleSetting != null)
                {
                    text.fontMaterial = styleSetting.fontMat;
                    if(styleSetting.colorGrandient != null)
                    {
                        text.enableVertexGradient = true;
                        text.colorGradientPreset = styleSetting.colorGrandient;
                    }
                }
                else
                    Debug.LogError("字体材质加载失败：font=" + text.font.name + ", style=" + style.name);
            }
        }

        //添加交互操作
        private static void AddInteractions(GameObject spriteObject, ImportResInfo resInfo, Layer layer)
        {
            if (resInfo.propType == EImportPropType.Button)
            {
                //给按钮添加上图片
                Button btn = spriteObject.AddComponent<Button>();
                btn.targetGraphic = spriteObject.GetComponent<Image>();
            }else
            {
                Image img = spriteObject.GetComponent<Image>();
                if (null != img)
                {
                    img.raycastTarget = false;
                }
            }
           

            if ((resInfo.funcTypeFlag & EImportMarkType.Lucency) == EImportMarkType.Lucency)
            {
                //有透明度调整
                spriteObject.GetComponent<Image>().color = new Color(1, 1, 1, (float)layer.Opacity / 255);
            }
        }

        //rgba转换成color对象
        public static Color HexToColor(string hex)
        {
            byte br = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte bg = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte bb = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte cc = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            float r = br / 255f;
            float g = bg / 255f;
            float b = bb / 255f;
            float a = cc / 255f;
            return new Color(r, g, b, a);
        }


        private static string GetGameObjectName(string layerName)
        {
            string realName = layerName;
            string[] names = layerName.Split('|');
            if (names.Length > 1)
            {
                realName = names[1];
            }
            else
            {
                realName = Path.GetFileName(layerName);
            }

            return realName;
        }

        private static void HandleGroupObject(PSDLayerGroupInfo groupInfo,
                                    Dictionary<PSDLayerGroupInfo, GameObject> groupHeaders,
                                    bool startGroup, IPsdConstructor constructor,
                                    ref GameObject lastParent)
        {
            if (startGroup)
            {
                string groupName = PinYinHelper.Convert(groupInfo.name);
                GameObject groupRoot = constructor.CreateGameObject(groupName, lastParent);
                constructor.HandleGroupOpen(groupRoot);

                lastParent = groupRoot;
                groupHeaders.Add(groupInfo, groupRoot);
                return;
            }

            // If not startGroup, closing group
            var header = groupHeaders[groupInfo].transform;
            if (header.parent != null)
            {
                constructor.HandleGroupClose(lastParent);

                lastParent = groupHeaders[groupInfo].transform.parent.gameObject;
            }
            else
            {
                lastParent = null;
            }
        }
        #endregion

        #region Public APIs

        public static Vector3 CalculateLayerPosition(Rect layerSize, Vector2 layerPivot)
        {
            Vector3 layerPos = Vector3.zero;
            layerPos.x = ((layerSize.width * layerPivot.x) + layerSize.x);
            layerPos.y = ((layerSize.height * layerPivot.y) + layerSize.y);
            return layerPos;
        }

        public static Vector2 GetPivot(SpriteAlignment spriteAlignment)
        {
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            if (spriteAlignment == SpriteAlignment.TopLeft ||
                spriteAlignment == SpriteAlignment.LeftCenter ||
                spriteAlignment == SpriteAlignment.BottomLeft)
            {
                pivot.x = 0f;
            }
            if (spriteAlignment == SpriteAlignment.TopRight ||
                spriteAlignment == SpriteAlignment.RightCenter ||
                spriteAlignment == SpriteAlignment.BottomRight)
            {
                pivot.x = 1;
            }
            if (spriteAlignment == SpriteAlignment.TopLeft ||
                spriteAlignment == SpriteAlignment.TopCenter ||
                spriteAlignment == SpriteAlignment.TopRight)
            {
                pivot.y = 1;
            }
            if (spriteAlignment == SpriteAlignment.BottomLeft ||
                spriteAlignment == SpriteAlignment.BottomCenter ||
                spriteAlignment == SpriteAlignment.BottomRight)
            {
                pivot.y = 0;
            }
            return pivot;
        }

        public static Vector2 GetPivot(TextureImporterSettings sprSettings)
        {
            SpriteAlignment align = (SpriteAlignment)sprSettings.spriteAlignment;
            if (align == SpriteAlignment.Custom)
                return sprSettings.spritePivot;
            return GetPivot(align);
        }
        #endregion
    }
}