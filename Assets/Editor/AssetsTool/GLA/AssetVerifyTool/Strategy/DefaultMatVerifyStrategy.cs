/*******************************************************************
** 文件名: DefaultMatVerifyStrategy.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 梁成 
** 日  期: 2018/10/25
** 版  本: 1.0
** 描  述: 资源检测工具—排查默认材质
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
	public class DefaultMatVerifyStrategy : VerifyStrategyBase 
	{
		/// <summary>
		/// 加载检测规则配置
		/// </summary>
		/// <param name="ruleConfigPath">配置路径</param>
		/// <returns></returns>
		public override bool LoadVerifyRuleConfig(string ruleConfigPath)
		{
			return true;
		}

		/// <summary>
		/// 检测资源
		/// </summary>
		/// <param name="asset">检测资源对象</param>
		/// <returns></returns>
		public override VerifyResult VerifyAsset(UnityEngine.Object asset)
		{
			if(asset == null)
			{
				Debug.LogError("[资源检测工具]检测对象为空！");
				return null;
			}

			VerifyResult resultData = new VerifyResult();

			string assetPath = AssetDatabase.GetAssetPath(asset);
			resultData.FillAssetBaseInfo(asset.name, assetPath);

            //errorMsg = "";
            if(asset is Material)
            {
                Material mat = asset as Material;
                //Debug.Log("material="+mat.name);
                string shaderName = mat.shader.name;                 
                if(mat.name=="Default-Material")
                {
					resultData.SetResultType(VerifyResultType.Failed);
				    resultData.AddOutputInfo(LogLevel.Error,asset.name+"材质使用了默认材质!材质名:" + mat.name + "---Shader名:" + shaderName, asset);
                }
            }
            else if (asset is GameObject)
            {                
                GameObject go = asset as GameObject;                
                Renderer[] renders=go.GetComponentsInChildren<Renderer>();
                foreach (Renderer render in renders)
                {
                    foreach (Material mat in render.sharedMaterials)
                    {
                        if(mat!=null && mat.name=="Default-Material")
                        {
                            //errorMsg = OutputMsg(ResCheckMsgLevel.Error, asset.name+"材质使用了默认材质!材质名:" + mat.name + "；", asset, asset.name);
							resultData.SetResultType(VerifyResultType.Failed);
				    		resultData.AddOutputInfo(LogLevel.Error,asset.name+"材质使用了默认材质!材质名:" + mat.name, asset);
                        }
                    }
                }
            }
            else if (asset is Renderer)
            {
                Renderer render = asset as Renderer;
                foreach (Material mat in render.sharedMaterials)
                {
                    if(mat!=null && mat.name=="Default-Material")
                    {
                        //errorMsg = OutputMsg(ResCheckMsgLevel.Error, asset.name+"材质使用了默认材质!材质名:" + mat.name + "；", asset, asset.name);
						resultData.SetResultType(VerifyResultType.Failed);
				    	resultData.AddOutputInfo(LogLevel.Error,asset.name+"材质使用了默认材质!材质名:" + mat.name, asset);
                    }
                }
            }

            return resultData;
		}
	}
}
