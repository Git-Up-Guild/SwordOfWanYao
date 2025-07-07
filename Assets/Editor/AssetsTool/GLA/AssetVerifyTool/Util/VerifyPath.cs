/*******************************************************************
** 文件名: VerifyPath.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 昔文博 
** 日  期: 2018/7/10
** 版  本: 1.0
** 描  述: 资源检测工具—路径管理  
** 应  用:     

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace XGameEditor.AssetVerifyTools
{
    public static class VerifyPath
    {
        public static readonly string CommonModelRuleConfig = "ModelRule_Common.csv"; 
        public static readonly string CommonTextureRuleConfig = "TextureRule_Common.csv";
        public static readonly string CommonAnimationRuleConfig = "AnimationRule_Common.csv";
        public static readonly string CommonMaterialRuleConfig = "MaterialRule_Common.csv";
        public static readonly string CommonAudioRuleConfig = "AudioRule_Common.csv";

        public static readonly string ConfigRootPath = "Editor/AssetVerifyTool/Config/";

        public static string GetRuleConfigRootPath()
        {
            return Application.dataPath + "/" + ConfigRootPath;
        }

        public static string GetTypeRulePath(VerifyAssetType assetType)
        {
            return GetRuleConfigRootPath() + assetType.ToString() + "/";
        }

        public static string GetModelCommonRuleConfigPath()
        {
            return GetTypeRulePath(VerifyAssetType.Model) + CommonModelRuleConfig;
        }

        public static string GetTextureCommonRuleConfigPath()
        {
            return GetTypeRulePath(VerifyAssetType.Texture) + CommonTextureRuleConfig;
        }

        public static string GetAnimationCommonRuleConfigPath()
        {
            return GetTypeRulePath(VerifyAssetType.Animation) + CommonAnimationRuleConfig;
        }

        public static string GetMaterialCommonRuleConfigPath()
        {
            return GetTypeRulePath(VerifyAssetType.Material) + CommonMaterialRuleConfig;
        }

        public static string GetAudioCommonRuleConfigPath()
        {
            return GetTypeRulePath(VerifyAssetType.Audio) + CommonAudioRuleConfig;
        }
    }
}
