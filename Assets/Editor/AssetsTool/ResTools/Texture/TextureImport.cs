using UnityEngine;
using System.Collections;
using UnityEditor;
using TextureFMT;

#if !UNITY_PACKING

/// <summary>
/// 纹理导入设置
/// </summary>
public class TextureImport : AssetPostprocessor
{
    private const string EffectTexturePatch = "g_artist";
    private const string ProjectorPatchKey = "tiehua";
    private const string CookiePatchKey = "cookie";
    private const string NormalPatchKey = "normalmap";
    private TextureImporter textureImporter = null;
    void OnPreprocessTexture()
    {
        textureImporter = assetImporter as TextureImporter;

        if (textureImporter.textureType != TextureImporterType.Default)
        {
            return;
        }

        string path = textureImporter.assetPath;
        path = path.ToLower();
        /*
        if (path.Contains(EffectTexturePatch))
        {
            if (path.Contains(ProjectorPatchKey)) //贴花
            {
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.isReadable = false;
                textureImporter.mipmapEnabled = false;
                //textureImporter.borderMipmap = true;
                //textureImporter.textureFormat = TextureImporterFormat.ARGB32;
                //textureImporter.wrapMode = TextureWrapMode.Clamp;
            }
            else if (path.Contains(CookiePatchKey))//cookie贴图
            {
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.isReadable = false;
                textureImporter.mipmapEnabled = false;
                textureImporter.textureFormat = TextureImporterFormat.Alpha8;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.grayscaleToAlpha = true;
            }
            else //普通贴图
            {
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.isReadable = false;
                textureImporter.mipmapEnabled = false;
            }
            
         
        }
        */

        //设置压缩格式
        TextureFMTSetting.ProcessTexture(path);



    }
}
#endif